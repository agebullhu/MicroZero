using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Agebull.Common.Logging;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZeroMQ;

namespace ProxyTest
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ZeroApplication.WorkModel = ZeroWorkModel.Client;
            ZeroApplication.CheckOption();
            ZeroApplication.Initialize();
            ZeroApplication.Run();
            while (Console.ReadKey().Key != ConsoleKey.Q)
                CheckToken("#D3BF2A12F0A046C38220C5B7C2F56DF2", out var json);
            Console.WriteLine("Bye World!");
            ZeroApplication.Shutdown();
        }
        private static ApiResult CheckToken(string token, out string json)
        {
            // 远程调用
            using (MonitorScope.CreateScope($"Check:{token}"))
            {
                var caller = new ApiClient
                {
                    Simple = true,
                    Station = "Auth",
                    Commmand = "v1/verify/at",
                    Argument = $"{{\"Token\":\"{token}\"}}"
                };
                caller.CallCommand();

                json = caller.Result;

                LogRecorder.MonitorTrace($"Result:{caller.Result}");
                if (caller.Result == null)
                    return null;
                GlobalContext.Current.Request.Token = token;
                var result = JsonConvert.DeserializeObject<ApiResult>(caller.Result);

                var context = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(GlobalContext.Current)) as JObject;
                if (result.Success)
                {
                    JObject obj = JsonConvert.DeserializeObject(json) as JObject;
                    context["user"] = obj["data"];
                }
                LogRecorder.MonitorTrace(context.ToString());
                return result;
            }
        }
    }
}