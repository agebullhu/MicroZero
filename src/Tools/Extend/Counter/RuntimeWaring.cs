using System.Runtime.Serialization;
using Agebull.Common;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 运行时警告
    /// </summary>
    internal class RuntimeWaring : SignlePublisher<WaringItem>, IRuntimeWaring
    {
        /// <summary>
        /// 构造
        /// </summary>
        private RuntimeWaring()
        {
            Name = "RuntimeWaring";
            StationName = "HealthCenter";
        }
        /// <summary>
        /// 单例
        /// </summary>
        internal static RuntimeWaring Instance = new RuntimeWaring();

        /// <summary>
        /// 运行时警告
        /// </summary>
        /// <param name="host"></param>
        /// <param name="api"></param>
        /// <param name="message"></param>
        void IRuntimeWaring.Waring(string host, string api, string message)
        {
            Publish(new WaringItem
            {
                Machine = ZeroApplication.Config.StationName,
                User = ApiContext.Customer?.Account ?? "Unknow",
                RequestId = GlobalContext.RequestInfo.RequestId,
                Host = host,
                Api = api,
                Message = message
            });
        }
    }
}