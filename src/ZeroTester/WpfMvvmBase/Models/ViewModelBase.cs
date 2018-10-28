// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Gboxt.Common.WpfMvvmBase
// 建立:2014-11-24
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows;
using Agebull.Common.Mvvm;

#endregion

namespace Agebull.EntityModel
{
    /// <summary>
    ///     MVVM的ViewModel的基类
    /// </summary>
    public abstract class ViewModelBase : MvvmBase
    {
        /// <summary>
        /// 对应的视图
        /// </summary>
        public FrameworkElement View { get; private set; }

        /// <summary>
        /// 绑定视图对象的行为
        /// </summary>
        public DependencyAction ViewBehavior => new DependencyAction
        {
            AttachAction = BindingView
        };

        /// <summary>
        ///  绑定视图对象
        /// </summary>
        /// <param name="obj"></param>
        private void BindingView(DependencyObject obj)
        {
            View = obj as FrameworkElement;
            if (View == null)
                return;
            Dispatcher = View.Dispatcher;
            //WorkContext.SynchronousContext = Synchronous = new DispatcherSynchronousContext
            //{
            //    Dispatcher = Dispatcher
            //};
            OnViewSeted();
        }
        /// <summary>
        /// 视图绑定成功后的初始化动作
        /// </summary>
        protected virtual void OnViewSeted()
        {

        }


        private NotificationList<CommandItemBase> _commands;

        /// <summary>
        ///     对应的命令集合
        /// </summary>
        public NotificationList<CommandItemBase> Commands => _commands ?? (_commands = CreateCommands());

        /// <summary>
        /// 构造命令列表
        /// </summary>
        /// <returns></returns>
        protected virtual NotificationList<CommandItemBase> CreateCommands()
        {
            return new NotificationList<CommandItemBase>();
        }
    }

    /// <summary>
    ///     MVVM的ViewModel的基类
    /// </summary>
    public abstract class ViewModelBase<TModel> : ViewModelBase
            where TModel : ModelBase, new()
    {
        private TModel _model;

        /// <summary>
        ///     模型
        /// </summary>
        public TModel Model
        {
            get => _model;
            set
            {
                _model = value;
                if (value != null)
                    value.ViewModel = this;
            }
        }


        /// <summary>
        ///     依赖对象字典
        /// </summary>
        [IgnoreDataMember]
        public ModelFunctionDictionary<TModel> ModelFunction { get; private set; }

        /// <summary>
        /// 视图绑定成功后的初始化动作
        /// </summary>
        protected override void OnViewSeted()
        {
            Model = new TModel
            {
                Dispatcher = Dispatcher,
                Synchronous = Synchronous
            };
            ModelFunction = new ModelFunctionDictionary<TModel>
            {
                Model = Model
            };
            Dispatcher.BeginInvoke(new Action(Model.Initialize));
            RaisePropertyChanged(() => Model);
        }
    }
}
