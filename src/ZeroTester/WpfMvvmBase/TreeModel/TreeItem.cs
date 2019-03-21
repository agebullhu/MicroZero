// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-24
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Agebull.Common.Reflection;
using Agebull.Common.Mvvm;

#endregion

namespace Agebull.EntityModel
{
    /// <summary>
    ///     树节点模型
    /// </summary>
    public class TreeItem<TModel> : TreeItem
            where TModel : class
    {
        #region 属性

        /// <summary>
        ///     绑定的模型(与Source相同,但这是强类型的)
        /// </summary>
        public TModel Model { get; }

        #endregion

        #region 构造

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="model"></param>
        public TreeItem(TModel model)
            : base(model)
        {
            Model = model;
            _modelFunction = new ModelFunctionDictionary<TModel>
            {
                Model = Model
            };
            Extend.DependencyDelegates.AnnexAction(Load);
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="model"></param>
        /// <param name="haseChildsAction"></param>
        /// <param name="loadChildsAction"></param>
        public TreeItem(TModel model, Func<TModel, IList> loadChildsAction, Func<TModel, bool> haseChildsAction)
            : this(model)
        {
            _modelFunction.AnnexFunc(loadChildsAction);
            _modelFunction.AnnexFunc(haseChildsAction);
            HaseChilds = haseChildsAction(model);
            if (HaseChilds)
            {
                Items.Add(LodingItem);
            }
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="model"></param>
        /// <param name="createItemsAction"></param>
        /// <param name="haseChildsAction"></param>
        public TreeItem(TModel model, Func<TModel, List<TreeItem>> createItemsAction, Func<TModel, bool> haseChildsAction)
            : this(model)
        {
            _modelFunction.AnnexFunc(createItemsAction);
            _modelFunction.AnnexFunc(haseChildsAction);
            HaseChilds = haseChildsAction(model);
            if (HaseChilds)
            {
                Items.Add(LodingItem);
            }
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="header"></param>
        /// <param name="model"></param>
        /// <param name="createItemsAction"></param>
        /// <param name="loadChildsAction"></param>
        public TreeItem(string header, TModel model, Func<TModel, List<TreeItem>> createItemsAction, Func<TModel, IList> loadChildsAction)
            : this(model)
        {
            Header = header;
            _modelFunction.AnnexFunc(createItemsAction);
            _modelFunction.AnnexFunc(loadChildsAction);
            _modelFunction.AnnexFunc(p => loadChildsAction(model).Count > 0);
            HaseChilds = loadChildsAction(model).Count > 0;
            if (HaseChilds)
            {
                Items.Add(LodingItem);
            }
        }


        /// <summary>
        /// 构建命令列表
        /// </summary>
        protected override void CreateCommandList(List<CommandItemBase> commands)
        {
            if (LoadCommand != null)
            {
                commands.Add(LoadCommand);
            }
        }
        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="model"></param>
        /// <param name="createItemsAction"></param>
        public TreeItem(TModel model, Func<TModel, List<TreeItem>> createItemsAction)
            : this(model)
        {
            Model = model;
            HaseChilds = true;
            _modelFunction.AnnexFunc(createItemsAction);
            _modelFunction.AnnexFunc(p => true);
            Items.Add(LodingItem);
        }

        #endregion

        #region 子级同步处理

        protected override void OnSourceChanged(bool isRemove)
        {
            base.OnSourceChanged(isRemove);
        }

        /// <summary>
        /// 展开状态变化的处理
        /// </summary>
        protected override void OnIsExpandedChanged()
        {
            if (IsExpanded && ChildsStatus <= CommandStatus.Faulted)
            {
                Load();
            }
        }


        #endregion

        #region 扩展方法


        [IgnoreDataMember]
        protected readonly ModelFunctionDictionary<TModel> _modelFunction;

        /// <summary>
        ///     依赖方法(模型相关)
        /// </summary>
        [IgnoreDataMember]
        public override IFunctionDictionary ModelDelegates => _modelFunction;

        /// <summary>
        ///     依赖方法(模型相关)
        /// </summary>
        [IgnoreDataMember]
        public ModelFunctionDictionary<TModel> ModelDelegateDictionary => _modelFunction;

        [IgnoreDataMember]
        private CommandItemBase _reloadCommand;
        /// <summary>
        ///     载入子级的命令
        /// </summary>
        [IgnoreDataMember]
        public CommandItemBase LoadCommand => _reloadCommand ?? (_reloadCommand = CreateCommand());

        private CommandItemBase CreateCommand()
        {
            var createItems = _modelFunction.GetFunction<List<TreeItem>>();
            if (createItems != null)
            {
                return new AsyncCommandItem<TModel, List<TreeItem>>(BeginLoad, createItems, SyncItems)
                {
                    Source = Model,
                    Caption = "载入子级",
                    Image = Application.Current.Resources["async_Executing"] as BitmapImage,
                };
            }
            var loadChildren = _modelFunction.GetFunction<IList>();
            if (loadChildren != null)
            {
                return new AsyncCommandItem<TModel, IList>(BeginLoad, loadChildren, SyncItems)
                {
                    Source = Model,
                    Caption = "载入子级",
                    Image = Application.Current.Resources["async_Executing"] as BitmapImage,
                };
            }
            return null;
        }

        private void SyncItems(CommandStatus status, Exception exception, IList values)
        {
            ChildsStatus = status;
            //Items.Clear();
            if (exception != null)
            {
                return;
            }
            if (values == null)
            {
                return;
            }
            FriendItems = values;
        }
        #endregion

        #region 扩展属性


        private bool _haseChilds;


        /// <summary>
        ///     是否有子级
        /// </summary>
        public bool HaseChilds
        {
            get => _haseChilds;
            set
            {
                if (_haseChilds == value)
                {
                    return;
                }
                _haseChilds = value;
                RaisePropertyChanged(() => HaseChilds);
            }
        }


        #endregion

        #region 内容自动更新

        private Expression<Func<TModel, BitmapImage>> _statusExpression;
        /// <summary>
        /// 取图标的方法
        /// </summary>
        [Browsable(false)]
        public Expression<Func<TModel, BitmapImage>> StatusExpression
        {
            get => _statusExpression;
            set
            {
                _statusExpression = value;
                if (value == null)
                    return;
                ModelDelegates.AnnexDelegate(ReflectionHelper.GetFunc(value));
                SyncStatusImageAutomatic();
            }
        }

        private Expression<Func<TModel, Brush>> _colorExpression;

        /// <summary>
        /// 取标题内容的方法
        /// </summary>
        [Browsable(false)]
        public Expression<Func<TModel, Brush>> ColorExpression
        {
            get => _colorExpression;
            set
            {
                _colorExpression = value;
                if (value == null)
                    return;
                _modelFunction.AnnexFunc(ReflectionHelper.GetFunc(value));
                SyncColorAutomatic();
            }
        }



        private Func<TModel, string> _headerExpression;

        /// <summary>
        /// 取标题内容的方法
        /// </summary>
        [Browsable(false)]
        public Func<TModel, string> HeaderExtendExpression
        {
            get => _headerExpression;
            set
            {
                _headerExpression = value;
                if (value == null)
                    return;
                _modelFunction.AnnexFunc(value);
                SyncHeaderAutomatic();
            }
        }
        #endregion

        #region 载入

        /// <summary>
        ///     载入子级
        /// </summary>
        public void Load()
        {
            LoadCommand?.Execute(Model);
        }

        /// <summary>
        ///     重新载入子级
        /// </summary>
        private bool BeginLoad(TModel args)
        {
            ChildsStatus = CommandStatus.Executing;
            Items.Clear();
            return true;
        }

        private void SyncItems(CommandStatus status, Exception exception, List<TreeItem> items)
        {
            ChildsStatus = status;
            //Items.Clear();
            if (exception != null)
            {
                return;
            }
            if (items == null)
            {
                return;
            }
            foreach (var item in items)
            {
                Items.Add(item);
            }
        }

        #endregion
        
        public ICommand Command { get; set; }
    }

}
