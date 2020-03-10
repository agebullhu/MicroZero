using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.MicroZero.ZeroApis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApplication2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseConfiguration(ConfigurationManager.Root)
                    .ConfigureLogging(HostExtend.LoggerOptions)
                    .UseKestrel(HostExtend.KestrelOptions)//HTTPS∂Àø⁄≈‰÷√
                    .UseStartup<Startup>();
                });
    }
}
