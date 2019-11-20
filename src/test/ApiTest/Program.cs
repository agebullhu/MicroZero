using System;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApis;


namespace ApiTest
{
    partial class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.TestFunc = Test;
            ZeroApplication.CheckOption();
            ZeroApplication.Discove(typeof(TestController).Assembly);
            ZeroApplication.Initialize();
            ZeroApplication.RunAwaite();
        }

        static string Test()
        {
            var result = ApiResult.Succees("IsOk");

            return JsonHelper.SerializeObject(result);
        }
    }
}
