using System.IO;
using System.Threading;
using Agebull.ZeroNet.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ZeroNet.Http.Route
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Thread.Sleep(11000);
            BuildWebHost(args).Run();
            PerformanceCounter.Save();
            StationProgram.Exit();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .Build();
        }
    }
}
