using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Agebull.Common.Mvvm;

namespace Agebull.EntityModel
{
    /// <summary>
    ///     ���ڵ�ģ��
    /// </summary>
    public class TreeItem : TreeItemBase
    {
        /// <summary>
        /// ����δ������ӽڵ�
        /// </summary>
        protected static readonly TreeItem LodingItem = new TreeItem("...");

        /// <summary>
        ///     ����
        /// </summary>
        public TreeItem()
        {
        }

        /// <summary>
        ///     ����
        /// </summary>
        /// <param name="source"></param>
        public TreeItem(object source) : base(source)
        {
        }

        /// <summary>
        ///     ����
        /// </summary>
        public string Catalog
        {
            get;
            set;
        }
        /// <summary>
        ///     ����
        /// </summary>
        /// <param name="header"></param>
        public TreeItem(string header)
        {
            Header = header;
        }


        public T FindParentModel<T>() where T : class
        {
            if (!(Parent is TreeItem itemModel))
                return null;
            var model = itemModel.NotifySource as T;
            return model ?? itemModel.FindParentModel<T>();
        }
        /// <summary>
        ///     ��
        /// </summary>
        protected internal TreeRoot Root
        {
            get
            {
                if (Parent is TreeRoot root)
                    return root;
                var item = Parent as TreeItem;
                return item?.Root;
            }
        }

        private bool _isExpend;

        /// <summary>
        ///     չ��
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpend;
            set
            {
                if (_isExpend == value)
                {
                    return;
                }
                _isExpend = value;
                RaisePropertyChanged(() => IsExpanded);
                OnIsExpandedChanged();
            }
        }

        /// <summary>
        ///     �л�չ��
        /// </summary>
        public void ExpandedChild(object arg)
        {
            ExpandedChild(!_isExpend);
        }

        /// <summary>
        ///     չ��
        /// </summary>
        public void ExpandedChild(bool ext)
        {
            IsExpanded = ext;
            foreach (var child in Items)
                child.ExpandedChild(ext);
        }
        private string _header;

        /// <summary>
        ///     ����
        /// </summary>
        public string Header
        {
            get => _header;
            set
            {
                if (_header == value)
                {
                    return;
                }
                _header = value;
                RaisePropertyChanged(() => Header);
            }
        }



        /// <summary>
        /// �Ҷ�Ӧ�ڵ�
        /// </summary>
        /// <returns></returns>
        public TreeItem Find(NotificationObject obj)
        {
            if (obj == null || Items.Count == 0)
                return null;
            if (NotifySource == obj)
                return this;
            return Items.Select(child => child.Find(obj)).FirstOrDefault(item => item != null);
        }

        /// <summary>
        /// չ��״̬�仯�Ĵ���
        /// </summary>
        protected virtual void OnIsExpandedChanged()
        {
        }

        protected override void OnSourceChanged(bool isRemove)
        {
            if (_commands == null)
                return;
            if (BindingObject is INotifyPropertyChanged pp)
            {
                if (isRemove)
                    pp.PropertyChanged -= OnModelPropertyChanged;
                else
                    pp.PropertyChanged += OnModelPropertyChanged;
            }
            if (isRemove)
            {
                foreach (var cmd in _commands.Where(p => p.Source == NotifySource).ToArray())
                {
                    _commands.Remove(cmd);
                }
            }
            else
            {
                var actions = CommandCoefficient.Coefficient(NotifySource);
                foreach (var action in actions)
                {
                    action.Source = NotifySource;
                    _commands.Add(action);
                }
            }
        }

        private List<CommandItemBase> _commands;

        public List<CommandItemBase> Commands => _commands;

