using System.Threading;
using System.Threading.Tasks;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace ApiTest
{
    partial class Program
    {
        public class LoginStation : ApiStation
        {
            public LoginStation()
            {
                StationName = "Test";
                var action =  RegistAction("api/login", arg => Login((Arg) arg),ApiAccessOption.Anymouse);
                action.ArgumenType = typeof(Arg);
            }

            [Route("api/login")]
            public ApiResult Login(Arg user)
            {
                return new ApiResult
                {
                    Success = true,
                    Status = new ApiStatsResult
                    {
                        ClientMessage = $"{Thread.CurrentThread.ManagedThreadId}:{Task.CurrentId}"
                    }
                };
            }
        }
    }
}
