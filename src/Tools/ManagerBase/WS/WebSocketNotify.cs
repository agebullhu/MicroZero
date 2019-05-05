using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Microsoft.Extensions.Configuration;

namespace WebMonitor
{

    public class WebSocketNotify
    {
        #region 系统操作


        /// <summary>
        /// 所有客户端连接实例
        /// </summary>
        public static Dictionary<string, List<WebSocketClient>> Handlers = new Dictionary<string, List<WebSocketClient>>();


        private static WebSocketConfig _config;
        public static WebSocketConfig Config
        {
            get
            {
                if (_config != null)
                    return _config;
                try
                {
                    var sec = ConfigurationManager.Root.GetSection("WebSocket");
                    return _config = sec.Get<WebSocketConfig>() ?? new WebSocketConfig();
                }
                catch (Exception e)
                {
                    LogRecorderX.Exception(e);
                    return _config = new WebSocketConfig();
                }
            }
        }
        /// <summary>  
        /// 路由绑定处理  
        /// </summary>  
        /// <param name="app"></param>  
        public static void Binding(IApplicationBuilder app)
        {
            if (Config.Folders == null) return;
            foreach (var folder in Config.Folders)
            {
                Handlers.Add(folder, new List<WebSocketClient>());
                app.Map($"/{folder}", Map);
            }
        }

        public static void Close()
        {
            foreach (var handler in Handlers.Values)
            {
                foreach (var client in handler)
                    client.Dispose();
            }
        }

        /// <summary>  
        /// 路由绑定处理  
        /// </summary>  
        /// <param name="app"></param>  
        private static void Map(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Use(Acceptor);
        }

        /// <summary>
        /// 接收连接时被引用
        /// </summary>
        /// <param name="hc"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static async Task Acceptor(HttpContext hc, Func<Task> n)
        {
            if (!hc.WebSockets.IsWebSocketRequest || !hc.Request.PathBase.HasValue)
                return;

            var classify = hc.Request.PathBase.Value.Trim('\\', '/', ' ');
            if (!Handlers.TryGetValue(classify, out var list))
            {
                return;
            }

            var socket = await hc.WebSockets.AcceptWebSocketAsync();
            var notify = new WebSocketClient(socket, classify, list);
            list.Add(notify);
            await notify.EchoLoop();
        }

        #endregion

        #region 广播管理


        public static void Publish(string classify, string title, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            if (!Handlers.TryGetValue(classify, out var list))
                return;
            var array = list.ToArray();
            Task.Factory.StartNew(() => PublishAsync(array, title, value));
        }

        private static void PublishAsync(WebSocketClient[] list, string title, string value)
        {
            var empty = string.IsNullOrWhiteSpace(title);
            var tbuffer = title.ToUtf8Bytes();
            var title_a = new ArraySegment<byte>(tbuffer, 0, tbuffer.Length);
            var buffer = value.ToUtf8Bytes();
            var value_a = new ArraySegment<byte>(buffer, 0, buffer.Length);
            foreach (var handler in list)
            {
                if (empty || handler.Subscriber.Count == 0)
                {
                    handler.Send(title_a, value_a);
                    continue;
                }

                foreach (var sub in handler.Subscriber)
                {
                    if (title.IndexOf(sub, StringComparison.Ordinal) != 0)
                        continue;
                    handler.Send(title_a, value_a);
                    break;
                }
            }
        }

        #endregion

    }
}