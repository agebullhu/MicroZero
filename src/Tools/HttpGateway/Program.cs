using Agebull.Common.Configuration;
using Agebull.MicroZero;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MicroZero.Http.Gateway
{
    public class Program
    {

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
            ZeroApplication.Shutdown();
        }


        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(ConfigurationManager.Root)
                .UseKestrel(RouteApp.Options)
                .UseStartup<Startup>()
                .Build();
        }
    }
}