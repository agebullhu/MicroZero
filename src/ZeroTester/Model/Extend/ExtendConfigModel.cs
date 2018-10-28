using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Agebull.EntityModel.Config;
using Agebull.Common.Mvvm;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    /// 扩展属性模型
    /// </summary>
    public class ExtendConfigModel : DesignModelBase
    {
        public void DeleteEnum(ConfigTreeItem<PropertyConfig> p)
        {
            if (p.Model.EnumConfig != null)
            {
                p.Model.EnumConfig.Option.IsDelete = true;
            }
            p.Model.CustomType = null;
            p.ReShow();
        }
        /// <summary>
        /// 扩展属性
        /// </summary>
        public NotificationList<ConfigItem> ExtendItems { get; private set; }

        /// <summary>
        /// 上下文属性变化
        /// </summary>
        protected override void OnContextPropertyChanged(string name)
        {
            switch (name)
            {
                case "SelectConfig":
                    UpdateItems();
                    break;
            }
        }

        private void UpdateItems()
        {
            ExtendItems = Context.SelectConfig?.Option.ExtendConfigList.Items;
            RaisePropertyChanged(nameof(ExtendItems));
        }

        /// <summary>
        /// 生成命令对象
        /// </summary>
        /// <returns></returns>
        public override NotificationList<CommandItemBase> CreateCommands()
        {
            return new NotificationList<CommandItemBase>
            {
                new CommandItem
                {
                    Action = Add,
                    Caption = "增加",
                    Image = Application.Current.Resources["tree_item"] as ImageSource
                }
            };
        }
        public string NewName { get; set; }

        private void Add(object arg)
        {
            if (Context.SelectConfig == null)
                return;
            if (Context.SelectConfig == null)
            {
                MessageBox.Show("添加", "当前配置不可用");
                return;
            }
            if (string.IsNullOrWhiteSpace(NewName))
            {
                MessageBox.Show("添加", "新名称不能为空");
                return;
            }
            if (Context.SelectConfig.Option.ExtendConfigList.Items.Any(p=>
                string.Equals(p.Name, NewName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("添加", "已存在同名内容");
                return;
            }
            Context.SelectConfig.Option[NewName] = "";
        }
    }
}
