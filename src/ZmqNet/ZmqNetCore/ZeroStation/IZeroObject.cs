using System;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点连接池
    /// </summary>
    public interface IZeroObject : IDisposable
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     运行状态
        /// </summary>
        int State { get; }

        /// <summary>
        /// 系统初始化时调用
        /// </summary>
        void OnZeroInitialize();

        /// <summary>
        /// 系统启动时调用
        /// </summary>
        void OnZeroStart();

        /// <summary>
        ///     要求心跳
        /// </summary>
        void OnHeartbeat();

        /// <summary>
        /// 系统关闭时调用
        /// </summary>
        void OnZeroEnd();

        /// <summary>
        /// 注销时调用
        /// </summary>
        void OnZeroDistory();

        /// <summary>
        /// 系统启动时调用
        /// </summary>
        void OnStationStateChanged(StationConfig config);
    }
}