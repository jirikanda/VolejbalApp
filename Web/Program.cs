using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.Web;

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
                webBuilder.UseStaticWebAssets(); // Sotsera.Blazor.Toaster
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                // delete all default configuration providers
                config.Sources.Clear();
                config
                    .AddJsonFile("appsettings.Web.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.Web.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables();
            });
}
