namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 中心事件
    /// </summary>
    public enum ZeroNetEventType
    {
        /// <summary>
        /// 没有事件
        /// </summary>
        None,
        /// <summary>
        /// 
        /// </summary>
        CenterSystemStart,
        /// <summary>
        /// 
        /// </summary>
        CenterSystemClosing,
        /// <summary>
        /// 
        /// </summary>
        CenterSystemStop,
        /// <summary>
        /// 
        /// </summary>
        CenterWorkerSoundOff,
        /// <summary>
        /// 
        /// </summary>
        CenterStationJoin,
        /// <summary>
        /// 
        /// </summary>
        CenterStationLeft,
        /// <summary>
        /// 
        /// </summary>
        CenterStationPause,
        /// <summary>
        /// 
        /// </summary>
        CenterStationResume,
        /// <summary>
        /// 
        /// </summary>
        CenterStationClosing,
        /// <summary>
        /// 
        /// </summary>
        CenterStationInstall,
        /// <summary>
        /// 
        /// </summary>
        CenterStationUninstall,
        /// <summary>
        /// 
        /// </summary>
        CenterStationState,

        /// <summary>
        /// 
        /// </summary>
        AppRun,
        /// <summary>
        /// 
        /// </summary>
        AppStop,
        /// <summary>
        /// 
        /// </summary>
        AppEnd
    }
}