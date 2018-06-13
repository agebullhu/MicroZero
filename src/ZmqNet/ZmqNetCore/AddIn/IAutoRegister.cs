namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 生自注册对象
    /// </summary>
    public interface IAutoRegister
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();


        /// <summary>
        /// 执行自动注册
        /// </summary>
        void AutoRegist();
    }
}