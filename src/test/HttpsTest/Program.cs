using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HttpsTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var certificate = new X509Certificate2(Path.Combine(Environment.CurrentDirectory, "cert.pfx"),"123456");
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(
                    options =>
                    {
                        options.AddServerHeader = false;
                        options.Listen(IPAddress.Loopback, 4431, listenOptions =>
                        {
                            listenOptions.UseHttps(certificate);
                        });
                    }
                )
                .UseStartup<Startup>()
                .UseUrls("https://*:4431")
                .Build()
                .Run();
        }

    }
}
