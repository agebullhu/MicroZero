using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using Agebull.Common.Mvvm;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    /// 扩展ViewModel基类
    /// </summary>
    public abstract class ExtendViewModelBase : ViewModelBase
    {
        private DataModelDesignModel _baseModel;
        private DesignContext _context;
        private EditorModel _editor;

        #region 基本设置

        /// <summary>
        ///     对应的命令集合
        /// </summary>
        public IEnumerable<CommandItemBase> Buttons => Commands.Where(p => !p.NoButton);

        /// <summary>
        ///     对应的命令集合
        /// </summary>
        public CommandItem Menus =>
            new CommandItem
            {
                IsRoot = true,
                Caption = "扩展操作",
                Items = Commands.Where(p => p.NoButton).ToNotificationList<CommandItemBase>()
            };
        /// <summary>
        ///     分类
        /// </summary>
        public string EditorName { get; set; }


        /// <summary>
        /// 基本模型
        /// </summary>
        public DataModelDesignModel BaseModel
        {
            get => _baseModel;
            set
            {
                _baseModel = value;
                Context = value.Context;
                Editor = value.Editor;
                Dispatcher = value.Dispatcher;
                OnBaseModelBinding();
            }
        }


        /// <summary>
        ///     模型
        /// </summary>
        public abstract void OnBaseModelBinding();

        /// <summary>
        /// 上下文
        /// </summary>
        public DesignContext Context
        {
            set
            {
                _context = value;
                RaisePropertyChanged(nameof(Context));
            }

            get => _context;
        }


        /// <summary>
        /// 设计器
        /// </summary>
        public EditorModel Editor
        {
            get => _editor;
            set
            {
                _editor = value;
                RaisePropertyChanged(nameof(Editor));
            }
        }

        #endregion

        #region 主面板

        /// <summary>
        /// 主面板构造完成
        /// </summary>
        /// <param name="body"></param>
        protected abstract void OnBodyCreating(FrameworkElement body);


        /// <summary>
        ///     模型
        /// </summary>
        public abstract DesignModelBase DesignModel
        {
            get;
        }

        #endregion
    }

    /// <summary>
    /// 扩展ViewModel基类
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class ExtendViewModelBase<TModel> : ExtendViewModelBase
    where TModel : DesignModelBase, new()
    {
        public TModel Model { get; }
        /// <summary>
        /// 构造
        /// </summary>
        protected ExtendViewModelBase()
        {
            Model = new TModel
            {
                ViewModel = this
            };
        }

        /// <summary>
        ///     模型
        /// </summary>
        public override void OnBaseModelBinding()
        {
            Model.Model = BaseModel;
            Model.Context = BaseModel.Context;
            Model.Dispatcher = BaseModel.Dispatcher;
            Model.EditorName = EditorName;
            Model.Initialize();
        }
        /// <summary>
        /// 主面板构造完成
        /// </summary>
        /// <param name="body"></param>
        protected override void OnBodyCreating(FrameworkElement body)
        {
            //body.DataContext = Model;
            //RaisePropertyChanged(nameof(Context));
            //RaisePropertyChanged(nameof(DesignModel));
        }
        protected override NotificationList<CommandItemBase> CreateCommands()
        {
            return Model.CreateCommands();
        }

        /// <summary>
        ///     模型
        /// </summary>
        public sealed override DesignModelBase DesignModel => Model;


        /// <summary>
        ///     依赖对象字典
        /// </summary>
        [IgnoreDataMember]
        public ModelFunctionDictionary<TModel> ModelFunction { get; }

    }
}