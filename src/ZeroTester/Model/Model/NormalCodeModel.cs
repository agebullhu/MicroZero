// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 配置:CodeRefactor-Agebull.CodeRefactor.CodeAnalyze.Application
// 建立:2014-11-20
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Agebull.EntityModel.Config;
using Agebull.Common.Mvvm;
using Agebull.EntityModel.RobotCoder;

#endregion

namespace Agebull.EntityModel.Designer
{
    public class NormalCodeModel : DesignModelBase
    {

        /// <summary>
        ///     分类
        /// </summary>
        public NormalCodeModel()
        {
            EditorName = "Code";
        }

        #region 操作命令

        /// <summary>
        /// 生成命令对象
        /// </summary>
        /// <param name="commands"></param>
        protected override void CreateCommands(NotificationList<CommandItemBase> commands)
        {
            commands.Add(new CommandItem
            {
                IsButton = true,
                Action = arg => DoMomentCode(),
                Caption = "生成代码片断",
                Image = Application.Current.Resources["img_file"] as ImageSource
            });

            commands.Add(new CommandItem
            {
                IsButton = true,
                Action = arg => CopyCode(),
                Caption = "复制代码",
                Image = Application.Current.Resources["img_file"] as ImageSource
            });
            foreach (var builder in ProjectBuilder.Builders.Values)
            {
                var b = builder();
                commands.Add(new ProjectCodeCommand(builder)
                {
                    Caption = b.Caption,
                    IconName = b.Icon,
                    OnCodeSuccess = OnCodeSuccess
                }.ToCommand(null));
            }
        }
        #endregion

        #region 文件代码

        /// <summary>
        /// 代码命令树根
        /// </summary>
        public TreeRoot FileTreeRoot { get; } = new TreeRoot();

        internal WebBrowser Browser;


        private void OnCodeSuccess(Dictionary<string, string> fields)
        {
            FileTreeRoot.SelectItemChanged -= OnFileSelectItemChanged;
            FileTreeRoot.Items.Clear();
            if (fields == null)
                return;
            int first = SolutionConfig.Current.RootPath == null ? 0 : SolutionConfig.Current.RootPath.Length;
            foreach (var file in fields)
            {
                string name = Path.GetFileName(file.Key);
                string path = Path.GetDirectoryName(file.Key);
                var folder = path?.Substring(first,path.Length - first) ?? "未知目录";
                folder= folder.Trim('\\', '/');
                var item = FileTreeRoot.Items.FirstOrDefault(p=>p.Name == folder);
                if (item == null)
                {
                    FileTreeRoot.Items.Add(item = new TreeItem(file.Key)
                    {
                        Header = folder,
                        Name = folder,
                        IsExpanded = true,
                        SoruceTypeIcon = Application.Current.Resources["tree_Folder"] as BitmapImage
                    });
                }
                item.Items.Add(new TreeItem<string>(file.Value)
                {
                    Header = name,
                    Name = name,
                    Tag = Path.GetExtension(file.Key)?.Trim('.'),
                    SoruceTypeIcon = Application.Current.Resources["img_code"] as BitmapImage
                });
            }
            
           ViewIndex = 1;
            FileTreeRoot.SelectItemChanged += OnFileSelectItemChanged;
        }

        private void OnFileSelectItemChanged(object sender, EventArgs e)
        {
            var value = sender as TreeItem<string>;
            _codeType = value?.Tag ?? ".cs";
            switch (_codeType.ToLower().Trim('.'))
            {
                case "h":
                    _codeType = "cpp";
                    break;
                case "aspx":
                case "htm":
                case "html":
                    _codeType = "xml";
                    break;
            }
            ExtendCode = value?.Model;
        }

        #endregion

        #region 树形菜单
        /// <summary>
        /// 代码命令树根
        /// </summary>
        public TreeRoot MomentTreeRoot { get; } = new TreeRoot();

