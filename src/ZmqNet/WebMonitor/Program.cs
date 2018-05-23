using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Agebull.ZeroNet.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            int haha = 0;
            int hoho = 0;
            for (int i = 0; i < short.MaxValue; i++)
            {
                if (DateTime.Now.Ticks != DateTime.Now.Ticks)
                    haha++;
                else hoho++;
            }
            Console.WriteLine($"haha:{haha} hoho{hoho}");
            BuildWebHost(args).Run();
            WebSocketPooler.Instance.Dispose();
            ZeroApplication.Destroy();
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
