using System.Runtime.InteropServices;
using KandaEu.Volejbal.DependencyInjection.Configuration;

namespace KandaEu.Volejbal.WebAPI;

public static class Program
{
	public static void Main(string[] args)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

		ConfigureConfigurationAndLogging(builder);
		ConfigureServices(builder);

		var app = builder.Build();

		ConfigureMiddleware(app);
		ConfigureEndpoints(app);

		app.Run();
	}

	private static void ConfigureConfigurationAndLogging(WebApplicationBuilder builder)
	{
		builder.Configuration.AddJsonFile("appsettings.WebAPI.json", optional: false);
		builder.Configuration.AddJsonFile($"appsettings.WebAPI.{builder.Environment.EnvironmentName}.json", optional: true);
#if DEBUG
		builder.Configuration.AddJsonFile($"appsettings.WebAPI.{builder.Environment.EnvironmentName}.local.json", optional: true); // .gitignored
#endif
		builder.Configuration.AddEnvironmentVariables();
		builder.Configuration.AddCustomizedAzureKeyVault();

		builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
		builder.Logging.AddConsole();
		builder.Logging.AddDebug();

		if (!builder.Environment.IsDevelopment() && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			builder.Logging.AddEventLog();
		}
	}

	private static void ConfigureServices(WebApplicationBuilder builder)
	{
		Startup startup = new Startup(builder.Configuration);
		startup.ConfigureServices(builder.Services, builder.Environment);
	}

	private static void ConfigureMiddleware(WebApplication app)
	{
		Startup startup = new Startup(app.Configuration);
		startup.ConfigureMiddleware(app);
	}

	private static void ConfigureEndpoints(WebApplication app)
	{
		Startup startup = new Startup(app.Configuration);
		startup.ConfigureEndpoints(app);
	}
}
