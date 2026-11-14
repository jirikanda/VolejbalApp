using System.Globalization;
using Havit.ApplicationInsights.DependencyCollector;
using KandaEu.Volejbal.Api.Infrastructure;
using KandaEu.Volejbal.Api.Mcp;
using KandaEu.Volejbal.DependencyInjection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.Api;

public static class Program
{
	public static void Main(string[] args)
	{
		FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

		builder.ConfigureFunctionsWebApplication();

		ConfigureCulture();
		ConfigureConfigurationAndLogging(builder);
		ConfigureServices(builder);

		builder.UseMiddleware<ExceptionHandlingMiddleware>();

		builder.Build().Run();
	}

	/// <summary>
	/// Nahrazuje AddCustomizedRequestLocalization() z původního ASP.NET Core hostingu - Functions
	/// nemají request localization middleware a kultura se stejně nikdy neodvozovala od klienta.
	/// </summary>
	/// <remarks>
	/// Časová zóna se nastavuje v kódu (viz PragueTimeProvider), ne proměnnou TZ - tu Flex Consumption nepodporuje.
	/// </remarks>
	private static void ConfigureCulture()
	{
		CultureInfo cultureInfo = new CultureInfo("cs-CZ");
		CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
		CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
	}

	private static void ConfigureConfigurationAndLogging(FunctionsApplicationBuilder builder)
	{
		builder.Configuration.AddJsonFile("appsettings.Api.json", optional: false);
		builder.Configuration.AddJsonFile($"appsettings.Api.{builder.Environment.EnvironmentName}.json", optional: true);
#if DEBUG
		builder.Configuration.AddJsonFile($"appsettings.Api.{builder.Environment.EnvironmentName}.local.json", optional: true); // .gitignored
#endif
		builder.Configuration.AddEnvironmentVariables();

		builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
	}

	private static void ConfigureServices(FunctionsApplicationBuilder builder)
	{
		builder.Services.AddOptions();

		builder.Services.AddExceptionMonitoring(builder.Configuration);

		builder.Services.AddApplicationInsightsTelemetryWorkerService(options =>
		{
			// Studený start: worker si nechává jen sběr requestů, závislostí (volání Cosmosu) a výjimek.
			// Ostatní moduly startují vlákna a spojení, která se na krátce žijící instanci Flex Consumption
			// (0,25 jádra) nikdy nevyplatí - Live Metrics otevírá streamovací spojení hned při startu,
			// performance a event countery čtou v intervalu čítače procesu, diagnostický modul posílá
			// heartbeat a dotazuje se na metadata instance (IMDS).
			options.EnableQuickPulseMetricStream = false;
			options.EnablePerformanceCounterCollectionModule = false;
			options.EnableEventCounterCollectionModule = false;
			options.EnableDiagnosticsTelemetryModule = false;
		});
		builder.Services.ConfigureFunctionsApplicationInsights();
		builder.Services.AddApplicationInsightsTelemetryProcessor<IgnoreCancellationExceptionsTelemetryProcessor>();

		builder.Services.ConfigureForWebAPI(builder.Configuration);
		builder.Services.AddVolejbalMcpServer();
	}
}
