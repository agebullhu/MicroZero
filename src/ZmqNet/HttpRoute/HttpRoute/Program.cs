using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
namespace ZeroNet.Http.Route
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
            RouteCounter.Save();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
