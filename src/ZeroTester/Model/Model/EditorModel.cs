using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Agebull.Common.Mvvm;
using Agebull.EntityModel.Config;
using Application = System.Windows.Application;
using TabControl = System.Windows.Controls.TabControl;

namespace Agebull.EntityModel.Designer
{
    public class EditorModel : DesignModelBase
    {
        #region 初始化

        public void CreateMenus(DataModelDesignModel model)
        {
            Menus = CreateMenus();
        }
        /// <summary>
        ///     初始化
        /// </summary>
        protected override void DoInitialize()
        {
            OnPropertyChanged(nameof(Menus));
            CheckWindow();
            Context.PropertyChanged += Context_PropertyChanged;
        }

        /// <summary>
        /// 同步解决方案变更
        /// </summary>
        public override void OnSolutionChanged()
        {
            SolutionConfig.Current.WorkView = Screen.WorkView;
            CheckWindow();
        }
        #endregion

        #region 扩展对象

        /// <summary>
        /// 扩展对象插入的控件
        /// </summary>
        internal TabControl ExtendEditorPanel { get; set; }

        private Action checkExtendAction;
        private Action CheckWindowAction => checkExtendAction ?? (checkExtendAction = CheckWindow);

        private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Context.SelectConfig))
            {
                ExtendEditorPanel.Dispatcher.BeginInvoke(CheckWindowAction);
            }
        }

        internal void CheckWindow()
        {
            CreateExtendEditor();
            CheckEditorVisibility();
        }
        Type extendType;
        string extendWorkView;
        private bool HaseExtend => ExtendEditorPanel.Items.Count > 0;
        internal void CreateExtendEditor()
        {
            if (Context.SelectConfig == null || NowEditor != EditorConfig)
            {
                return;
            }
            var type = Context.SelectConfig.GetType();
            if (extendType == type && extendWorkView == WorkView)
            {
                return;
            }
            extendType = type;
            extendWorkView = WorkView;

            if (!DesignerManager.ExtendDictionary.TryGetValue(type, out var exts) || exts.Count == 0)
            {
                ExtendEditorPanel.Items.Clear();
                return;
            }

            ExtendEditorPanel.Items.Clear();
            foreach (var ext in exts.OrderBy(p => p.Value.Index))
            {
                if (WorkView != null && ext.Value.Filter.Count > 0 && !ext.Value.Filter.Contains(WorkView))
                    continue;
                CreateExtendEditor(ext.Key, ext.Value);
            }
            if (ExtendEditorPanel.Items.Count > 0)
            {
                Model.Dispatcher.Invoke(() =>
                {
                    ExtendEditorPanel.UpdateLayout();
                    ExtendEditorPanel.SelectedIndex = 0;
                    ExtendEditorPanel.UpdateLayout();
                });
            }
        }
        private void CreateExtendEditor(string title, ExtendViewOption option)
        {
            var editor = option.Create();
            var vm = (ExtendViewModelBase)editor.DataContext;
            vm.BaseModel = Model;
            var item = new TabItem
            {
                Header = title,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
            };
            var ext = new ExtendPanel
            {
                Child = editor,
                DataContext = editor.DataContext,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            };
            item.Content = ext;
            ExtendEditorPanel.Items.Add(item);
            item.UpdateLayout();
        }

        #endregion


        #region 菜单

        /// <summary>
        /// 菜单
        /// </summary>
        public NotificationList<CommandItemBase> Menus { get; set; }

        private int fileMenuCount;
        /// <summary>
        /// 构造命令列表
        /// </summary>
        /// <returns></returns>
        NotificationList<CommandItemBase> CreateMenus()
        {
            #region 文件菜单

            fileMenu = new CommandItem
            {
                Caption = "文件",
                IsRoot = true,
                Items = new NotificationList<CommandItemBase>
                {
                    new CommandItem
                    {
                        Action =arg=> Model.ConfigIo.CreateNew(),
                        Caption = "新建",
                        Image = Application.Current.Resources["img_add"] as ImageSource
                    },
                    new CommandItem
                    {
                        IsButton=true,
                        Action =arg=> Model.ConfigIo.Load(),
                        Caption = "打开",
                        NoConfirm=true,
                        Image = Application.Current.Resources["img_open"] as ImageSource
                    },
                    new CommandItem
                    {
                        Action =arg=> Model.ConfigIo.ReLoad(),
                        Caption = "重新载入",
                        Image = Application.Current.Resources["img_redo"] as ImageSource
                    },
                    CommandItemBase.Line,
                    new CommandItem
                    {
                        Action = arg=>Model.ConfigIo.LoadGlobal(),
                        Caption = "全局",
                        NoConfirm=true,
                        Image = Application.Current.Resources["img_open"] as ImageSource
                    },
                    new CommandItem
                    {
                        Action = arg=>Model.ConfigIo.LoadLocal(),
                        Caption = "本地",
                        NoConfirm=true,
                        Image = Application.Current.Resources["img_open"] as ImageSource
                    },
                    CommandItemBase.Line,
                    new CommandItem
                    {
                        IsButton=true,
                        NoConfirm=true,
                        Action = arg=>Model.ConfigIo.Save(),
                        Caption = "保存",
                        Image = Application.Current.Resources["imgSave"] as ImageSource
                    },
                    CommandItemBase.Line,
                    CommandItemBase.Line,
                    new CommandItem
                    {
                        Action = arg=>Application.Current.Shutdown(),
                        Caption = "退出"
                    }
                }
            };
            fileMenuCount = fileMenu.Items.Count;
            #endregion
            #region 窗口菜单

            WindowMenu = new CommandItem
            {
                Caption = "窗口",
                IsRoot = true
            };
            foreach (var job in RootJobs)
            {
                var item = new CommandItem<CommandItemBase>
                {
                    Action = OnJobSelect,
                    Caption = job.Key,
                    IsChecked = job.Key == NowEditor,
                    IconName = "tree_Child1"
                };
                item.Source = item;
                WindowMenu.Items.Add(item);
            }
            #endregion
            #region 视角菜单

            viewMenu = new CommandItem
            {
                Caption = "设计",
                IsRoot = true
            };
            var vitem = new CommandItem<CommandItemBase>
            {
                Action = OnWorkView,
                Caption = "专家视角"
            };
            vitem.Source = vitem;
            viewMenu.Items.Add(vitem);
            vitem = new CommandItem<CommandItemBase>
            {
                Action = OnWorkView,
                Caption = "数据库设计",
                Catalog = "DataBase"
            };
            vitem.Source = vitem;
            viewMenu.Items.Add(vitem);
            vitem = new CommandItem<CommandItemBase>
            {
                Action = OnWorkView,
                Caption = "实体设计",
                Catalog = "Entity"
            };
            vitem.Source = vitem;
            viewMenu.Items.Add(vitem);
            vitem = new CommandItem<CommandItemBase>
            {
                Action = OnWorkView,
                Caption = "模型设计",
                Catalog = "Model"
            };
            vitem.Source = vitem;
            viewMenu.Items.Add(vitem);
            vitem = new CommandItem<CommandItemBase>
            {
                Action = OnWorkView,
                Caption = "接口设计",
                Catalog = "Api"
            };
            vitem.Source = vitem;
            viewMenu.Items.Add(vitem);
            viewMenu.Items.Add(CommandItemBase.Line);
            vitem = new CommandItem<CommandItemBase>
            {
                Action = OnAdvancedView,
                Caption = "高级设置",
                Catalog = "Api",
                IsChecked = AdvancedView
            };
            vitem.Source = vitem;
            viewMenu.Items.Add(vitem);
            viewMenu.Items.Add(CommandItemBase.Line);
            #endregion
            return new NotificationList<CommandItemBase>
            {
                fileMenu,
                viewMenu,
                WindowMenu
            };
        }

        private CommandItemBase[] _buttons;


        /// <summary>
        ///     对应的命令集合
        /// </summary>
        public CommandItemBase[] Buttons
        {
            get => _buttons;
            set
            {
                _buttons = value;
                RaisePropertyChanged(nameof(Buttons));
            }
        }

        public CommandItem WindowMenu { get; private set; }

        private CommandItem viewMenu, fileMenu;
        /// <summary>
        /// 同步菜单
        /// </summary>
        /// <param name="item"></param>
        internal void SyncMenu(TreeItem item)
        {
            if (item == null)
            {
                return;
            }
            var menus = Menus;

            while (menus.Count > 3)
                menus.RemoveAt(2);
            while (fileMenu.Items.Count > fileMenuCount)
                fileMenu.Items.RemoveAt(fileMenuCount - 2);
            while (viewMenu.Items.Count > 8)
                viewMenu.Items.RemoveAt(8);

            menus.Insert(2, new CommandItem
            {
                IsRoot = true,
                Caption = "编辑",
                Items = new NotificationList<CommandItemBase>()
            });
            menus.Insert(3, new CommandItem
            {
                IsRoot = true,
                Caption = "其它",
                Items = new NotificationList<CommandItemBase>()
            });
            if (item.Commands == null || item.Commands.Count == 0)
            {
                Buttons = new CommandItemBase[0];
                return;
            }

            var selType = Context.SelectConfig.GetType();
            bool preIsLine = false;
            foreach (var cmd in item.Commands)
            {
                if (cmd.IsLine)
                {
                    preIsLine = true;
                    continue;
                }
                if (cmd.SignleSoruce)
                {
                    if (cmd.TargetType != null && selType != cmd.TargetType && !selType.IsSubclassOf(cmd.TargetType))
                        continue;
                }
                else if (WorkView != null && cmd.ViewModel != null && !cmd.ViewModel.Contains(WorkView))
                {
                    continue;
                }
                string cl = cmd.Catalog ?? "其它";
                CommandItemBase sub;
                switch (cl)
                {
                    case "文件":
                        preIsLine = false;
                        fileMenu.Items.Insert(fileMenu.Items.Count - 2, cmd);
                        continue;
                    case "窗口":
                        sub = WindowMenu;
                        break;
                    default:
                        sub = menus.FirstOrDefault(p => p.Caption == cl);
                        if (sub == null)
                            menus.Insert(menus.Count - 2, sub = new CommandItem
                            {
                                IsRoot = true,
                                Name = cl,
                                Caption = cl,
                                Items = new NotificationList<CommandItemBase>()
                            });
                        break;
                }
                if (preIsLine)
                {
                    if (sub.Items.Count > 0)
                        sub.Items.Add(CommandItemBase.Line);
                    preIsLine = false;
                }
                sub.Items.Add(cmd);
            }

            List<CommandItemBase> buttons = new List<CommandItemBase>();
            foreach (var menu in menus)
            {
                foreach (var cmd in menu.Items.Where(p => !p.IsLine && !p.NoButton))
                {
                    buttons.Add(cmd);
                }
            }
            Buttons = buttons.ToArray();

        }


        CommandItemBase preJobItem;
        public void OnJobSelect(CommandItemBase item)
        {
            if (preJobItem != null)
                preJobItem.IsChecked = false;
            preJobItem = item;
            preJobItem.IsChecked = true;
            Screen.NowEditor = item.Caption;
            CheckWindow();
            DataModelDesignModel.SaveUserScreen();
        }
        #endregion

        #region 业务视角

        CommandItemBase preWorkViewItem;
        public void OnWorkView(CommandItemBase view)
        {
            if (preWorkViewItem != null)
                preWorkViewItem.IsChecked = false;
            preWorkViewItem = view;
            view.IsChecked = true;
            WorkView = view.Catalog;
            CheckWindow();
            DataModelDesignModel.SaveUserScreen();
        }

        /// <summary>
        /// 工作视角
        /// </summary>
        public string WorkView
        {
            get => SolutionConfig.Current?.WorkView;
            set
            {
                Screen.WorkView = value;
                if (SolutionConfig.Current != null)
                    SolutionConfig.Current.WorkView = value;
            }
        }

        public void OnAdvancedView(CommandItemBase view)
        {
            view.IsChecked = AdvancedView = !view.IsChecked;
        }
        /// <summary>
        /// 高级视角
        /// </summary>
        public bool AdvancedView
        {
            get => SolutionConfig.Current?.AdvancedView ?? false;
            set
            {
                Screen.AdvancedView = value;
                if (SolutionConfig.Current != null)
                    SolutionConfig.Current.AdvancedView = value;
                DataModelDesignModel.SaveUserScreen();
            }
        }

        /// <summary>
        /// 用户操作的现场记录
        /// </summary>
        public static UserScreen Screen => DataModelDesignModel.Screen;

        #endregion

        #region EditPanel

        /// <summary>
        ///     属性表格
        /// </summary>
        internal PropertyGrid PropertyGrid { get; set; }

        /// <summary>
        ///     可用工作列表
        /// </summary>
        internal Dictionary<string, int> RootJobs { get; } = new Dictionary<string, int>
        {
            {EditorInfo,0},
            {EditorConfig,0},
            {EditorPropertyGrid,0},
            {EditorCode,0},
            {EditorTrace,0}
        };

        public const string EditorInfo = "基本信息";
        public const string EditorConfig = "对象设计";
        public const string EditorCode = "代码生成";
        public const string EditorPropertyGrid = "属性表格";
        public const string EditorTrace = "跟踪消息";

        /// <summary>
        /// 当前编辑器
        /// </summary>
        public string NowEditor => Screen.NowEditor ?? EditorPropertyGrid;

        /// <summary>
        /// 临时显示跟踪窗口
        /// </summary>
        public void ShowTrace()
        {
            ExtendPanelVisibility = Visibility.Collapsed;
            ConfigFormVisibility = Visibility.Collapsed;
            CodePanelVisibility = Visibility.Collapsed;
            TracePanelVisibility = Visibility.Visible;
            PropertyPageVisibility = Visibility.Collapsed;
            RaisePropertyChanged(nameof(ExtendPanelVisibility));
            RaisePropertyChanged(nameof(ConfigFormVisibility));
            RaisePropertyChanged(nameof(CodePanelVisibility));
            RaisePropertyChanged(nameof(TracePanelVisibility));
            RaisePropertyChanged(nameof(PropertyPageVisibility));
        }
        /// <summary>
        /// 临时显示代码窗口
        /// </summary>
        public void ShowCode()
        {
            ExtendPanelVisibility = Visibility.Collapsed;
            ConfigFormVisibility = Visibility.Collapsed;
            CodePanelVisibility = Visibility.Visible;
            TracePanelVisibility = Visibility.Collapsed;
            PropertyPageVisibility = Visibility.Collapsed;
            RaisePropertyChanged(nameof(ExtendPanelVisibility));
            RaisePropertyChanged(nameof(ConfigFormVisibility));
            RaisePropertyChanged(nameof(CodePanelVisibility));
            RaisePropertyChanged(nameof(TracePanelVisibility));
            RaisePropertyChanged(nameof(PropertyPageVisibility));
        }
        public Visibility ExtendPanelVisibility { get; set; } = Visibility.Collapsed;
        public Visibility ConfigFormVisibility { get; set; } = Visibility.Collapsed;
        public Visibility CodePanelVisibility { get; set; } = Visibility.Collapsed;
        public Visibility TracePanelVisibility { get; set; } = Visibility.Collapsed;
        public Visibility PropertyPageVisibility { get; set; } = Visibility.Collapsed;


        internal void CheckEditorVisibility()
        {
            if (WindowMenu != null)
            {
                var item = WindowMenu.Items.FirstOrDefault(p => string.Equals(p.Caption, NowEditor, StringComparison.OrdinalIgnoreCase));
                if (item != null && preJobItem != item)
                {
                    item.IsChecked = true;
                    if (preJobItem != null)
                        preJobItem.IsChecked = false;
                    preJobItem = item;
                }

            }
            if (viewMenu != null)
            {
                var item = viewMenu.Items.FirstOrDefault(p => string.Equals(p.Catalog, WorkView, StringComparison.OrdinalIgnoreCase));
                if (item != null && preWorkViewItem != item)
                {
                    item.IsChecked = true;
                    if (preWorkViewItem != null)
                        preWorkViewItem.IsChecked = false;
                    preWorkViewItem = item;
                }

            }
            switch (NowEditor)
            {
                case EditorInfo:
                    ExtendPanelVisibility = Visibility.Collapsed;
                    ConfigFormVisibility = Visibility.Visible;
                    CodePanelVisibility = Visibility.Collapsed;
                    TracePanelVisibility = Visibility.Collapsed;
                    PropertyPageVisibility = Visibility.Collapsed;
                    break;
                case EditorCode:
                    ExtendPanelVisibility = Visibility.Collapsed;
                    ConfigFormVisibility = Visibility.Collapsed;
                    CodePanelVisibility = Visibility.Visible;
                    TracePanelVisibility = Visibility.Collapsed;
                    PropertyPageVisibility = Visibility.Collapsed;
                    break;
                case EditorPropertyGrid:
                    ExtendPanelVisibility = Visibility.Collapsed;
                    ConfigFormVisibility = Visibility.Collapsed;
                    CodePanelVisibility = Visibility.Collapsed;
                    TracePanelVisibility = Visibility.Collapsed;
                    PropertyPageVisibility = Visibility.Visible;
                    break;
                case EditorTrace:
                    ExtendPanelVisibility = Visibility.Collapsed;
                    ConfigFormVisibility = Visibility.Collapsed;
                    CodePanelVisibility = Visibility.Collapsed;
                    TracePanelVisibility = Visibility.Visible;
                    PropertyPageVisibility = Visibility.Collapsed;
                    break;
                case EditorConfig:
                    ExtendPanelVisibility = HaseExtend ? Visibility.Visible : Visibility.Collapsed;
                    ConfigFormVisibility = HaseExtend ? Visibility.Collapsed : Visibility.Visible;
                    CodePanelVisibility = Visibility.Collapsed;
                    TracePanelVisibility = Visibility.Collapsed;
                    PropertyPageVisibility = Visibility.Collapsed;
                    break;
                default:
                    return;
            }
            RaisePropertyChanged(nameof(ExtendPanelVisibility));
            RaisePropertyChanged(nameof(ConfigFormVisibility));
            RaisePropertyChanged(nameof(CodePanelVisibility));
            RaisePropertyChanged(nameof(TracePanelVisibility));
            RaisePropertyChanged(nameof(PropertyPageVisibility));
        }

        #endregion

    }
}