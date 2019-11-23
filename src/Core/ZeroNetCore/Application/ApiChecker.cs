using System.Linq;
using System.Threading;
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
            while (ZeroApplication.IsAlive)
            {
                Thread.Sleep(1500);
                foreach (var api in ZeroApplication.ActiveObjects.OfType<ApiStationBase>().ToArray())
                {
                    if (api.CanLoop)
                        api.CheckTask();
                }
                if (!ZeroApplication.IsAlive)
                    return;
                Thread.Sleep(1500);
            }
        }
    }
}