using System.ComponentModel.Composition;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
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
            if (!ConfigurationManager.Get("LogRecorder").GetBool("local", false))
            {
                ZeroTrace.SystemLog("RemoteLogRecorder", "IAutoRegister.Initialize");
                IocHelper.ServiceCollection.AddTransient<ILogRecorder>(provider => RemoteLogRecorder.Instance);
                IocHelper.Update();
                LogRecorderX.Initialize();
            }
            //IocHelper.Update();
            //LogRecorderX.Initialize();
            //if (ConfigurationManager.AppSettings.GetBool("RuntimeWaring"))
            //    IocHelper.ServiceCollection.AddSingleton<IRuntimeWaring>(provider => RuntimeWaring.Instance);
            //if (ConfigurationManager.AppSettings.GetBool("ApiCount"))
            //    IocHelper.ServiceCollection.AddSingleton<IApiCounter>(provider => ApiCounter.Instance);
        }
        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.AutoRegist()
        {
            //if (ConfigurationManager.AppSettings.GetBool("RemoteLog"))
            ZeroApplication.RegistZeroObject(RemoteLogRecorder.Instance);
            //if (ConfigurationManager.AppSettings.GetBool("RuntimeWaring"))
            //    ZeroApplication.RegistZeroObject(RuntimeWaring.Instance);
            //if (ConfigurationManager.AppSettings.GetBool("ApiCount"))
            //    ZeroApplication.RegistZeroObject(ApiCounter.Instance);
        }
    }
}