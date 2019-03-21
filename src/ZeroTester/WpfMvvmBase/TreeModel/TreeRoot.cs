// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-24
// 修改:2014-11-29
// *****************************************************/

#region 引用

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Agebull.Common.Mvvm;

#endregion

namespace Agebull.EntityModel
{
    /// <summary>
    ///     表示一个树的根节点
    /// </summary>
    public sealed class TreeRoot : TreeItemBase
    {
        public static TreeRoot Root;

        private TreeItem _selectItem;

        /// <summary>
        ///     构造
        /// </summary>
        public TreeRoot()
        {
            Root = this;
        }
        /// <summary>
        /// 全局配置
        /// </summary>
        public readonly Dictionary<string, object> GlobalConfig = new Dictionary<string, object>();

        /// <summary>
        /// 找对应节点
        /// </summary>
        /// <returns></returns>
        public TreeItem Find(NotificationObject obj)
        {
            return Items.Select(child => child.Find(obj)).FirstOrDefault(item => item != null);
        }


        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="items">节点</param>
        public TreeRoot(IEnumerable<TreeItem> items)
        {
            if (items == null)
            {
                return;
            }
            foreach (var item in items)
            {
                Items.Add(item);
            }
        }

        #region 当前选中的节点

        /// <summary>
        ///     当前选中的节点
        /// </summary>
        public TreeItem SelectItem
        {
            get => _selectItem;
            set
            {
                if (_selectItem == value)
                {
                    return;
                }
                _selectItem?.ClearCommandList();
                _selectItem = value;
                _selectItem?.CreateCommandList();
                RaisePropertyChanged(() => SelectItem);
                SelectItemChanged?.Invoke(value, EventArgs.Empty);
            }
        }

        public event EventHandler SelectItemChanged;
        #endregion

        #region 选择

        /// <summary>
        ///     子级选择发生变化
        /// </summary>
        /// <param name="select">是否选中</param>
        /// <param name="child">子级</param>
        /// <param name="selectItem">选中的对象</param>
        protected internal override void OnChildIsSelectChanged(bool select, TreeItemBase child, TreeItemBase selectItem)
        {
            SelectPath = IsSelected ? null : child.SelectPath;
            if (isSelected != select)
            {
                isSelected = select;
                RaisePropertyChanged(() => IsSelected);
            }
            SelectItem = selectItem as TreeItem;
        }

        #endregion

        #region 扩展方法

        private NotificationList<CommandItemBase> _commands;

        private ModelFunctionDictionary<TreeRoot> _modelFunction;

        /// <summary>
        ///     依赖对象字典
        /// </summary>
        [IgnoreDataMember]
        public ModelFunctionDictionary<TreeRoot> ModelFunction
        {
            get => _modelFunction ?? (_modelFunction = new ModelFunctionDictionary<TreeRoot>() );
            set => _modelFunction = value;
        }

        /// <summary>
        ///     对应的命令集合
        /// </summary>
        public NotificationList<CommandItemBase> Commands
        {
            get => _commands ?? (_commands = new NotificationList<CommandItemBase>() );
            set
            {
                if (_commands == value)
                {
                    return;
                }
                _commands = value;
                RaisePropertyChanged(() => Commands);
            }
        }

        #endregion
        
    }
}
