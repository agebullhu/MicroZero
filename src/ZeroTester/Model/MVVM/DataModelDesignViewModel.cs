// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.CodeRefactor.CodeAnalyze.Application
// 建立:2014-11-20
// 修改:2014-11-27
// *****************************************************/

#region 引用

using System.Collections;
using System.Windows;
using System.Windows.Media;
using Agebull.Common.Mvvm;

#endregion

namespace Agebull.EntityModel.Designer
{
    public sealed class DataModelDesignViewModel : ViewModelBase<DataModelDesignModel>, IGridSelectionBinding
    {
        public NotificationList<CommandItemBase> UserCommands => new NotificationList<CommandItemBase>
        {
            new CommandItem
            {
                IsButton = true,
                Action = Model.Login,
                Caption = "登录",
                Image = Application.Current.Resources["img_file"] as ImageSource
            },
            new CommandItem
            {
                IsButton = true,
                Action = Model.LoadDeviceId,
                Caption = "获取DeviceId",
                Image = Application.Current.Resources["img_file"] as ImageSource
            },
            new CommandItem
            {
                IsButton = true,
                Action = Model.VerifyAccessToken,
                Caption = "更换AT",
                Image = Application.Current.Resources["img_file"] as ImageSource
            },
            new CommandItem
            {
                IsButton = true,
                Action = Model.Anymouse,
                Caption = "匿名用户",
                Image = Application.Current.Resources["img_file"] as ImageSource
            },
            new CommandItem
            {
                IsButton = true,
                Action = Model.Customer,
                Caption = "登录用户",
                Image = Application.Current.Resources["img_file"] as ImageSource
            },
            new CommandItem
            {
                IsButton = true,
                Action = Model.NoUser,
                Caption = "无用户",
                Image = Application.Current.Resources["img_file"] as ImageSource
            }
        };

        /// <summary>
        ///     生成命令对象
        /// </summary>
        /// <returns></returns>
        protected override NotificationList<CommandItemBase> CreateCommands()
        {
            var commands = new NotificationList<CommandItemBase>
            {
                new CommandItem
                {
                    IsButton = true,
                    Action = Model.TestApi,
                    Caption = "执行",
                    Image = Application.Current.Resources["img_file"] as ImageSource
                },
                new CommandItem
                {
                    IsButton = true,
                    Action = Model.ResetArgument,
                    Caption = "重置参数",
                    Image = Application.Current.Resources["img_file"] as ImageSource
                }
            };
            return commands;
        }

        #region 基础

        public IList SelectColumns
        {
            get;
            set; //Model.Context.SelectConfigs = value;
        }

        /// <summary>
        ///     视图绑定成功后的初始化动作
        /// </summary>
        protected override void OnViewSeted()
        {
            GlobalTrigger.Dispatcher = Dispatcher;
            base.OnViewSeted();
            Model.OnSolutionChanged();
        }

        #endregion
    }
}