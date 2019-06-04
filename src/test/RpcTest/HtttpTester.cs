using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Agebull.MicroZero.ZeroApis;
using MicroZero.Http.Route;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace RpcTest
{
    internal class HttpTester : Tester
    {
        public override bool Init()
        {
            return true;
        }

        private List<KeyValuePair<string, string>> users = new Dictionary<string, string>
        {
            {"18821201689","123456"},
            {"13661603114","123456"},
            {"18552782087","123456"},
            {"15881466008","123456"},
            {"13636672479","123456"},
            {"13550741429","123456"},
            {"18036552210","123456"},
            {"17766476916","123456"},
            {"19981592839","123456"},
            {"18765543801","123456"},
            {"17738537122","123456"},
            {"13453926150","123456"},
            {"17621807007","123456"},
            {"15561301570","123456"},
            {"18870163935","123456"},
            {"18354960783","123456"},
            {"19938444305","123456"},
            {"17630303122","123456"},
            {"18759978195","123456"},
            {"15235916819","123456"},
            {"17502173667","123456"},
            {"17301894889","123456"},
            {"18296858121","123456"},
            {"18049939527","123456"},
            {"15070116118","123456"},
            {"18721306175","123456"},
            {"16602107168","123456"},
            {"13636606174","123456"},
            {"18208245547","123456"},
            {"18294884427","123456"},
            {"18321968817","123456"},
            {"18175466613","123456"},
            {"15295838932","123456"},
            {"18217409755","123456"},
            {"18355332997","123456"},
            {"13694898037","123456"},
            {"15708311758","123456"},
            {"18736997638","123456"},
            {"13773882537","123456"},
            {"15343876011","123456"},
            {"13671749035","123456"},
            {"15656577310","123456"},
            {"13195600317","123456"},
            {"15100489320","123456"},
            {"17682822210","123456"},
            {"18915122970","123456"},
            {"18752370627","123456"},
            {"18792094828","123456"},
            {"15055517321","123456"},
            {"15317993559","123456"},
            {"16621008626","123456"},
            {"13120925358","123456"},
            {"18325366093","123456"},
            {"17555082025","123456"},
            {"15737821672","123456"},
            {"13393004998","123456"},
            {"15240354498","123456"},
            {"13815511835","123456"},
            {"17625515420","123456"},
            {"17627015323","123456"},
            {"13986629817","123456"},
            {"18655724119","123456"},
            {"18063076951","123456"},
            {"13890885445","123456"},
            {"15862248468","123456"},
            {"18502138223","123456"},
            {"18655724118","123456"},
            {"13440101353","123456"},
            {"17633909335","123456"},
            {"18721717927","123456"}

        }.ToList();

        public class Org
        {
            public string OrgOID { get; set; }
            public string OrgName { get; set; }
        }
        protected override void DoAsync()
        {
            string token = null;
            Call("Authority/v1/refresh/did", "{\"appId\":\"10V6WMADM\"}", token, p =>
            {
                var apiResult = (JObject)JsonConvert.DeserializeObject(p);
                if (!apiResult.Value<bool>("success"))
                {
                    Interlocked.Increment(ref BlError);
                    Console.WriteLine(p);
                    return;
                }
                var result = JsonConvert.DeserializeObject<ApiValueResult>(p);
                token = result.ResultData;
            });
            var random = new Random();
            var idx = random.Next(0, users.Count);

            Call("Authority/v1/login/hpc", $"{{\"MobilePhone\":\"{users[idx].Key}\",\"UserPassword\":\"{users[idx].Value}\"}}", token, p =>
            {
                var result = (JObject)JsonConvert.DeserializeObject(p);
                if (result.Value<string>("State") != "success")
                {
                    Interlocked.Increment(ref BlError);
                    Console.WriteLine(p);
                    return;
                }
                var r2 = (JObject)JsonConvert.DeserializeObject(result.Value <string >("Message"));
                token = r2.Value<string>("token");
            });
            Call("Authority/v1/scene/org/site", "{\"Value\":6402331437441220608}", token, p =>
            {
                var result = (JObject)JsonConvert.DeserializeObject(p);
                if (result.Value<string>("State") != "success")
                {
                    Interlocked.Increment(ref BlError);
                    Console.WriteLine(p);
                }
            });
            string orgId=null;
            Call("Authority/v2/org/list", null, token, p =>
            {
                var apiResult = JsonConvert.DeserializeObject<ApiArrayResult< Org>>(p);
                if (!apiResult.Success)
                {
                    Interlocked.Increment(ref BlError);
                    Console.WriteLine(p);
                }
                else orgId = apiResult.ResultData?.FirstOrDefault()?.OrgOID;
            });
            if (orgId != null)
            {
                Call("Authority/v1/scene/org/org", $"{{\"Value\":{orgId}}}", token, p =>
                {
                    var result = (JObject)JsonConvert.DeserializeObject(p);
                    if (result.Value<string>("State") != "success")
                    {
                        Interlocked.Increment(ref BlError);
                        Console.WriteLine(p);
                    }
                });
            }
            Call("Authority/v2/token/info", null, token, p =>
            {
                var result = (JObject)JsonConvert.DeserializeObject(p);
                if (result.Value<string>("State") != "success")
                {
                    Interlocked.Increment(ref BlError);
                    Console.WriteLine(p);
                }
            });
        }

        bool Call(string api, string arg, string token = null, Action<string> action = null)
        {
            HttpApiCaller caller = new HttpApiCaller(Host)
            {
                Bearer = token == null ? null : $"Bearer {token}"
            };
            caller.CreateRequest(api, "POST", arg ?? "{}");
            var result = caller.GetResult().Result;
            if (caller.Status != WebExceptionStatus.Success)
            {
                Interlocked.Increment(ref NetError);
                return false;
            }
            action?.Invoke(result);
            return true;
        }
    }
}