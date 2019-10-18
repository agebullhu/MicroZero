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




        string _token/* = "#CSLQXNM8WUYA"*/;
        protected override void DoTest()
        {
            new LoginTester().LoginTest();
            //LoadTest();
            //SettTest();
        }


        #region EShopTest


        void GetToken()
        {
            Call("Authority/v1/login/hpc", $"{{\"MobilePhone\":\"{LoginTester.Users[1].Key}\",\"UserPassword\":\"{LoginTester.Users[1].Value}\"}}", null, p =>
            {
                var result = (JObject)JsonConvert.DeserializeObject(p);
                if (!result.TryGetValue("State", out var val) || val.Value<string>() != "success")
                {
                    //users.RemoveAt(idx);
                    Interlocked.Increment(ref BugError);
                    //Console.WriteLine($"¡¾login/hpc¡¿ {p}");
                    return;
                }
                var r2 = (JObject)JsonConvert.DeserializeObject(result.Value<string>("Message"));
                _token = r2.Value<string>("token");
            });
        }

        private const string arg =
            "{\"CurrentItem\":-1,\"OrderCode\":\"\",\"OrderType\":2,\"LogistcicsType\":1,\"Items\":[{\"ItemType\":1,\"PlatformId\":\"898663154082637\",\"BaseSkuSid\":\"898662818538286\",\"BarCode\":\"3760283409019\",\"SkuName\":\"Ä¢¹½Æøµæ·ÛÄýËªZD9001\",\"Number\":1,\"CostPrice\":38.9900,\"SalePrice\":69.0,\"ConfigJson\":\"\",\"Amount\":69.0,\"RealState\":1,\"Pay\":69.0,\"Extend\":\"\"},{\"ItemType\":1,\"PlatformId\":\"898663154082623\",\"BaseSkuSid\":\"898662818538272\",\"BarCode\":\"3760283407046\",\"SkuName\":\"³ÖÈó²»Õ´±­¿ÚºìZD7004\",\"Number\":1,\"CostPrice\":19.2100,\"SalePrice\":34.0,\"ConfigJson\":\"\",\"Amount\":34.0,\"RealState\":1,\"Pay\":34.0,\"Extend\":\"\"},{\"ItemType\":1,\"PlatformId\":\"898663154082628\",\"BaseSkuSid\":\"898662818538277\",\"BarCode\":\"3760283408036\",\"SkuName\":\"¾ªÕÀ´óÑÛË«Í·½ÞÃ«¸àZD8003\",\"Number\":1,\"CostPrice\":21.4700,\"SalePrice\":38.0,\"ConfigJson\":\"\",\"Amount\":38.0,\"RealState\":1,\"Pay\":38.0,\"Extend\":\"\"}],\"UserId\":\"6577516094666313728\",\"OrderDate\":\"2019-09-11T20:05:36.6340837+08:00\",\"Coupons\":[],\"OrgOID\":\"6402336636121645056\",\"Remark\":\"\",\"RealState\":1}";
        void SettTest()
        {
            if (string.IsNullOrEmpty(_token))
                GetToken();

            Call("SettlementCenter/v1/order/check", arg, _token, p =>
            {
                var result = (JObject)JsonConvert.DeserializeObject(p);
                if (!result.TryGetValue("success", out var val) || !val.Value<bool>())
                {
                    Interlocked.Increment(ref BugError);
                }
            });
        }
        void LoadTest()
        {
            if (string.IsNullOrEmpty(_token))
                GetToken();
            Call("eShop/v1/active/all", null, _token, p =>
             {
                 var result = (JObject)JsonConvert.DeserializeObject(p);
                 if (!result.TryGetValue("success", out var val) || !val.Value<bool>())
                 {
                     Interlocked.Increment(ref BugError);
                 }
             });
        }
        #endregion
        #region Function

        internal static bool Call(string api, string arg, string token = null, Action<string> action = null)
        {
            HttpApiCaller caller = new HttpApiCaller(Host)
            {
                Bearer = token == null ? null : $"Bearer {token}"
            };
            caller.CreateRequest(api, "POST", arg ?? "{}");
            var result = caller.GetResult().Result;
            if (caller.Status != WebExceptionStatus.Success)
            {
                Interlocked.Increment(ref Instance.NetError);
                return false;
            }
            action?.Invoke(result);
            return true;
        }

        #endregion
    }

    #region LoginTest
    class LoginTester
    {
        public void LoginTest()
        {
            //Call("Authority/v1/refresh/did", "{\"appId\":\"10V6WMADM\"}", token, p =>
            //{
            //    var apiResult = (JObject)JsonConvert.DeserializeObject(p);
            //    if (!apiResult.Value<bool>("success"))
            //    {
            //        Interlocked.Increment(ref BlError);
            //        Console.WriteLine(p);
            //        return;
            //    }
            //    var result = JsonConvert.DeserializeObject<ApiValueResult>(p);
            //    token = result.ResultData;
            //});
            var random = new Random();
            var idx = random.Next(0, Users.Count);
            string token=null;
            HttpTester.Call("Authority/v1/login/hpc", $"{{\"MobilePhone\":\"{Users[idx].Key}\",\"UserPassword\":\"{Users[idx].Value}\"}}", null, p =>
            {
                var result = (JObject)JsonConvert.DeserializeObject(p);
                if (!result.TryGetValue("State", out var val) || val.Value<string>() != "success")
                {
                    //users.RemoveAt(idx);
                    Interlocked.Increment(ref Tester.Instance.BugError);
                    Console.WriteLine($"¡¾login/hpc¡¿{p}");
                    return;
                }
                var r2 = (JObject)JsonConvert.DeserializeObject(result.Value<string>("Message"));
                token = r2.Value<string>("token");
            });

            if (string.IsNullOrEmpty(token))
                return;
            HttpTester.Call("Authority/v1/scene/org/site", "{\"Value\":6402331437441220608}", token, p =>
            {
                var result = (JObject)JsonConvert.DeserializeObject(p);
                if (!result.TryGetValue("State", out var val) || val.Value<string>() != "success")
                {
                    Interlocked.Increment(ref Tester.Instance.BugError);
                    Console.WriteLine($"¡¾org/site¡¿{token} {p}");
                }
            });
            string orgId = null;
            HttpTester.Call("Authority/v2/org/list", null, token, p =>
            {
                var apiResult = JsonConvert.DeserializeObject<ApiArrayResult<Org>>(p);
                if (!apiResult.Success)
                {
                    Interlocked.Increment(ref Tester.Instance.BugError);
                    Console.WriteLine($"¡¾org/list¡¿{token} {p}");
                }
                else orgId = apiResult.ResultData?.FirstOrDefault()?.OrgOID;
            });
            if (orgId != null)
            {
                HttpTester.Call("Authority/v1/scene/org/org", $"{{\"Value\":{orgId}}}", token, p =>
                {
                    var result = (JObject)JsonConvert.DeserializeObject(p);
                    if (!result.TryGetValue("State", out var val) || val.Value<string>() != "success")
                    {
                        Interlocked.Increment(ref Tester.Instance.BugError);
                        Console.WriteLine($"¡¾org/org¡¿{token} {p}");
                    }
                });
            }
            HttpTester.Call("Authority/v2/token/info", null, token, p =>
            {
                var result = (JObject)JsonConvert.DeserializeObject(p);
                if (!result.TryGetValue("State", out var val) || val.Value<string>() != "success")
                {
                    Interlocked.Increment(ref Tester.Instance.BugError);
                    Console.WriteLine($"¡¾token/info¡¿{token} {p}");
                }
            });
        }


        #region Data

        public class Org
        {
            public string OrgOID { get; set; }
        }

        public static readonly List<KeyValuePair<string, string>> Users = new Dictionary<string, string>
        {
            {"18175466613","123456"},
            {"18321968817","123456"},
            {"18294884427","123456"},
            {"18208245547","123456"},
            {"13636606174","123456"},
            {"16602107168","123456"},
            {"18721306175","123456"},
            {"15070116118","123456"},
            {"18049939527","123456"},
            {"18296858121","123456"},
            {"17301894889","123456"},
            {"17502173667","123456"},
            {"15235916819","123456"},
            {"18759978195","123456"},
            {"17630303122","123456"},
            {"19938444305","123456"},
            //{"19938444305","123456"},
            //{"17630303122","123456"},
            //{"18759978195","123456"},
            //{"15235916819","123456"},
            //{"17502173667","123456"},
            //{"17301894889","123456"},
            //{"18296858121","123456"},
            //{"18049939527","123456"},
            //{"15070116118","123456"},
            //{"18721306175","123456"},
            //{"16602107168","123456"},
            //{"13636606174","123456"},
            //{"18208245547","123456"},
            //{"18294884427","123456"},
            //{"18321968817","123456"},
            //{"18175466613","123456"},
            //{"15295838932","123456"},
            //{"18217409755","123456"},
            //{"18355332997","123456"},
            //{"13694898037","123456"},
            //{"15708311758","123456"},
            //{"18736997638","123456"},
            //{"13773882537","123456"},
            //{"15343876011","123456"},
            //{"13671749035","123456"},
            //{"15656577310","123456"},
            //{"13195600317","123456"},
            //{"15100489320","123456"},
            //{"17682822210","123456"},
            //{"18915122970","123456"},
            //{"18752370627","123456"},
            //{"18792094828","123456"},
            //{"15055517321","123456"},
            //{"15317993559","123456"},
            //{"16621008626","123456"},
            //{"13120925358","123456"},
            //{"18325366093","123456"},
            //{"17555082025","123456"},
            //{"15737821672","123456"},
            //{"13393004998","123456"},
            //{"15240354498","123456"},
            //{"13815511835","123456"},
            //{"17625515420","123456"},
            //{"17627015323","123456"},
            //{"13986629817","123456"},
            //{"18655724119","123456"},
            //{"18063076951","123456"},
            //{"13890885445","123456"},
            //{"15862248468","123456"},
            //{"18502138223","123456"},
            //{"18655724118","123456"},
            //{"13440101353","123456"},
            //{"17633909335","123456"},
            //{"18721717927","123456"}
        }.ToList();

        #endregion
    }
    #endregion
}