        /// <summary>
        /// ���������б�
        /// </summary>
        public List<CommandItemBase> CreateCommandList()
        {
            var commands = new List<CommandItemBase>();
            var actions = CommandCoefficient.Coefficient(NotifySource);
            if (actions != null)
            {
                foreach (var action in actions)
                {
                    action.Source = NotifySource;
                    commands.Add(action);
                }
            }
            CreateCommandList(commands);
            commands.Add(new CommandItem
            {
                IsButton = true,
                NoConfirm = true,
                Name = "ExpandedChild",
                Caption = "�л�չ��",
                Catalog = "��ͼ",
                IconName = "tree_Open",
                Action = ExpandedChild
            });
            return _commands = commands;
        }

        /// <summary>
        /// ���������б�
        /// </summary>
        protected virtual void CreateCommandList(List<CommandItemBase> commands)
        {
        }

        /// <summary>
        /// ���������б�
        /// </summary>
        public void ClearCommandList()
        {
            if (_commands == null)
                return;
            foreach (var command in _commands)
            {
                command.Source = null;
            }
            _commands.Clear();
            _commands = null;
        }
        private string _soruceType;


        /// <summary>
        ///     ������
        /// </summary>
        public string SoruceType
        {
            get => _soruceType;
            set
            {
                if (_soruceType == value)
                {
                    return;
                }
                _soruceType = value;
                RaisePropertyChanged(() => SoruceType);
                RaisePropertyChanged(() => SoruceTypeIcon);
            }
        }

        private FontWeight _font;
        /// <summary>
        ///     �����С
        /// </summary>
        public FontWeight FontWeight
        {
            get => _font;
            set
            {
                if (Equals(_font, value))
                {
                    return;
                }
                _font = value;
                RaisePropertyChanged(() => FontWeight);
            }
        }

        private Brush _color = Brushes.Black;
        /// <summary>
        ///     ������ɫ
        /// </summary>
        public Brush Color
        {
            get => _color;
            set
            {
                if (Equals(_color, value))
                {
                    return;
                }
                _color = value;
                RaisePropertyChanged(() => Color);
            }
        }

        private Brush _bcolor = Brushes.Transparent;
        /// <summary>
        ///     ������ɫ
        /// </summary>
        public Brush BackgroundColor
        {
            get => _bcolor;
            set
            {
                if (Equals(_bcolor, value))
                {
                    return;
                }
                _bcolor = value;
                RaisePropertyChanged(() => BackgroundColor);
            }
        }

        private BitmapImage _soruceTypeIcon;
        protected BitmapImage _baseIcon;
        /// <summary>
        ///     ������
        /// </summary>
        public BitmapImage SoruceTypeIcon
        {
            get => _soruceTypeIcon;
            set
            {
                if (Equals(_soruceTypeIcon, value))
                {
                    return;
                }
                if (_baseIcon == null)
                    _baseIcon = value;
                _soruceTypeIcon = value;
                RaisePropertyChanged(() => SoruceTypeIcon);
            }
        }
        protected BitmapImage _statusIcon;
        /// <summary>
        ///     ������
        /// </summary>
        public BitmapImage StatusIcon
        {
            get => _statusIcon;
            set
            {
                if (Equals(_statusIcon, value))
                {
                    return;
                }
                _statusIcon = value;
                RaisePropertyChanged(() => StatusIcon);
            }
        }


        private CommandStatus _childsStatus;
        /// <summary>
        ///     �Ӽ�����״̬
        /// </summary>
        public CommandStatus ChildsStatus
        {
            get => _childsStatus;
            set
            {
                if (_childsStatus == value)
                {
                    return;
                }
                _childsStatus = value;
                RaisePropertyChanged(() => ChildsStatus);
                RaisePropertyChanged(() => IsBusy);
                RaisePropertyChanged(() => ChildsStatusIcon);
            }
        }

        /// <summary>
        ///     �Ӽ�����״̬
        /// </summary>
        public bool IsBusy => ChildsStatus == CommandStatus.Executing;

