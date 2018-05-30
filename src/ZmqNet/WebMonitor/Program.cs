using System;
using System.IO;
using Agebull.ZeroNet.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace WebMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
            WebSocketPooler.Instance.Dispose();
            ZeroApplication.Shutdown();
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
