using System.Collections.ObjectModel;
using System.IO;
using Agebull.EntityModel.Config;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    /// 设计器全局对象
    /// </summary>
    public class DesignGlobal : DesignModelBase, IDesignGlobal
    {

        private readonly string _programRoot;

        internal DesignGlobal()
        {
            _programRoot = Path.GetDirectoryName(Path.GetDirectoryName(typeof(GlobalConfig).Assembly.Location));
        }

        /// <summary>
        ///     状态消息
        /// </summary>
        string IDesignGlobal.StateMessage
        {
            get => Context.StateMessage;
            set => Context.StateMessage = value;
        }

        /// <inheritdoc />
        /// <summary>
        /// 应用根目录
        /// </summary>
        string IDesignGlobal.ProgramRoot => _programRoot;

        /// <summary>
        /// 解决方案
        /// </summary>
        SolutionConfig IDesignGlobal.GlobalSolution { get; set; }

        /// <summary>
        /// 解决方案
        /// </summary>
        SolutionConfig IDesignGlobal.LocalSolution { get; set; }

        /// <summary>
        /// 解决方案
        /// </summary>
        SolutionConfig IDesignGlobal.CurrentSolution
        {
            get => Context.Solution;
            set => Context.Solution = value;
        }


        /// <summary>
        /// 当前选择
        /// </summary>
        ConfigBase IDesignGlobal.CurrentConfig
        {
            get => Context.SelectConfig;
            set => Context.SelectConfig = value;
        }


        /// <summary>
        ///     枚举集合
        /// </summary>
        private NotificationList<SolutionConfig> _solutions;

        /// <summary>
        ///     解决方案集合
        /// </summary>
        NotificationList<SolutionConfig> IDesignGlobal.Solutions
        {
            get
            {
                if (_solutions != null)
                    return _solutions;
                _solutions = new NotificationList<SolutionConfig>();
                return _solutions;
            }
        }

        /// <summary>
        ///     枚举集合
        /// </summary>
        private NotificationList<EnumConfig> _enums;

        /// <summary>
        ///     枚举集合
        /// </summary>
        NotificationList<EnumConfig> IDesignGlobal.Enums
        {
            get
            {
                if (_enums != null)
                    return _enums;
                _enums = new NotificationList<EnumConfig>();
                return _enums;
            }
        }

        /// <summary>
        ///     实体集合
        /// </summary>
        private NotificationList<EntityConfig> _entities;

        /// <summary>
        ///     实体集合
        /// </summary>
        NotificationList<EntityConfig> IDesignGlobal.Entities
        {
            get
            {
                if (_entities != null)
                    return _entities;
                _entities = new NotificationList<EntityConfig>();
                return _entities;
            }
        }

        /// <summary>
        ///     项目集合
        /// </summary>
        private NotificationList<ProjectConfig> _projects;

        /// <summary>
        ///     项目集合
        /// </summary>
        NotificationList<ProjectConfig> IDesignGlobal.Projects
        {
            get
            {
                if (_projects != null)
                    return _projects;
                _projects = new NotificationList<ProjectConfig>();
                return _projects;
            }
        }

        /// <summary>
        ///     API集合
        /// </summary>
        private NotificationList<ApiItem> _apiItems;

        /// <summary>
        ///     API集合
        /// </summary>
        NotificationList<ApiItem> IDesignGlobal.ApiItems
        {
            get
            {
                if (_apiItems != null)
                    return _apiItems;
                _apiItems = new NotificationList<ApiItem>();
                return _apiItems;
            }
        }

    }
}