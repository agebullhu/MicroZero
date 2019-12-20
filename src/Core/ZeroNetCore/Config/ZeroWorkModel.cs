namespace Agebull.MicroZero
{
    /// <summary>
    /// 工作模式
    /// </summary>
    public enum ZeroWorkModel
    {
        /// <summary>
        /// 服务模式（默认）
        /// </summary>
        Service,
        /// <summary>
        /// 客户模式，仅可调用
        /// </summary>
        Client,
        /// <summary>
        /// 反向桥接模式
        /// </summary>
        Bridge
    }
}