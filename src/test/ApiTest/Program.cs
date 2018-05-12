using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace ApiTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //Thread.Sleep(9000);
            StationProgram.WriteLine("Hello ZeroNet");
            StationProgram.Initialize();
            StationProgram.Run();
            StationProgram.WriteLine("Run ZeroNet");
            Console.TreatControlCAsInput = true;
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey();
            } while (key.Modifiers != ConsoleModifiers.Control || key.Key != ConsoleKey.C);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            StationProgram.Exit();
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
                    Result=true,
                    Status = new ApiStatsResult
                    {
                        Message= $"{Task.CurrentId}"
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
