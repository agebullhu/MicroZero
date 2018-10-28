using System;
using Agebull.EntityModel.Config;

namespace Agebull.Common.Mvvm
{
    /// <summary>
    /// 表示一个命令生成器
    /// </summary>
    public interface ICommandItem
    {
        /// <summary>
        ///     名称
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        ///     标题
        /// </summary>
        string Caption
        {
            get;
            set;
        }

        /// <summary>
        ///     说明
        /// </summary>
        string Description
        {
            get;
            set;
        }
        /// <summary>
        ///     显示为按钮
        /// </summary>
        bool IsButton
        {
            get;
            set;
        }
        /// <summary>
        ///     不显示为按钮
        /// </summary>
        bool NoButton { get; }

        /// <summary>
        ///     分类
        /// </summary>
        string Catalog { get; set; }

        /// <summary>
        ///     视角
        /// </summary>
        string ViewModel { get; set; }

        /// <summary>
        ///     面对的编辑器
        /// </summary>
        string Editor
        {
            get;
            set;
        }

        /// <summary>
        ///     只能单个操作
        /// </summary>
        bool SignleSoruce { get; set; }

        /// <summary>
        ///     目标类型
        /// </summary>
        Type TargetType { get; set; }

        /// <summary>
        ///     图标
        /// </summary>
        string IconName { get; set; }

        /// <summary>
        /// 确认消息
        /// </summary>
        string ConfirmMessage { get; set; }

        /// <summary>
        ///     无需确认
        /// </summary>
        bool NoConfirm
        {
            get;
            set;
        }

    }

    /// <summary>
    /// 表示一个命令生成器
    /// </summary>
    public static class ICommandItemExtend
    {
        /// <summary>
        /// 从源中复制
        /// </summary>
        /// <param name="dest">目标</param>
        /// <param name="sour">源</param>
        public static void CopyFrom(this ICommandItem dest, ICommandItem sour)
        {
            dest.NoConfirm = sour.NoConfirm;
            dest.Name = sour.Name ?? sour.Caption;
            dest.Caption = sour.Caption?? sour.Name;
            dest.Description = sour.Description;
            dest.IsButton = sour.IsButton;
            dest.SignleSoruce = sour.SignleSoruce;
            dest.Catalog = sour.Catalog;
            dest.ViewModel = sour.ViewModel;
            dest.TargetType = sour.TargetType;
            dest.IconName = sour.IconName;
            dest.ConfirmMessage = sour.ConfirmMessage;
        }
    }
}