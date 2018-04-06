using Agebull.ZeroNet.Core;
using System;

namespace Agebull.ZeroNet.LogService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello ZeroNet");
            
            StationProgram.RegisteApiStation(new SubStation
            {
                StationName = "RemoteLog",
                ExecFunc = LogService.RecordLog
            });

            StationProgram.RunConsole();
        }
    }
}
