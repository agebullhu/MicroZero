using System;
using System.Windows;
using System.Windows.Media;
using Agebull.EntityModel;

namespace Agebull.Common.Mvvm
{
    /// <summary>
    /// 表示一个命令生成器
    /// </summary>
    public class CommandConfig : NotificationObject, ICommandItem
    {
        /// <summary>
        ///     名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        ///     标题
        /// </summary>
        public string Caption
        {
            get;
            set;
        }

        /// <summary>
        ///     说明
        /// </summary>
        public string Description
        {
            get;
            set;
        }
        /// <summary>
        ///     显示为按钮
        /// </summary>
        public bool IsButton
        {
            get;
            set;
        }

        /// <summary>
        ///     不显示为按钮
        /// </summary>
        public bool NoButton => !IsButton;

        /// <summary>
        ///     图标
        /// </summary>
        public string IconName
        {
            get;
            set;
        }

        /// <summary>
        ///     ViewModel
        /// </summary>
        public string ViewModel
        {
            get;
            set;
        }

        /// <summary>
        ///     面对的编辑器
        /// </summary>
        public string Editor
        {
            get;
            set;
        }

        /// <summary>
        ///     只能单个操作
        /// </summary>
        public bool SignleSoruce
        {
            get;
            set;
        }

        /// <summary>
        ///     分类
        /// </summary>
        public string Catalog
        {
            get;
            set;
        }

        private Type _targetType;
        /// <summary>
        ///     目标类型
        /// </summary>
        public Type TargetType
        {
            get => SignleSoruce ? _targetType : SuppertType ?? _targetType;
            set => _targetType = value;
        }


        /// <summary>
        ///     代替类型
        /// </summary>
        public virtual Type SuppertType => null;

        /// <summary>
        /// 确认消息
        /// </summary>
        public string ConfirmMessage { get; set; }

        /// <summary>
        /// 无需确认
        /// </summary>
        public bool NoConfirm { get; set; }

        /// <summary>
        /// 无需确认
        /// </summary>
        public bool DoConfirm => !string.IsNullOrEmpty(ConfirmMessage);
    }
}