using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public class ApiStationItem
    {
        #region 变量

        /// <summary>
        ///     站点
        /// </summary>
        internal ApiStation Station { private get; set; }

        /// <summary>
        ///     在心跳
        /// </summary>
        internal bool InHeart;

        /// <summary>
        ///     在侦听
        /// </summary>
        internal bool InPoll;

        /// <summary>
        ///     在侦听
        /// </summary>
        internal int Index;

        /// <summary>
        ///     实名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        ///     运行状态
        /// </summary>
        public StationState RunState { get; set; }


        #endregion

        #region 网络与执行


        /// <summary>
        ///     命令轮询
        /// </summary>
        /// <returns></returns>
        public bool Run()
        {
            StationProgram.WriteLine($"【{RealName}】start...");
            InPoll = false;
            InHeart = false;

            

            try
            {
                //var word = _socket.Request(ZeroByteCommand.WorkerJoin);
                //if (word == null || word[0] != ZeroNetStatus.ZeroStatusSuccess)
                //{
                //    StationProgram.WriteError($"【{RealName}】 proto@ error");
                //    return TryRun();
                //}
                var word = Station.Config.HeartAddress.RequestNet(ZeroByteCommand.HeartJoin, RealName);
                if (word == null)
                {
                    StationProgram.WriteError($"【{RealName}】 *heart error:timeout");
                    return TryRun();
                }
                if (word[0] != ZeroNetStatus.ZeroStatusSuccess)
                {
                    StationProgram.WriteError($"【{RealName}】 *heart error:{word}");
                    return TryRun();
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationProgram.WriteError($"【{RealName}】request error =>{e.Message}");
                return TryRun();
            }
            
            RunState = StationState.Run;
            _tryCount = 0;

            Task.Factory.StartNew(PollTask).ContinueWith(OnTaskStop);
            Task.Factory.StartNew(HeartbeatTask).ContinueWith(OnTaskStop);

            while (!InHeart || !InPoll)
                Thread.Sleep(50);
            StationProgram.WriteLine($"【{RealName}】runing...");
            return true;
        }

        /// <summary>
        ///     命令轮询
        /// </summary>
        /// <returns></returns>
        private void OnTaskStop(Task task)
        {
            if (InPoll || InHeart)
                return;
            if (StationProgram.State == StationState.Run && RunState == StationState.Failed)
            {
                StationProgram.WriteInfo($"【{RealName}】restart...");
                TryRun();
                return;
            }
            RunState = StationState.Closed;
            Station.OnItemStop();
        }

        #endregion


    }
}