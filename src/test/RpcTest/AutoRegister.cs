using System;
using System.ComponentModel.Composition;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.Extensions.DependencyInjection;
using Gboxt.Common.DataModel.ZeroNet;
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
            Console.WriteLine("Initialize");
            ApiContext.ServiceCollection.AddSingleton<ILogRecorder, RemoteLogRecorder>();
            ApiContext.ServiceCollection.AddSingleton<IEntityEventProxy, EntityEventProxy>();
        }

        /// <summary>
        /// 注册
        /// </summary>
        void IAutoRegister.AutoRegist()
        {
            ZeroApplication.RegistZeroObject(ApiCounter.Instance);
        }
    }
}