using Agebull.Common.Ioc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace ApiTest
{
    partial class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.Initialize();
            ZeroApplication.RunAwaite();
        }
    }
}
