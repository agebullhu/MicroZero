using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.MicroZero;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace MicroZero.Http.Gateway
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            BuildWebHost(args).Run();
            await ZeroApplication.Shutdown();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(ConfigurationManager.Root)
                .UseKestrel(RouteApp.Options)//HTTPS∂Àø⁄≈‰÷√
                .UseStartup<GatewayStartup>()
                .Build();
        }
    }
}