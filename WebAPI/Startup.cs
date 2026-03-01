using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.Dashboard;
using Havit.ApplicationInsights.DependencyCollector;
using Havit.AspNetCore.Mvc.ExceptionMonitoring.Filters;
using KandaEu.Volejbal.DependencyInjection;
using KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions;
using KandaEu.Volejbal.WebAPI.Infrastructure.Middlewares;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;

[assembly: ApiController]

namespace KandaEu.Volejbal.WebAPI;

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
	public void ConfigureServices(IServiceCollection services, IWebHostEnvironment _)
	{
		services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

		services.AddOptions(); // Adds services required for using options.
		services.AddMemoryCache(); // ie. IClaimsCacheStorage

		services.AddCustomizedRequestLocalization();
		services.AddCustomizedMvc(_configuration);
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

		services.AddCustomizedCors(_configuration);
		services.AddCustomizedOpenApi();

		services.AddApplicationInsightsTelemetry(_configuration);
		services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) => { module.EnableSqlCommandTextInstrumentation = true; });
		services.AddApplicationInsightsTelemetryProcessor<IgnoreCancellationExceptionsTelemetryProcessor>();

		services.AddTransient<ErrorMonitoringFilter>();

		services.ConfigureForWebAPI(_configuration);

		services.AddCustomizedHangfireServer();
	}

	/// <summary>
	/// Configure middleware.
	/// </summary>
	public void ConfigureMiddleware(WebApplication app)
	{
		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
			app.UseMiddleware<DelayRequestMiddleware>();
		}
		app.UseExceptionHandler(_ => { /* NOOP */ });

		var corsOptions = app.Services.GetRequiredService<IOptions<KandaEu.Volejbal.WebAPI.Infrastructure.Cors.CorsOptions>>();
		app.UseCustomizedCors(corsOptions);
		app.UseStaticFiles();
		app.UseAuthentication();

		app.UseRequestLocalization();

		app.UseErrorToJson();
		app.UseRouting();
		app.UseRateLimiter();

		// výchozí stránku hangfire změníme na recurring jobs (na výchozí stránku se nyní není jak dostat, což nám nevadí)
		app.UseRewriter(new Microsoft.AspNetCore.Rewrite.RewriteOptions().AddRedirect("^hangfire(/)?$", "hangfire/recurring"));
	}

	public void ConfigureEndpoints(WebApplication app)
	{
		app.MapControllers().RequireRateLimiting("DefaultAPI");

		app.MapHangfireDashboard("/hangfire", new DashboardOptions
		{
			DefaultRecordsPerPage = 50,
#if !DEBUG
			IsReadOnlyFunc = _ => true,
#endif
			Authorization = new List<IDashboardAuthorizationFilter>() { }, // see https://sahansera.dev/securing-hangfire-dashboard-with-endpoint-routing-auth-policy-aspnetcore/
			DisplayStorageConnectionString = false,
			DashboardTitle = $"VolejbalApp",
			StatsPollingInterval = 60_000, // once a minute
			DisplayNameFunc = (_, job) => Havit.Hangfire.Extensions.Helpers.JobNameHelper.TryGetSimpleName(job, out string simpleName)
												? simpleName
												: job.ToString()
		});

		app.UseCustomizedOpenApiSwaggerUI();
	}

}
