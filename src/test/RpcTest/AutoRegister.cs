using System.ComponentModel.Composition;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;


namespace Agebull.MicroZero.Log
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
            IocHelper.ServiceCollection.AddSingleton<Tester, HttpTester>();
        }

        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.AutoRegist()
        {
            ZeroApplication.RegistZeroObject(TestEventProxy.Instance);
        }
    }
}