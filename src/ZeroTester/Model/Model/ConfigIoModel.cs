using System.IO;
using System.Text;
using System.Windows.Forms;
using Agebull.EntityModel.Config;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    /// 配置读写模型
    /// </summary>
    public class ConfigIoModel : DesignModelBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        protected override void DoInitialize()
        {
            if (!File.Exists(Context.FileName))
            {
                Load();
            }
            else
            {
                ReLoad();
            }
        }

        private const string fileType = "数据结构文件(*.json)|*.json";
        #region 文件读写
        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            Context.StateMessage = "正在保存...";
            if (Context.FileName == null)
            {
                var sfd = new SaveFileDialog
                {
                    Filter = fileType
                };
                if (sfd.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }
            }
            SaveSolution();
        }
        /// <summary>
        /// 保存解决方案
        /// </summary>
        public void SaveSolution()
        {
            ConfigWriter.Save(Context.Solution, Context.FileName);
            SolutionModel model = new SolutionModel
            {
                Solution = Context.Solution
            };
            model.ResetStatus();
            Context.StateMessage = "保存成功";
        }
        /// <summary>
        /// 载入解决方案
        /// </summary>
        public void Load()
        {
            var sfd = new OpenFileDialog
            {
                Filter = fileType,
                FileName = Context.FileName
            };

            if (sfd.ShowDialog() != true)
            {
                return;
            }
            Load(sfd.FileName);
            DataModelDesignModel.Screen.LastFile = sfd.FileName;
            DataModelDesignModel.SaveUserScreen();

        }
        /// <summary>
        /// 载入解决方案
        /// </summary>
        public void LoadGlobal()
        {
            Context.Solution = GlobalConfig.GlobalSolution;
            Model.OnSolutionChanged();
        }
        /// <summary>
        /// 载入解决方案
        /// </summary>
        public void LoadLocal()
        {
            Context.Solution = GlobalConfig.LocalSolution;
            Model.OnSolutionChanged();
        }
        /// <summary>
        /// 重新载入
        /// </summary>
        public void ReLoad()
        {
            Load(Context.FileName);
        }
        /// <summary>
        /// 载入解决方案
        /// </summary>
        /// <param name="sluFile"></param>
        public void Load(string sluFile)
        {
            Context.StateMessage = "正在载入...";
            Context.Solution = ConfigLoader.Load(sluFile);
            Context.StateMessage = "载入成功";
            Model.OnSolutionChanged();
        }

        /// <summary>
        /// 新增解决方案
        /// </summary>
        public void CreateNew()
        {
            var sfd = new SaveFileDialog
            {
                Filter = fileType
            };
            if (sfd.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            Context.Solution = new SolutionConfig
            {
                Name = Path.GetFileNameWithoutExtension(sfd.FileName),
                Caption = Path.GetFileNameWithoutExtension(sfd.FileName),
                SaveFileName=sfd.FileName 
            };
            DataModelDesignModel.Screen.LastFile = sfd.FileName;
            SaveSolution();
            Load(sfd.FileName);
            DataModelDesignModel.SaveUserScreen();
        }

        #endregion
    }
}
