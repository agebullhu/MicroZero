using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Yizuan.Service.Api;

namespace ConsoleTest
{
    class Program
    {
        public static void Main(string[] args)
        {
            //var thread = new Thread(Test)
            //{
            //    IsBackground = true,
            //    Priority = ThreadPriority.Lowest
            //};
            //thread.Start();
            while (Console.ReadKey().KeyChar != 'q')
            {
                Unit();
            }
        }

        static void Test()
        {
            End = 0;
            times = 0.0F;
            avg = 0.0F;
            int start = 0;
            DateTime s = DateTime.Now;
            Console.WriteLine($"Start:{s}");
            do
            {
                if (All < 1)
                    Test(1);
                Thread.Sleep(10000);
                int count_10 = End - start;
                int end = End;
                var real = (DateTime.Now - s).TotalMilliseconds;
                Console.WriteLine($"Now({All}):{times:F}/{End} = {times / End} =>{1000.0 / (real / End)}");
                Console.WriteLine($"Rel({All}):{real}/{End} = {real / End} => {1000.0 / (real / End)}");
                Console.WriteLine($@"Win({count_10}):{count_10 / 10.0}");

                start = end;
            } while (true);
        }

        private volatile static int End, All;
        private volatile static float times = 0.0F;
        private volatile static float avg = 0.0F;
        static void Test(int cnt)
        {
            All += cnt;
            for (int i = 0; i < cnt; i++)
            {
                var thread = new Thread(Unit)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                thread.Start();
            }
        }
        private static void Unit()
        {
            var host = ConfigurationManager.AppSettings["Host"].Trim('/') + "/";
            var hosts = new string[]
            {
                @"GoodLin-OAuth-Api/v1/oauth/getdid?Browser=APP&Os=Android&DeviceId=&v=1513782311187",
                @"GoodLin-Goods-External-Api/v2/dashboard/hotcake?ClientKey=6FA70DFE-D8C5-47AE-BC06-AA3FE2D2BA63",
                @"GoodLin-Goods-External-Api/v1/dashboard/advertandchannel?ClientKey=6FA70DFE-D8C5-47AE-BC06-AA3FE2D2BA63",
                @"GoodLin-Goods-External-Api/v1/discover/info?ClientKey=6FA70DFE-D8C5-47AE-BC06-AA3FE2D2BA63&limit=5&pageIndex=3",
                @"Yizuan-ForcedUpdating-External-Api/v2/activty/getconfig",
                @"GoodLin-DiamondMall-External-Api/v1/diamond/config",
                @"GoodLin-DiamondMall-External-Api/v1/user/diamond"
            };
            foreach (var api in hosts)
                Unit(host + api);
            first = false;
        }
        static bool first = true;
        private static void Unit(string url, string bear = "Bear *0A3B4F50A52A4B0686CBBD09D20BE6D7_IOS_APP")
        {
            do
            {
                try
                {
                    var req = WebRequest.CreateHttp(url);
                    req.Headers["Authorization"] = bear;
                    
                    DateTime s = DateTime.Now;
                    using (var res = req.GetResponse())
                    {
                        using (var stream = res.GetResponseStream())
                        {
                            if (first)
                            {
                                List<byte> buffers = new List<byte>();
                                int len = 0;
                                do
                                {
                                    var bytes = new byte[256];
                                    len = stream.Read(bytes, 0, 256);
                                    for (int i = 0; i < len; i++)
                                    {
                                        buffers.Add(bytes[i]);
                                    }
                                } while (len == 256);
                                stream.Close();
                                var buffer = buffers.ToArray();
                                var str = Encoding.UTF8.GetString(buffer);
                                Console.WriteLine(url);
                                Console.WriteLine(str.Trim());
                                //var result = JsonConvert.DeserializeObject<ApiResult>(str);
                                //if (!result.Result)
                                //    Console.WriteLine("error");
                            }
                            else
                            {
                                var bytes = new byte[256];
                                stream?.Read(bytes, 0, 256);
                                stream?.Close();
                            }
                        }
                        res.Close();
                    }
                    lock (Console.Out)
                    {
                        times += (float)(DateTime.Now - s).TotalMilliseconds;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + url);
                }
                lock (Console.Out)
                {
                    End++;
                }
            } while (false);
        }
        //static void Main(string[] args)
        //{
        //    int cnt = 0;
        //    var pKey = RedisKeyBuilder.ToSystemKey("api", "ctx","*");
        //    using (var proxy = new RedisProxy())
        //    {
        //        //long cursor = 0;
        //        //do
        //        //{
        //        //    var keys = proxy.Client.ScanKeys(ref cursor, pKey, 10);
        //        //    foreach (var key in keys)
        //        //    {
        //        //        proxy.Client.Remove(key);
        //        //    }
        //        //    cnt += keys.Length;
        //        //    Console.WriteLine(cursor);
        //        //} while (cursor > 0);
        //        Console.WriteLine($"一共清理了{cnt}个Key");
        //    }
        //    Console.ReadLine();
        //}

