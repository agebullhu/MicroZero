using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Agebull.Common.Mvvm;
using Agebull.EntityModel.Config;
using Agebull.ZeroNet.Core;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    ///     树节点模型
    /// </summary>
    public class ConfigTreeItem : TreeItem<StationConfig>
    {
        
        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="model"></param>
        public ConfigTreeItem(StationConfig model)
            : base(model)
        {
            InitDef();
        }
        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="model"></param>
        /// <param name="haseChildsAction"></param>
        /// <param name="loadChildsAction"></param>
        public ConfigTreeItem(StationConfig model, Func<StationConfig, IList> loadChildsAction, Func<StationConfig, bool> haseChildsAction)
            : base(model, loadChildsAction, haseChildsAction)
        {
            InitDef();
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="model"></param>
        /// <param name="createItemsAction"></param>
        /// <param name="haseChildsAction"></param>
        public ConfigTreeItem(StationConfig model, Func<StationConfig, List<TreeItem>> createItemsAction, Func<StationConfig, bool> haseChildsAction)
            : base(model, createItemsAction, haseChildsAction)
        {
            InitDef();
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="header"></param>
        /// <param name="model"></param>
        /// <param name="createItemsAction"></param>
        /// <param name="loadChildsAction"></param>
        public ConfigTreeItem(string header, StationConfig model, Func<StationConfig, List<TreeItem>> createItemsAction, Func<StationConfig, IList> loadChildsAction)
            : base(header, model, createItemsAction, loadChildsAction)
        {
            InitDef();
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="model"></param>
        /// <param name="createItemsAction"></param>
        public ConfigTreeItem(StationConfig model, Func<StationConfig, List<TreeItem>> createItemsAction)
            : base(model, createItemsAction)
        {
            InitDef();
        }

        /// <summary>
        /// 是否辅助节点
        /// </summary>
        public bool IsAssist { get; set; }

        #region 构造

        protected override void OnSourceModify()
        {
            if (IsAssist)
                return;
            if (!Equals(BackgroundColor, NotifySource.IsModify ? Brushes.Red : Brushes.Transparent))
                BackgroundColor = NotifySource.IsModify ? Brushes.Red : Brushes.Transparent;
            var par = Parent as TreeItem;
            if (par?.NotifySource != null && par.NotifySource.IsModify != NotifySource.IsModify)
            {
                par.NotifySource.IsModify = NotifySource.IsModify;
            }
            base.OnSourceModify();
        }

        protected override void OnSourceChanged(bool isRemove)
        {
            base.OnSourceChanged(isRemove);
        }

        private void InitDef()
        {
            HeaderField = "Name,Caption";
            HeaderExtendExpression = FormatTitle;
            StatusField = "IsReference,IsDelete,IsFreeze,Discard";
            StatusExpression = p => GetImage(p);
            NotifySource.PropertyChanged += OnModelPropertyChanged;
        }


        private string FormatTitle(StationConfig m)
        {
            return $"{m.Caption}({m.Name})";
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ModelPropertyChanged?.Invoke(this, e);
        }
        protected override void CreateCommandList(List<CommandItemBase> commands)
        {
            var treeItem = Parent as TreeItem;
            if (treeItem?.CreateChildFunc != null)
                commands.Add(new CommandItem
                {
                    Source = this,
                    Caption = "刷新",
                    Catalog = "视图",
                    Action = arg => ReBuildChild(),
                    Image = Application.Current.Resources["img_flush"] as ImageSource
                });
            base.CreateCommandList(commands);
        }
        public event EventHandler<PropertyChangedEventArgs> ModelPropertyChanged;
        /// <summary>
        /// 重新生成子级
        /// </summary>
        /// <returns></returns>
        private void ReBuildChild()
        {
            var treeItem = Parent as TreeItem;
            if (treeItem?.CreateChildFunc == null)
                return;
            TreeItem item = treeItem.CreateChild(NotifySource);
            Items.Clear();
            Items.AddRange(item.Items);
        }

        private BitmapImage GetImage(StationConfig m)
        {
            return imgDefault;
        }
        #endregion

        #region 默认方法

        private static readonly BitmapImage imgRef = Application.Current.Resources["img_ref"] as BitmapImage;

        private static readonly BitmapImage imgLock = Application.Current.Resources["img_lock"] as BitmapImage;

        private static readonly BitmapImage imgDel = Application.Current.Resources["img_del"] as BitmapImage;

        private static readonly BitmapImage imgDiscard = Application.Current.Resources["img_discard"] as BitmapImage;

        private static readonly BitmapImage imgModify = Application.Current.Resources["img_modify"] as BitmapImage;

        private static readonly BitmapImage imgDefault = Application.Current.Resources["img_no_modify"] as BitmapImage;

        #endregion

    }
}