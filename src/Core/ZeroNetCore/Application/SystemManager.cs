using Agebull.Common.Context;


namespace Agebull.MicroZero.ZeroManagemant
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public class SystemManager : HeartManager
    {
        #region 实例

        /// <summary>
        /// 构造
        /// </summary>
        public static SystemManager CreateInstance()
        {
            return Instance = new SystemManager();
        }

        /// <summary>
        /// 构造
        /// </summary>
        private SystemManager()
        {
            ManageAddress = ZeroApplication.Config.Master.ManageAddress;
            ServiceKey = ZeroApplication.Config.Master.ServiceKey.ToZeroBytes();
        }

        /// <summary>
        /// 单例
        /// </summary>
        public static SystemManager Instance { get; set; }

        #endregion

        #region 系统支持

        /// <summary>
        ///     连接到
        /// </summary>
        public bool PingCenter()
        {
            return ByteCommand(ZeroByteCommand.Ping);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public bool HeartLeft()
        {
            return HeartLeft("SystemManage", GlobalContext.ServiceRealName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public bool HeartReady()
        {
            return HeartReady("SystemManage", GlobalContext.ServiceRealName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public bool HeartJoin()
        {
            return HeartJoin("SystemManage", GlobalContext.ServiceRealName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public bool Heartbeat()
        {
            return Heartbeat("SystemManage", GlobalContext.ServiceRealName);
        }

        #endregion
    }
}