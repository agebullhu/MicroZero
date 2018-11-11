using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.Log;

namespace ApiTest
{
    partial class Program
    {static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.Discove(typeof(AutoRegister).Assembly);
            ZeroApplication.Initialize();
            ZeroApplication.RunAwaite();
        }
    }
}
