using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Base;
using Agebull.Common.Logging;
using Gboxt.Common.DataModel;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Agebull.ZeroNet.Core;

namespace ZeroApi
{
    /// <summary>
    /// 数据事件广播代理
    /// </summary>
    public class ZeroEntityEventProxy : ScopeBase, IEntityEventProxy
    {
        private readonly RequestSocket socket;
        private readonly StationConfig config;

        /// <summary>
        /// 构造
        /// </summary>
        public ZeroEntityEventProxy()
        {
            config = StationProgram.GetConfig("EntityEvent");
            if (config == null)
                throw new Exception("无法拉取配置");
            socket = new RequestSocket();
            CreateSocket();
        }

        void CreateSocket()
        {
            try
            {
                socket.Options.Identity = StationProgram.Config.ServiceName.ToAsciiBytes();
                socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                socket.Connect(config.OutAddress);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationProgram.WriteLine($"【EntityEvent】connect error =>{e.Message}");
                throw;
            }

        }

        bool Request(string msg, ref int retry)
        {
            if (++retry > 5)
            {
                LogRecorder.Error($"数据事件服务(EntityEvent)无法连接!\r\n{msg}");
                return false;
            }
            try
            {
                lock (this)
                {
                    socket.Request(msg);
                }
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationProgram.WriteLine($"【EntityEvent】publish error =>{e.Message}");
                retry++;
                Thread.Sleep(10);
                CreateSocket();
                return Request(msg, ref retry);
            }
        }
        void Request(object arg)
        {
            var retry = 0;
            Request(arg.ToString(), ref retry);
        }
        void IEntityEventProxy.OnStatusChanged(int entityType, EntitySubsist type, NotificationObject data)
        {
            var msg = $@"state:{entityType}:{type}
{StationProgram.Config.StationName}
{(data == null ? "{}" : JsonConvert.SerializeObject(data))}";
            Task.Factory.StartNew(Request, msg);
        }

        /// <summary>清理资源</summary>
        protected override void OnDispose()
        {
            socket.Dispose();
        }
    }
}