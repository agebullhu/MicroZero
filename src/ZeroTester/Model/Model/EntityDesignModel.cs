using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Agebull.EntityModel.Config;
using Agebull.Common.Mvvm;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    /// 实体配置相关模型
    /// </summary>
    public class EntityDesignModel : DesignModelBase
    {
        #region 操作命令

        public EntityDesignModel()
        {
            Model = DataModelDesignModel.Current;
            Context = DataModelDesignModel.Current?.Context;
        }

        /// <summary>
        /// 生成命令对象
        /// </summary>
        /// <returns></returns>
        public override NotificationList<CommandItemBase> CreateCommands()
        {
            return CreateCommands(true, true, true);
        }

        /// <summary>
        /// 生成命令对象
        /// </summary>
        /// <returns></returns>
        protected NotificationList<CommandItemBase> CreateCommands(bool edit, bool create, bool ext)
        {
            NotificationList<CommandItemBase> commands = new NotificationList<CommandItemBase>();
            if (edit)
            {
                commands.Add(new CommandItem
                {
                    IsButton = true,
                    NoConfirm = true,
                    Action = CopyColumns,
                    Caption = "复制列",
                    Image = Application.Current.Resources["tree_item"] as ImageSource
                });
                commands.Add(new CommandItem
                {
                    IsButton = true,
                    Action = PasteColumns,
                    NoConfirm = true,
                    Caption = "粘贴列",
                    Image = Application.Current.Resources["tree_item"] as ImageSource
                });
                commands.Add(new CommandItem
                {
                    IsButton = true,
                    Action = ClearColumns,
                    Caption = "清除列",
                    Image = Application.Current.Resources["img_del"] as ImageSource
                });
                commands.Add(new CommandItem
                {
                    IsButton = true,
                    Action = DeleteColumns,
                    Caption = "删除所选列",
                    Image = Application.Current.Resources["img_del"] as ImageSource
                });
                commands.Add(new CommandItem
                {
                    IsButton = true,
                    Action = AddProperty,
                    NoConfirm = true,
                    Caption = "新增字段",
                    Image = Application.Current.Resources["tree_Open"] as ImageSource
                });
            }
            if (create)
                CreateCommands(commands);
            if (ext)
            {
                var extends = CommandCoefficient.CoefficientEditor(typeof(EntityConfig), EditorName);
                if (extends.Count > 0)
                    commands.AddRange(extends);
            }
            return commands;
        }

        #endregion
        /// <summary>
        /// 复制字段
        /// </summary>
        public void AddProperty(object arg)
        {
            var perperty = new PropertyConfig();
            if (CommandIoc.NewConfigCommand("新增字段", perperty))
                Context.SelectEntity.Add(perperty);
        }

        /// <summary>
        /// 复制字段
        /// </summary>
        public void CopyColumns(object arg)
        {
            Context.CopiedTable = Context.SelectEntity;
            Context.CopyColumns = Context.SelectColumns.OfType<PropertyConfig>().ToList();
            Context.StateMessage = $"复制了{Context.CopyColumns.Count}行";
        }

        /// <summary>
        /// 复制字段
        /// </summary>
        public void PasteColumns(object arg)
        {
            if (Context.CopyColumns == null || Context.CopyColumns.Count == 0 || Context.CopiedTable == Context.SelectEntity ||
                Context.SelectEntity == null || Context.CopiedTable == null)
            {
                Context.StateMessage = "没可粘贴的行";
                return;
            }
            var yes = MessageBox.Show("是否粘贴关系信息?", "粘贴行", MessageBoxButton.YesNo) ==
                      MessageBoxResult.Yes;

            PateFields(yes, Context.CopiedTable, Context.SelectEntity, Context.CopyColumns);

            //this.Context.CopiedTable = null;
            //this.Context.CopiedTables.Clear();
            //Context.SelectColumns = null;
            //this.RaisePropertyChanged(() => this.Context.CopiedTableCounts);
        }
        public void ClearColumns(object arg)
        {
            if (Context.SelectEntity == null ||
                MessageBox.Show("确认删除所有字段吗?", "对象编辑", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }
            Context.SelectColumns = null;
            Context.SelectEntity.Properties.Clear();
        }

        public void DeleteColumns(object arg)
        {
            if (Context.SelectEntity == null || Context.SelectColumns == null ||
                MessageBox.Show("确认删除所选字段吗?", "对象编辑", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }
            foreach (var col in Context.SelectColumns.OfType<PropertyConfig>().ToArray())
            {
                Context.SelectEntity.Properties.Remove(col);
            }
            Context.SelectColumns = null;
        }

        #region 复制

        public void PateFields(bool toReference, EntityConfig source, EntityConfig Entity, List<PropertyConfig> columns)
        {
            foreach (var copyColumn in columns)
            {
                var refe = toReference && !copyColumn.Parent.IsInterface;
                PropertyConfig newColumn = null;
                if (refe)
                {
                    newColumn = Entity.Properties.FirstOrDefault(p => p.ReferenceKey == copyColumn.Key);
                    if (newColumn == null)
                    {
                        string name = copyColumn.Parent.Name;
                        if (copyColumn.IsPrimaryKey)
                        {
                            newColumn = Entity.Properties.FirstOrDefault(p => p.LinkTable == name && p.IsLinkKey);
                        }
                        else if (copyColumn.IsCaption)
                        {
                            newColumn = Entity.Properties.FirstOrDefault(p => p.LinkTable == name && p.IsLinkCaption);
                        }
                        else
                        {
                            newColumn = Entity.Properties.FirstOrDefault(
                                p => string.Equals(p.LinkTable, name, StringComparison.OrdinalIgnoreCase) && (
                                         string.Equals(p.LinkField, copyColumn.Name, StringComparison.OrdinalIgnoreCase) ||
                                         string.Equals(p.LinkField, copyColumn.DbFieldName, StringComparison.OrdinalIgnoreCase)));
                        }
                    }
                }
                if (newColumn == null)
                {
                    newColumn = new PropertyConfig();
                    newColumn.CopyFromProperty(copyColumn, false, true, true);
                    newColumn.Option.Index = newColumn.Option.Identity = 0;
                    
                    if (refe && !copyColumn.IsLinkField)
                    {
                        if (copyColumn.IsPrimaryKey)
                        {
                            newColumn.Name = copyColumn.Parent.Name + "Id";
                            newColumn.Caption = copyColumn.Parent.Caption + "外键";
                        }
                        else if (copyColumn.IsCaption)
                        {
                            newColumn.Name = copyColumn.Parent.Name;
                            newColumn.Caption = copyColumn.Parent.Caption;
                        }
                        newColumn.Option.IsLink = true;
                        newColumn.Option.ReferenceConfig = copyColumn;
                        newColumn.DbFieldName = DataBaseHelper.ToDbFieldName(newColumn.Name);
                    }
                    Entity.Add(newColumn);
                }
                if (refe)
                {
                    if (copyColumn.IsLinkField)
                    {
                        newColumn.Option.IsLink = true;
                        newColumn.Option.ReferenceConfig = copyColumn.Option.ReferenceConfig;
                    }
                    else
                    {
                        newColumn.IsLinkField = true;
                        newColumn.Option.ReferenceConfig = copyColumn;
                        newColumn.LinkTable = source.Name;
                        newColumn.LinkField = copyColumn.Name;
                        if (copyColumn.IsLinkKey || copyColumn.IsPrimaryKey)
                        {
                            newColumn.IsLinkKey = true;
                        }
                        else if (copyColumn.IsCaption)
                        {
                            newColumn.IsLinkCaption = true;
                            newColumn.IsCompute = true;
                        }
                    }
                }
                newColumn.Parent = Entity;
                newColumn.IsPrimaryKey = false;
                newColumn.IsCaption = false;
                if (newColumn.IsLinkKey)
                    newColumn.NoneApiArgument = true;
            }
            Entity.IsModify = true;
        }

        #endregion
    }

}