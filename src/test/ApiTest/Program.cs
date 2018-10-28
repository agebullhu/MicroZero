using Agebull.ZeroNet.Core;

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
