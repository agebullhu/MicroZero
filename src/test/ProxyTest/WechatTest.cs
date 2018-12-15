using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using ZeroMQ;

namespace ProxyTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.Write("access token:");
            //string access_token = Console.ReadLine();
            string noncestr = CreatenNonce_str();
            Console.WriteLine($"noncestr:{noncestr}");
            var timestamp = CreatenTimestamp();
            Console.WriteLine($"timestamp:{timestamp}");
            string url = "http://agebull.yizuanbao.cn/jssdk.html";
            Console.WriteLine($"url:{url}");
            var sign =GetSignature("", "XXebQXkdgFgVjWM", 1542803041, url,out _);
            Console.WriteLine($"sign:{sign}");
            Console.ReadKey();
        }


        private static string[] strs = new string[]
        {
            "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z",
            "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"
        };
        /// <summary>
        /// 创建随机字符串
        ///本代码来自开源微信SDK项目：https://github.com/night-king/weixinSDK
        /// </summary>
        /// <returns></returns>
        public static string CreatenNonce_str()
        {
            Random r = new Random();
            var sb = new StringBuilder();
            var length = strs.Length;
            for (int i = 0; i < 15; i++)
            {
                sb.Append(strs[r.Next(length - 1)]);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 创建时间戳
        ///本代码来自开源微信SDK项目：https://github.com/night-king/weixinSDK
        /// </summary>
        /// <returns></returns>
        public static long CreatenTimestamp()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }
        /// <summary>
        /// 获取jsapi_ticket
        /// jsapi_ticket是公众号用于调用微信JS接口的临时票据。
        /// 正常情况下，jsapi_ticket的有效期为7200秒，通过access_token来获取。
        /// 由于获取jsapi_ticket的api调用次数非常有限，频繁刷新jsapi_ticket会导致api调用受限，影响自身业务，开发者必须在自己的服务全局缓存jsapi_ticket 。
        /// </summary>
        /// <param name="access_token">BasicAPI获取的access_token,也可以通过TokenHelper获取</param>
        /// <returns></returns>
        public static string GetTickect(string access_token)
        {
            return "HoagFKDcsGMVCIY2vOjf9mYScbUKVhftyfO0TRZhG5YC8ktxr3Z9MjV-lKrk-5uG9xUjFLDuVU8GnQL3jk6Ozg";
            var url = string.Format("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi", access_token);
            var client = new HttpClient();
            var result = client.GetAsync(url).Result;
            if (!result.IsSuccessStatusCode)
                return string.Empty;
            var jsTicket = JsonConvert.DeserializeObject<Dictionary<string,string>>(result.Content.ReadAsStringAsync().Result);
            Console.WriteLine(jsTicket["ticket"]);
            return jsTicket["ticket"];
        }


        /// <summary>
        /// 签名算法
        /// </summary>
        /// <param name="at">jsapi_ticket</param>
        /// <param name="noncestr">随机字符串(必须与wx.config中的nonceStr相同)</param>
        /// <param name="timestamp">时间戳(必须与wx.config中的timestamp相同)</param>
        /// <param name="url">当前网页的URL，不包含#及其后面部分(必须是调用JS接口页面的完整URL)</param>
        /// <returns></returns>
        public static string GetSignature(string at, string noncestr, long timestamp, string url, out string string1)
        {
            var jsapi_ticket = GetTickect(at);
            var string1Builder = new StringBuilder();
            string1Builder.Append("jsapi_ticket=").Append(jsapi_ticket).Append("&")
                          .Append("noncestr=").Append(noncestr).Append("&")
                          .Append("timestamp=").Append(timestamp).Append("&")
                          .Append("url=").Append(url.IndexOf("#") >= 0 ? url.Substring(0, url.IndexOf("#")) : url);
            string1 = string1Builder.ToString();
            Console.WriteLine(string1);
            return Sha1(string1);
        }

        /// <summary>
        /// Sha1
        /// </summary>
        /// <param name="orgStr"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string Sha1(string orgStr, string encode = "UTF-8")
        {
            var sha1 = new SHA1Managed();
            var sha1bytes = System.Text.Encoding.GetEncoding(encode).GetBytes(orgStr);
            byte[] resultHash = sha1.ComputeHash(sha1bytes);
            string sha1String = BitConverter.ToString(resultHash).ToLower();
            sha1String = sha1String.Replace("-", "");
            return sha1String;
        }
    }
}