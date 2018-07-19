using System;
using System.ComponentModel.Composition;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.Extensions.DependencyInjection;
using Gboxt.Common.DataModel.ZeroNet;
using ZeroNet.Http.Route;
using Agebull.Common.Configuration;
using Gboxt.Common.DataModel.ExtendEvents;

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
            if (ConfigurationManager.AppSettings.GetBool("RemoteLog", false))
                IocHelper.ServiceCollection.AddSingleton<ILogRecorder>(provider => RemoteLogRecorder.Instance);
            if (ConfigurationManager.AppSettings.GetBool("RuntimeWaring", false))
                IocHelper.ServiceCollection.AddSingleton<IRuntimeWaring>(provider => RuntimeWaring.Instance);
            if (ConfigurationManager.AppSettings.GetBool("ApiCount", false))
                IocHelper.ServiceCollection.AddSingleton<IApiCounter>(provider => ApiCounter.Instance);
            if (ConfigurationManager.AppSettings.GetBool("EntityEvent", false))
                IocHelper.ServiceCollection.AddSingleton<IEntityEventProxy>(provider => EntityEventProxy.Instance);
        }
        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.AutoRegist()
        {
            if (ConfigurationManager.AppSettings.GetBool("RemoteLog", false))
                ZeroApplication.RegistZeroObject(RemoteLogRecorder.Instance);
            if (ConfigurationManager.AppSettings.GetBool("RuntimeWaring", false))
                ZeroApplication.RegistZeroObject(RuntimeWaring.Instance);
            if (ConfigurationManager.AppSettings.GetBool("ApiCount", false))
                ZeroApplication.RegistZeroObject(ApiCounter.Instance);
            if (ConfigurationManager.AppSettings.GetBool("EntityEvent", false))
                ZeroApplication.RegistZeroObject(EntityEventProxy.Instance);
        }
    }
}