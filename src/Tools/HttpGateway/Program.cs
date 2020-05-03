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
using System.Security.AccessControl;
using System;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Diagnostics;

namespace MicroZero.Http.Gateway
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            LogRecorder.LogPath = Path.Combine(Environment.CurrentDirectory, "logs", ConfigurationManager.Root["AppName"]);

            CreateHostBuilder(args).Build().Run();
            await ZeroApplication.Shutdown();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseConfiguration(ConfigurationManager.Root)
                    .ConfigureLogging((hostingContext, builder) =>
                    {
                        builder.AddConfiguration(ConfigurationManager.Root.GetSection("Logging"));
                        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TextLoggerProvider>());
                        LoggerProviderOptions.RegisterProviderOptions<TextLoggerOption, TextLoggerProvider>(builder.Services);
                    })
                    .UseUrls(ConfigurationManager.Root.GetSection("Kestrel.Endpoints.Http.Url").Value)
                    .ConfigureKestrel(opt =>
                    {
                        try
                        {
                            File.Delete("/tmp/kestrel.sock");
                            opt.ListenUnixSocket("/tmp/kestrel.sock");
                            Task.Factory.StartNew(CheckFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    })
                    .UseKestrel((ctx, opt) =>
                    {
                        opt.Configure(ctx.Configuration.GetSection("Kestrel"));
                    })
                    .UseStartup<GatewayStartup>();
                });

        static void CheckFile()
        {
            try
            {
                do
                {
                    Thread.Sleep(100);
                }
                while (!File.Exists("/tmp/kestrel.sock"));
                Process.Start("chmod","go+w /tmp/kestrel.sock").WaitForExit();
                //var file = new FileInfo("/tmp/kestrel.sock");
                //var fs = file.GetAccessControl();
                //fs.AddAccessRule(new FileSystemAccessRule("nginx", FileSystemRights.FullControl, AccessControlType.Allow));
                //file.SetAccessControl(fs);
                //file.Refresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}