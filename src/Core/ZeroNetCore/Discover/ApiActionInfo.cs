using Gboxt.Common.DataModel;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     Api方法的信息
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class ApiActionInfo : ApiDocument
    {
        /// <summary>
        /// 所在控制器类型
        /// </summary>
        public string Controller;

        /// <summary>
        ///     是否有调用参数
        /// </summary>
        public bool HaseArgument;

        /// <summary>
        ///     无参方法
        /// </summary>
        public Func<IApiResult> Action;

        /// <summary>
        ///     有参方法
        /// </summary>
        public Func<IApiArgument, IApiResult> ArgumentAction;

        /// <summary>
        ///     参数类型
        /// </summary>
        public Type ArgumenType;
    }
}