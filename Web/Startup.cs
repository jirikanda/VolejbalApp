using System.Threading.RateLimiting;
using Havit.ApplicationInsights.DependencyCollector;
using Havit.AspNetCore.Mvc.ExceptionMonitoring.Filters;
using KandaEu.Volejbal.DependencyInjection;
using KandaEu.Volejbal.Web.Components;
using KandaEu.Volejbal.Web.Infrastructure;
using KandaEu.Volejbal.Web.Infrastructure.ApplicationInsights;
using KandaEu.Volejbal.Web.Infrastructure.ConfigurationExtensions;
using KandaEu.Volejbal.Web.Infrastructure.HealthChecks;
using KandaEu.Volejbal.Web.Infrastructure.Middlewares;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Rewrite;

[assembly: ApiController]

namespace KandaEu.Volejbal.Web;

public class Startup
{
	private readonly IConfiguration _configuration;

	public Startup(IConfiguration configuration)
	{
		this._configuration = configuration;
	}

	/// <summary>
	/// Configure services.
	/// </summary>
	public void ConfigureServices(IServiceCollection services, IWebHostEnvironment environment)
	{
		services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

		services.AddOptions(); // Adds services required for using options.
		services.AddMemoryCache(); // ie. IClaimsCacheStorage

		services.AddCustomizedRequestLocalization();
		services.AddCustomizedMvc(_configuration);
		services.AddRazorComponents()
			.AddInteractiveWebAssemblyComponents();
		services.AddAuthorization();
		services.AddRateLimiter(c => c.AddFixedWindowLimiter("DefaultAPI", options =>
		{
			options.Window = TimeSpan.FromSeconds(5); // v pětisekundovém okně
			options.PermitLimit = 10; // umožníme zpracovat 10 requestů
			options.QueueLimit = 10; // a dalších 10 umožníme nechat ve frontě ke zpracování
			options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
		}));

		services.AddExceptionMonitoring(_configuration);
		services.AddCustomizedErrorToJson();

		// Bez registrovaných checků vrací endpoint 200 hned, jakmile stojí pipeline. To je záměr:
		// probe má měřit "je aplikace schopná obsloužit request", ne dostupnost závislostí. Kontrola
		// databáze by prodloužila studený start a při jediné replice (maxReplicas: 1) by z výpadku
		// databáze udělala výpadek celé aplikace.
		services.AddHealthChecks();

		// OpenAPI potřebujeme jen v Development (Scalar) a při build-time exportu dokumentu pro generátor klientů
		// (OpenApiGenerateDocumentsOnBuild) - ten bootuje aplikaci nástrojem GetDocument.Insider mimo Development.
		bool isBuildTimeOpenApiExport = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
		if (environment.IsDevelopment() || isBuildTimeOpenApiExport)
		{
			services.AddCustomizedOpenApi();
		}

		services.AddApplicationInsightsTelemetry(_configuration);
		services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) => { module.EnableSqlCommandTextInstrumentation = true; });
		services.AddApplicationInsightsTelemetryProcessor<IgnoreCancellationExceptionsTelemetryProcessor>();
		services.AddApplicationInsightsTelemetryProcessor<IgnoreHealthChecksTelemetryProcessor>();

		services.AddTransient<ErrorMonitoringFilter>();

		services.ConfigureForWebAPI(_configuration);

		if (!String.IsNullOrEmpty(_configuration.GetConnectionString("Database"))) // při build-time exportu OpenAPI dokumentu (GetDocument.Insider) nemáme connection string
		{
			services.AddHostedService<WarmupBackgroundService>();
		}
	}

	/// <summary>
	/// Configure middleware.
	/// </summary>
	public void ConfigureMiddleware(WebApplication app)
	{
		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
			app.UseWebAssemblyDebugging();
			// jen na API - jinak by 500 ms delay brzdil i download WASM assetů
			app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder => appBuilder.UseMiddleware<DelayRequestMiddleware>());
		}
		else
		{
			app.UseHsts();
		}
		app.UseExceptionHandler(_ => { /* NOOP */ });

		// Health probes chodí z ACA přímo na kontejner po HTTP (mimo ingress), takže nenesou
		// X-Forwarded-Proto a UseHttpsRedirection by je odbavil 307 redirectem. ACA přitom považuje
		// za úspěch cokoli v rozsahu 200-399, takže by probe procházel i u úplně rozbité aplikace.
		// Proto na health cestě redirect přeskakujeme.
		app.UseWhen(
			context => !context.Request.Path.StartsWithSegments(HealthCheckEndpoints.Path),
			appBuilder => appBuilder.UseHttpsRedirection());
		app.UseAuthentication();

		app.UseRequestLocalization();

		app.UseErrorToJson();
		app.UseRouting();
		app.UseAntiforgery();
		app.UseRateLimiter();
	}

	public void ConfigureEndpoints(WebApplication app)
	{
		app.MapStaticAssets();

		app.MapControllers().RequireRateLimiting("DefaultAPI");

		// Mimo MapControllers, takže se na něj nevztahuje rate limit "DefaultAPI" - probe každých
		// pár sekund by jinak ukusoval z limitu určeného pro API.
		app.MapHealthChecks(HealthCheckEndpoints.Path);

		app.MapRazorComponents<App>()
			.AddInteractiveWebAssemblyRenderMode()
			.AddAdditionalAssemblies(typeof(Client.Components.Routes).Assembly);

		if (app.Environment.IsDevelopment())
		{
			app.UseCustomizedOpenApiScalarUI();
		}
	}

}
