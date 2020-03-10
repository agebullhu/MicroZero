using System;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     缓存特征
    /// </summary>
    [Flags]
    public enum CacheFeature : uint
    {
        /// <summary>
        ///     无
        /// </summary>
        None,

        /// <summary>
        ///     身份头相同
        /// </summary>
        Bear = 0x1,

        /// <summary>
        ///     发生网络错误时
        /// </summary>
        NetError = 0x2,

        /// <summary>
        ///     参数相同（Get与POST请求）
        /// </summary>
        QueryString = 0x4,

        /// <summary>
        ///     表单相同(POST请求）
        /// </summary>
        Form = 0x8,

        /// <summary>
        /// 使用键设置,忽略其它特性
        /// </summary>
        Keys = 0x10
    }
}