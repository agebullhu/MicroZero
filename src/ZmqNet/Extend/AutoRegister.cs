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
            IocHelper.ServiceCollection.AddSingleton<ILogRecorder>(provider => RemoteLogRecorder.Instance);
            IocHelper.ServiceCollection.AddSingleton<IEntityEventProxy>(provider => EntityEventProxy.Instance);
            IocHelper.ServiceCollection.AddSingleton<IApiCounter>(provider => ApiCounter.Instance);
            IocHelper.ServiceCollection.AddSingleton<IRuntimeWaring>(provider => RuntimeWaring.Instance);
        }

        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.AutoRegist()
        {
            ZeroApplication.RegistZeroObject(RemoteLogRecorder.Instance);
            ZeroApplication.RegistZeroObject(RuntimeWaring.Instance);
            ZeroApplication.RegistZeroObject(EntityEventProxy.Instance);
        }
    }
}