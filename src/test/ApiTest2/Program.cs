using System.Threading.Tasks;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace ApiTest
{
    class Program
    {
        static void Main(string[] args)
        {
            StationConsole.WriteLine("Hello ZeroNet");
            ZeroApplication.Initialize();
            ZeroApplication.RunAwaite();
        }

        public class LoginControler : ZeroApiController
        {
            public LoginControler()
            {

            }
            [Route("api/login")]
            public ApiResult Login(Arg user)
            {
                return new ApiResult
                {
                    Success = true,
                    Status = new ApiStatsResult
                    {
                        ClientMessage = $"Api2 --- {Task.CurrentId}"
                    }
                };
            }
        }

        public class Arg : IApiArgument
        {
            string IApiArgument.ToFormString()
            {
                return "";
            }

            bool IApiArgument.Validate(out string message)
            {
                message = null;
                return true;
            }
        }
    }
}
