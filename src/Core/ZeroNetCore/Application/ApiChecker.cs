using System;
using System.Linq;
using System.Threading;
using Agebull.Common.Logging;
using Agebull.MicroZero.ZeroApis;

namespace Agebull.MicroZero.ZeroManagemant
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public class ApiChecker
    {
        /// <summary>
        /// 执行检查
        /// </summary>
        public static void RunCheck()
        {
            LogRecorder.SystemLog("ApiChecker runing");
            while (ZeroApplication.IsAlive)
            {
                Thread.Sleep(1000);
                if (!ZeroApplication.IsAlive)
                    return;
                foreach (var api in ZeroApplication.ActiveObjects.OfType<ApiStationBase>().ToArray())
                {
                    api.CheckTask();
                }
                if (!ZeroApplication.IsAlive)
                    return;
                foreach (var task in ApiProxy.Instance.Tasks.ToArray())
                {
                    if ((DateTime.Now - task.Value.Start).TotalMinutes > ZeroApplication.Config.ApiTimeout)
                    {
                        ApiProxy.Instance.Tasks.TryRemove(task.Key,out _);
                        LogRecorder.Error("task:({0})=>Time Out", task.Value);
                        task.Value.TaskSource.SetException(new Exception("Time Out"));
                    }
                }
                if (!ZeroApplication.IsAlive)
                    return;
                Thread.Sleep(1000);
            }
        }
    }
}