/*

        protected override void DoAsync()
        {
            string token = null;
            //Call("Authority/v1/refresh/did", "{\"appId\":\"10V6WMADM\"}", token, p =>
            //{
            //    var apiResult = (JObject)JsonConvert.DeserializeObject(p);
            //    if (!apiResult.Value<bool>("success"))
            //    {
            //        Interlocked.Increment(ref BlError);
            //        Console.WriteLine(p);
            //        return;
            //    }
            //    var result = JsonConvert.DeserializeObject<ApiValueResult>(p);
            //    token = result.ResultData;
            //});
            var random = new Random();
            var idx = random.Next(0, users.Count);

            Call("Authority/v1/login/hpc", $"{{\"MobilePhone\":\"{users[idx].Key}\",\"UserPassword\":\"{users[idx].Value}\"}}", null, p =>
            {
                var result = (JObject)JsonConvert.DeserializeObject(p);
                if (!result.TryGetValue("State",out var val) || val.Value<string>() != "success")
                {
                    //users.RemoveAt(idx);
                    Interlocked.Increment(ref BlError);
                    //Console.WriteLine($"¡¾login/hpc¡¿ {p}");
                    return;
                }
                var r2 = (JObject)JsonConvert.DeserializeObject(result.Value <string >("Message"));
                token = r2.Value<string>("token");
            });
            if (string.IsNullOrEmpty(token))
                return;
            //Call("Authority/v1/scene/org/site", "{\"Value\":6402331437441220608}", token, p =>
            //{
            //    var result = (JObject)JsonConvert.DeserializeObject(p);
            //    if (!result.TryGetValue("State", out var val) || val.Value<string>() != "success")
            //    {
            //        Interlocked.Increment(ref BlError);
            //        // Console.WriteLine($"¡¾org/site¡¿ {p}");
            //    }
            //});
            //string orgId = null;
            //Call("Authority/v2/org/list", null, token, p =>
            //{
            //    var apiResult = JsonConvert.DeserializeObject<ApiArrayResult<Org>>(p);
            //    if (!apiResult.Success)
            //    {
            //        Interlocked.Increment(ref BlError);
            //        // Console.WriteLine($"¡¾org/list¡¿ {p}");
            //    }
            //    else orgId = apiResult.ResultData?.FirstOrDefault()?.OrgOID;
            //});
            //if (orgId != null)
            //{
            //    Call("Authority/v1/scene/org/org", $"{{\"Value\":{orgId}}}", token, p =>
            //    {
            //        var result = (JObject)JsonConvert.DeserializeObject(p);
            //        if (!result.TryGetValue("State", out var val) || val.Value<string>() != "success")
            //        {
            //            Interlocked.Increment(ref BlError);
            //            // Console.WriteLine($"¡¾org/org¡¿ {p}");
            //        }
            //    });
            //}
            Call("Authority/v2/token/info", null, token, p =>
            {
                var result = (JObject)JsonConvert.DeserializeObject(p);
                if (!result.TryGetValue("State", out var val) || val.Value<string>() != "success")
                {
                    Interlocked.Increment(ref BlError);
                    // Console.WriteLine($"¡¾token/info¡¿ {p}");
                }
            });
        }
 */
