using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public static async Task RunCheck()
        {
            await Task.Yield();
            while (ZeroApplication.IsAlive)
            {
                await Task.Delay(1000);
                foreach (var api in ZeroApplication.ActiveObjects.OfType<ApiStationBase>().ToArray())
                {
                    await api.CheckTask();
                }
                if (!ZeroApplication.IsAlive)
                    return;
                await Task.Delay(1000);
            }
        }
    }
}