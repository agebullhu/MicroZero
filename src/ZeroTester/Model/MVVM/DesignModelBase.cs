using Agebull.Common.Mvvm;

namespace Agebull.EntityModel.Designer
{
    public class DesignModelBase : ModelBase
    {
        #region 操作命令
        
        /// <summary>
        ///     分类
        /// </summary>
        public string EditorName { get; set; }

        NotificationList<CommandItemBase> _commands;
        public NotificationList<CommandItemBase> Commands => _commands ??  (_commands=CreateCommands());

        /// <summary>
        /// 生成命令对象
        /// </summary>
        /// <returns></returns>
        public virtual NotificationList<CommandItemBase> CreateCommands()
        {
            var commands = new NotificationList<CommandItemBase>();
            CreateCommands(commands);

            return commands;
        }

        /// <summary>
        /// 生成命令对象
        /// </summary>
        /// <param name="commands"></param>
        protected virtual void CreateCommands(NotificationList<CommandItemBase> commands)
        {
        }

        #endregion

        #region 基本设置

        /// <summary>
        /// 同步解决方案变更
        /// </summary>
        public virtual void OnSolutionChanged()
        {
        }

        /// <summary>
        /// 上下文
        /// </summary>
        private DataModelDesignModel _model;

        /// <summary>
        /// 基本模型
        /// </summary>
        public DataModelDesignModel Model
        {
            get => _model;
            set
            {
                _model = value;
                RaisePropertyChanged(nameof(Model));
            }
        }

        #endregion

    }
}