        /// <summary>
        ///     ������
        /// </summary>
        public BitmapImage ChildsStatusIcon => Application.Current.Resources[$"async_{ChildsStatus}"] as BitmapImage;

        #region �����Զ�����

        private IFunctionDictionary _modelFunction;
        /// <summary>
        /// �����ֵ�
        /// </summary>
        public virtual IFunctionDictionary ModelDelegates => _modelFunction ?? (_modelFunction = new ModelFunctionDictionary<object>());

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (HeaderField != null && HeaderField.Contains(e.PropertyName))
            {
                BeginInvokeInUiThread(SyncHeaderAutomatic);
            }
            if (StatusField != null && StatusField.Contains(e.PropertyName))
            {
                BeginInvokeInUiThread(SyncStatusImageAutomatic);
            }
            if (ColorField != null && ColorField.Contains(e.PropertyName))
            {
                BeginInvokeInUiThread(SyncColorAutomatic);
            }

            OnStatePropertyChanged(NotifySource, e);
            _customPropertyChanged?.Invoke(this, NotifySource, e.PropertyName);
        }

        protected void OnStatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NotificationObject.IsModify))
            {
                BackgroundColor = NotifySource.IsModify ? Brushes.Silver : Brushes.Transparent;
            }
            else if (StatusField != null && StatusField.Contains(e.PropertyName))
            {
                BeginInvokeInUiThread(SyncStatusImageAutomatic);
            }
        }
        private Action<TreeItem, NotificationObject, string> _customPropertyChanged;
        /// <summary>
        /// ͬ���Զ�����
        /// </summary>
        public Action<TreeItem, NotificationObject, string> CustomPropertyChanged
        {
            get => _customPropertyChanged;
            set
            {
                _customPropertyChanged = value;
                value?.Invoke(this, NotifySource, null);
            }
        }

        /// <summary>
        /// ͼ����ص��ֶ�
        /// </summary>
        public string StatusField
        {
            get;
            set;
        }

        /// <summary>
        /// ��ɫ��ص��ֶ�
        /// </summary>
        public string ColorField
        {
            get;
            set;
        }

        /// <summary>
        /// ͬ���Զ�����
        /// </summary>
        protected virtual void SyncStatusImageAutomatic()
        {
            StatusIcon = ModelDelegates.TryExecute<BitmapImage>();
        }

        /// <summary>
        /// ģ��������ص��ֶ�
        /// </summary>
        public string HeaderField
        {
            get;
            set;
        }

        /// <summary>
        /// ͬ���Զ�����
        /// </summary>
        protected virtual void SyncColorAutomatic()
        {
            Color = ModelDelegates.TryExecute<Brush>();
        }

        /// <summary>
        /// ͬ���Զ�����
        /// </summary>
        protected virtual void SyncHeaderAutomatic()
        {
            Header = ModelDelegates.TryExecute<string>();
        }
        #endregion

        #region ѡ��

        /// <summary>
        ///     ��ǰѡ�����仯
        /// </summary>
        protected sealed override void OnIsSelectChanged()
        {
            SelectPath = IsSelected ? Header : null;
            if (!IsSelected)
            {
                SelectPath = null;
            }
            Parent?.OnChildIsSelectChanged(IsSelected, this, this);
        }

        /// <summary>
        ///     �Ӽ�ѡ�����仯
        /// </summary>
        /// <param name="select">�Ƿ�ѡ��</param>
        /// <param name="child">�Ӽ�</param>
        /// <param name="selectItem">ѡ�еĶ���</param>
        protected internal sealed override void OnChildIsSelectChanged(bool select, TreeItemBase child, TreeItemBase selectItem)
        {
            SelectPath = IsSelected ? null : Header + " > " + child.SelectPath;
            if (isSelected != select)
            {
                isSelected = select;
                RaisePropertyChanged(() => IsSelected);
            }
            Parent?.OnChildIsSelectChanged(IsSelected, this, selectItem);
        }

        #endregion

    }
}