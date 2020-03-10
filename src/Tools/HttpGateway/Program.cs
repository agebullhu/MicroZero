using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Agebull.MicroZero;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IO;
using System;

namespace MicroZero.Http.Gateway
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            LogRecorder.LogPath = Path.Combine(Environment.CurrentDirectory, "logs", ConfigurationManager.Root["AppName"]);
            BuildWebHost(args).Run();

            await ZeroApplication.Shutdown();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(ConfigurationManager.Root)
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddConfiguration(ConfigurationManager.Root.GetSection("Logging"));
                    //builder.AddConsole();
                    builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TextLoggerProvider>());
                    LoggerProviderOptions.RegisterProviderOptions<TextLoggerOption, TextLoggerProvider>(builder.Services);
                })
                .UseKestrel(RouteApp.Options)//HTTPS∂Àø⁄≈‰÷√
                .UseStartup<GatewayStartup>()
                .Build();
        }
    }
}