using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ServerSDK;

namespace VonageBlazorApp
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
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Listen(IPAddress.Loopback, 5002);
                       serverOptions.Listen(IPAddress.Loopback, 5003, listenOptions =>
                        {
                            listenOptions.UseHttps(@"C:\Certs\upwork.pfx", "CERTPASSWORD");

                        });


                    });
                });
    }
}
