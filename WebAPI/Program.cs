using System.Runtime.InteropServices;
using KandaEu.Volejbal.DependencyInjection.Configuration;

namespace KandaEu.Volejbal.WebAPI;

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
				webBuilder.UseStartup<Startup>();
			})
			.ConfigureAppConfiguration((hostContext, config) =>
			{
				// delete all default configuration providers
				config.Sources.Clear();
				config
					.AddJsonFile("appsettings.WebAPI.json", optional: false, reloadOnChange: false)
					.AddJsonFile($"appsettings.WebAPI.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
					.AddEnvironmentVariables()
					.AddCustomizedAzureKeyVault();
			})
			.ConfigureLogging((hostingContext, logging) =>
			{
				logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
				logging.AddConsole();
				logging.AddDebug();
				if (!hostingContext.HostingEnvironment.IsDevelopment() && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					logging.AddEventLog();
				}
			});
	}
}
