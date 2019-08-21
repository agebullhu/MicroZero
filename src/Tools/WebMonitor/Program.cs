using Agebull.Common.Configuration;
using Agebull.MicroZero;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace WebMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
            WebSocketNotify.Close();
            ZeroApplication.Shutdown();
        }


        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(ConfigurationManager.Root)
                .UseStartup<Startup>()
                .Build();
        }
    }
}