        /// <summary>
        /// 初始化
        /// </summary>
        protected override void DoInitialize()
        {
            base.DoInitialize();

            foreach (var clasf in MomentCoder.Coders)
            {
                if (clasf.Value == null || clasf.Value.Count == 0)
                    continue;
                TreeItemBase parent = new TreeItem(clasf.Key)
                {
                    IsExpanded = false,
                    SoruceTypeIcon = Application.Current.Resources["tree_Folder"] as BitmapImage
                };
                MomentTreeRoot.Items.Add((TreeItem)parent);
                foreach (var item in clasf.Value)
                {
                    parent.Items.Add(new TreeItem<CoderDefine>(item.Value)
                    {
                        Header = item.Key,
                        SoruceTypeIcon = Application.Current.Resources["img_code"] as BitmapImage
                    });
                }
            }
            MomentTreeRoot.SelectItemChanged += OnMomentSelectItemChanged;
        }
        private void OnMomentSelectItemChanged(object sender, EventArgs e)
        {
            if (!(sender is TreeItem<CoderDefine> value))
                return;
            _codeType = value.Model.Lang;
            MomentCodeModel = value.Model.Func;
            if (MomentCodeModel != null)
            {
                DoMomentCode();
            }
            else
            {
                ExtendCode = "无生成器";
            }
        }

        /// <summary>
        /// 选中的代码片断对象的方法体
        /// </summary>
        private Func<ConfigBase, string> MomentCodeModel;

        private int _ViewIndex;
        public int ViewIndex
        {
            get => _ViewIndex;
            set
            {
                if (_ViewIndex == value)
                    return;
                _ViewIndex = value;
                OnPropertyChanged(nameof(ViewIndex));
            }
        }
        #endregion
        #region 代码片断
        /// <summary>
        /// 复制代码
        /// </summary>
        private void CopyCode()
        {
            if (string.IsNullOrWhiteSpace(ExtendCode)) return;
            try
            {
                Clipboard.SetText(ExtendCode);
            }
            catch (Exception e)
            {
                MessageBox.Show("因为【" + e.Message + "】未能复制", "复制代码");
            }
        }

        /// <summary>
        /// 生成的代码片断
        /// </summary>
        private string _codeType="cs";

        /// <summary>
        /// 生成的代码片断
        /// </summary>
        private string _extendCode;

        /// <summary>
        /// 生成的代码片断
        /// </summary>
        public string ExtendCode
        {
            get => _extendCode;
            set
            {
                if (Equals(_extendCode, value))
                    return;
                _extendCode = value;
                var code = System.Web.HttpUtility.HtmlEncode(value ?? "");
                var html = $@"
<html style='padding:0;margin:0'>
    <head>
        <meta charset='utf-8'/>
        <meta http-equiv='X-UA-Compatible' content='IE=edge'>
        <meta name='viewport' content='width=device-width, initial-scale=1' />
        <meta name='referrer' content='never' />
        <script src='https://code.jquery.com/jquery-1.11.3.js'></script>
        <link href='https://highlightjs.org/static/demo/styles/vs2015.css' rel='stylesheet'>  
        <script src='http://cdn.bootcss.com/highlight.js/8.0/highlight.min.js'></script>  
        <script>
            hljs.initHighlightingOnLoad();
            $(document).ready(function() {{
                    $('pre code').each(function(i, block) {{
                    hljs.highlightBlock(block);
                }});
            }});
        </script>
    <head>
    <body style='padding:0;margin:0;background-color:black'>
        <pre><code class='{_codeType}'>{code}</code></pre>
    </body>
</html>
";
                Browser.NavigateToString(html);
                Editor.ShowCode();
            }
        }

        private void DoMomentCode()
        {
            ViewIndex = 0;
            if (MomentCodeModel == null)
            {
                ExtendCode = "请选择一个生成方法";
                return;
            }
            try
            {
                ExtendCode = MomentCodeModel(Model.Context.SelectConfig);
            }
            catch (Exception e)
            {
                ExtendCode = $"因为【{e.Message}】未能生成对应的代码片断\n{e}";
            }
        }
        #endregion
    }
}