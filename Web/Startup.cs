using System.Threading.RateLimiting;
using Havit.ApplicationInsights.DependencyCollector;
using Havit.AspNetCore.Mvc.ExceptionMonitoring.Filters;
using KandaEu.Volejbal.DependencyInjection;
using KandaEu.Volejbal.Web.Components;
using KandaEu.Volejbal.Web.Infrastructure.ConfigurationExtensions;
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

		services.AddCustomizedMailing(_configuration);

		services.AddExceptionMonitoring(_configuration);
		services.AddCustomizedErrorToJson();

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

		services.AddTransient<ErrorMonitoringFilter>();

		services.ConfigureForWebAPI(_configuration);
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

		app.UseHttpsRedirection();
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

		app.MapRazorComponents<App>()
			.AddInteractiveWebAssemblyRenderMode()
			.AddAdditionalAssemblies(typeof(Client.Components.Routes).Assembly);

		if (app.Environment.IsDevelopment())
		{
			app.UseCustomizedOpenApiScalarUI();
		}
	}

}
