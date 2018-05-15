using System.Threading.Tasks;
using Agebull.ZeroNet.Core;

namespace ApiTest
{
    partial class Program
    {
        static void Main(string[] args)
        {
            StationConsole.WriteLine("Hello ZeroNet");
            ZeroApplication.RunAwaite();
        }
    }
}
