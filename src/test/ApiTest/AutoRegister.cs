using System.ComponentModel.Composition;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace Agebull.ZeroNet.Log
{
    /// <summary>
    ///   远程记录器
    /// </summary>
    [Export(typeof(IAutoRegister))]
    [ExportMetadata("Symbol", '%')]
    public sealed class AutoRegister : IAutoRegister
    {  

        /// <summary>
        /// 初始化
        /// </summary>
        void IAutoRegister.Initialize()
        {
            ZeroApplication.Discove(typeof(AutoRegister).Assembly); 
        }

        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.AutoRegist()
        {

        }
    }
}