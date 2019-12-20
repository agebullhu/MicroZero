using System;
using System.Threading.Tasks;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApis;


namespace ApiTest
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            ZeroApplication.TestFunc = Test;
            ZeroApplication.CheckOption();
            ZeroApplication.Discove(typeof(TestController).Assembly);
            ZeroApplication.Initialize();
            await ZeroApplication.RunAwaiteAsync();
            Console.WriteLine("The end");
        }

        static string Test()
        {
            var result = ApiResult.Succees("IsOk");

            return JsonHelper.SerializeObject(result);
        }
    }
}
