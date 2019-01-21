using System;
using System.ComponentModel.Composition;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Microsoft.Extensions.DependencyInjection;
using ZeroNet.Http.Route;
using Agebull.Common.Configuration;

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

            var sec = ConfigurationManager.Get("AppSettings");
            foreach (var k in sec.Configuration.GetChildren())
                Console.WriteLine(k.Key );
            if (ConfigurationManager.AppSettings.GetBool("RemoteLog"))
                IocHelper.ServiceCollection.AddSingleton<ILogRecorder>(provider => RemoteLogRecorder.Instance);
            if (ConfigurationManager.AppSettings.GetBool("RuntimeWaring"))
                IocHelper.ServiceCollection.AddSingleton<IRuntimeWaring>(provider => RuntimeWaring.Instance);
            if (ConfigurationManager.AppSettings.GetBool("ApiCount"))
                IocHelper.ServiceCollection.AddSingleton<IApiCounter>(provider => ApiCounter.Instance);
        }
        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.AutoRegist()
        {
            if (ConfigurationManager.AppSettings.GetBool("RemoteLog"))
                ZeroApplication.RegistZeroObject(RemoteLogRecorder.Instance);
            if (ConfigurationManager.AppSettings.GetBool("RuntimeWaring"))
                ZeroApplication.RegistZeroObject(RuntimeWaring.Instance);
            if (ConfigurationManager.AppSettings.GetBool("ApiCount"))
                ZeroApplication.RegistZeroObject(ApiCounter.Instance);
        }
    }
}