        ///// <summary>
        ///// 得到缓存的键
        ///// </summary>
        //public static string GetCacheKey(string requestId)
        //{
        //    var key = RedisKeyBuilder.ToSystemKey("api", "ctx", requestId.Trim('$').ToUpper());
        //    Debug.WriteLine(key);
        //    return key;
        //}
        //private static ApiValueResult<DeviceArgument> test = new ApiValueResult<DeviceArgument>
        //{
        //    Result = true,
        //    ResultData = new DeviceArgument
        //    {
        //        DeviceId = "abc",
        //        DeviceId2 = new[] { "a", "b" },
        //        DeviceId3 = new byte[] { (byte)1, 2, 3 },

        //    }
        //};
        /*static void Main(string[] args)
        {
            string result = "{}";//JsonConvert.SerializeObject(test);
            StringBuilder code = new StringBuilder();
            using (var textReader = new StringReader(result))
            {
                var reader = new JsonTextReader(textReader);
                bool isResultData = false;
                int levle = 0;
                while (reader.Read())
                {
                    if (!isResultData && reader.TokenType == JsonToken.PropertyName)
                    {
                        var name = reader.Value.ToString();
                        if (name == "ResultData")
                        {
                            isResultData = true;
                        }
                        continue;
                    }
                    if (!isResultData)
                        continue;
                    switch (reader.TokenType)
                    {
                        case JsonToken.StartArray:
                            code.Append('[');
                            continue;
                        case JsonToken.StartObject:
                            code.Append('{');
                            levle++;
                            continue;
                        case JsonToken.PropertyName:
                            code.Append($"\"{reader.Value}\"=");
                            continue;
                        case JsonToken.Bytes:
                            code.Append($"\"{reader.Value}\"");
                            break;
                        case JsonToken.Date:
                        case JsonToken.String:
                            code.Append($"\"{reader.Value}\"");
                            break;
                        case JsonToken.Integer:
                        case JsonToken.Float:
                        case JsonToken.Boolean:
                            code.Append("null");
                            break;
                        case JsonToken.Null:
                            code.Append("null");
                            break;
                        case JsonToken.EndObject:
                            if (code.Length > 0 && code[code.Length - 1] == ',')
                                code[code.Length - 1] = '}';
                            else
                                code.Append('}');
                            levle--;
                            break;
                        case JsonToken.EndArray:
                            if (code.Length > 0 && code[code.Length - 1] == ',')
                                code[code.Length - 1] = ']';
                            else
                                code.Append(']');
                            break;
                        case JsonToken.Raw:
                            code.Append(reader.Value);
                            break;
                        case JsonToken.Undefined:
                            break;
                        case JsonToken.StartConstructor:
                            break;
                        case JsonToken.None:
                            break;
                        case JsonToken.EndConstructor:
                            break;
                        case JsonToken.Comment:
                            break;
                    }
                    if (levle == 0)
                        break;
                    code.Append(',');
                }

            }
            if (code.Length > 0 && code[code.Length - 1] == ',')
                code[code.Length - 1] = ' ';
            Console.WriteLine(code.ToString());
            Console.ReadKey();
        }*/
    }
}
