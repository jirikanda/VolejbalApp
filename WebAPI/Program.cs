using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Castle.Windsor.MsDependencyInjection;
using KandaEu.Volejbal.WindsorInstallers;

namespace KandaEu.Volejbal.WebAPI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
			CreateHostBuilder(args).Build().Run();
        }

	    public static IHostBuilder CreateHostBuilder(string[] args)
	    {
			return Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder
						.UseStartup<Startup>()
#if DEBUG
						.UseEnvironment("Development") // pro Red-Gate ANTS Performance Profiler
						.UseUrls("http://localhost:9901"); // pro Red-Gate ANTS Performance Profiler
#endif
				})
				.UseServiceProviderFactory(ctx => new WebApiServiceProviderFactory(ctx.Configuration))
				.ConfigureAppConfiguration((hostContext, config) =>
				{
					// delete all default configuration providers
					config.Sources.Clear();
					config
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
						.AddJsonFile(@"Config\appsettings.json", optional: true, reloadOnChange: true)
						.AddEnvironmentVariables();
				})
				.ConfigureLogging((hostingContext, logging) =>
				{
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole();
					logging.AddDebug();
#if !DEBUG
					logging.AddEventLog();
#endif
				});
	    }
    }
}
