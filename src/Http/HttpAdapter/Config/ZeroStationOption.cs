using System;
using System.Runtime.Serialization;

namespace Agebull.MicroZero
{
    /// <summary>
    ///     本地站点配置
    /// </summary>
    [Serializable]
    public class ZeroStationOption
    {
        /// <summary>
        ///   API使用路由模式（而非默认的Push/Pull)
        /// </summary>
        [IgnoreDataMember]
        public bool ApiRouterModel { get; set; }


        /// <summary>
        ///     ApiClient与ApiStation限速模式
        /// </summary>
        /// <remarks>
        ///     Single：单线程无等待
        ///     ThreadCount:按线程数限制,线程内无等待
        ///     线程数计算公式 : 机器CPU数量 X TaskCpuMultiple 最小为1,请合理设置并测试
        ///     WaitCount: 单线程,每个请求起一个新Task,直到最高未完成数量达MaxWait时,
        ///     ApiClient休眠直到等待数量 低于 MaxWait
        ///     ApiStation返回服务器忙(熔断)
        /// </remarks>
        [DataMember]
        public SpeedLimitType SpeedLimitModel { get; set; }

        /// <summary>
        ///     最大等待数(0xFF-0xFFFFF)
        /// </summary>
        [DataMember]
        public int MaxWait { get; set; }


        /// <summary>
        ///     最大Task与Cpu核心数的倍数关系(0-128)
        /// </summary>
        [DataMember]
        public decimal TaskCpuMultiple { get; set; }

        /// <summary>
        ///     API最大执行超时时间(单位秒)
        /// </summary>
        [DataMember]
        public int ApiTimeout { get; set; }

        /// <summary>
        ///     检测到API执行超时是否强行清除资源
        /// </summary>
        [DataMember]
        public bool ApiTimeoutKill { get; set; }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="option"></param>
        public void CopyByEmpty(ZeroStationOption option)
        {
            if (SpeedLimitModel == SpeedLimitType.None)
                SpeedLimitModel = option.SpeedLimitModel;
            if (TaskCpuMultiple <= 0)
                TaskCpuMultiple = option.TaskCpuMultiple;
            if (MaxWait <= 0)
                MaxWait = option.MaxWait;
            if (ApiTimeout <= 0)
                ApiTimeout = option.ApiTimeout;
            if (!ApiTimeoutKill)
                ApiTimeoutKill = option.ApiTimeoutKill;
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="option"></param>
        public void CopyByHase(ZeroStationOption option)
        {
            if (option.SpeedLimitModel > SpeedLimitType.None)
                SpeedLimitModel = option.SpeedLimitModel;
            if (option.TaskCpuMultiple > 0)
                TaskCpuMultiple = option.TaskCpuMultiple;
            if (option.MaxWait > 0)
                MaxWait = option.MaxWait;
            if (option.ApiTimeout > 0)
                ApiTimeout = option.ApiTimeout;
            if (option.ApiTimeoutKill)
                ApiTimeoutKill = option.ApiTimeoutKill;
            if (option.ApiTimeoutKill)
                ApiTimeoutKill = true;
        }

    }
}