using System;

namespace Agebull.EntityModel
{
    /// <summary>
    ///     编辑的阻止模式
    /// </summary>
    [Flags]
    public enum EditArrestMode : short
    {
        /// <summary>
        ///     无
        /// </summary>
        None = 0x0,

        /// <summary>
        ///     内部处理修改前逻辑
        /// </summary>
        InnerCheck = 0x1,

        /// <summary>
        ///     发出属性修改前事件
        /// </summary>
        PropertyChangingEvent = 0x2,

        /// <summary>
        ///     内部处理修改后逻辑
        /// </summary>
        PropertyChanged = 0x4,

        /// <summary>
        ///     发出属性修改后事件
        /// </summary>
        PropertyChangedEvent = 0x8,

        /// <summary>
        ///     记录修改
        /// </summary>
        RecordChanged = 0x10,

        /// <summary>
        ///     内部逻辑处理
        /// </summary>
        InnerLogical = 0x20,

        /// <summary>
        ///     发出事件
        /// </summary>
        Events = PropertyChangingEvent | PropertyChangedEvent,

        /// <summary>
        ///     扩展操作
        /// </summary>
        ExtendProcess = InnerCheck | PropertyChanged,

        /// <summary>
        ///     阻止所有
        /// </summary>
        All = Events | ExtendProcess | InnerLogical
    }
}