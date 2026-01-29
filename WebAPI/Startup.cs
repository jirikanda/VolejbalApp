using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.Dashboard;
using Havit.AspNetCore.ExceptionMonitoring.Services;
using Havit.AspNetCore.Mvc.ExceptionMonitoring.Filters;
using KandaEu.Volejbal.DependencyInjection;
using KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions;
using KandaEu.Volejbal.WebAPI.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;

[assembly: ApiController]

namespace KandaEu.Volejbal.WebAPI;

public class Startup
{
	private readonly IConfiguration configuration;

	public Startup(IConfiguration configuration)
	{
		this.configuration = configuration;
	}

	/// <summary>
	/// Configure services.
	/// </summary>
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

		services.AddOptions(); // Adds services required for using options.
		services.AddMemoryCache(); // ie. IClaimsCacheStorage

		services.AddCustomizedRequestLocalization();
		services.AddCustomizedMvc(configuration);
		services.AddAuthorization();
		services.AddRateLimiter(c => c.AddFixedWindowLimiter("DefaultAPI", options =>
		{
			options.Window = TimeSpan.FromSeconds(5); // v pětisekundovém okně
			options.PermitLimit = 10; // umožníme zpracovat 10 requestů
			options.QueueLimit = 10; // a dalších 10 umožníme nechat ve frontě ke zpracování
			options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
		}));

		services.AddCustomizedMailing(configuration);

		services.AddExceptionMonitoring(configuration);
		services.AddCustomizedErrorToJson();

		services.AddCustomizedCors(configuration);
		services.AddCustomizedOpenApi();

		services.AddApplicationInsightsTelemetry(configuration);

		services.AddTransient<ErrorMonitoringFilter>();

		services.ConfigureForWebAPI(configuration);

		services.AddCustomizedHangfireServer();
	}

	/// <summary>
	/// Configure middleware.
	/// </summary>
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<KandaEu.Volejbal.WebAPI.Infrastructure.Cors.CorsOptions> corsOptions, IExceptionMonitoringService exceptionMonitoringService)
	{
		try
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseMiddleware<DelayRequestMiddleware>();
			}
			app.UseExceptionHandler(_ => { /* NOOP */ });

			app.UseCustomizedCors(corsOptions);
			app.UseStaticFiles();
			app.UseAuthentication();

			app.UseRequestLocalization();

			app.UseErrorToJson();
			app.UseRouting();
			app.UseRateLimiter();

			// výchozí stránku hangfire změníme na recurring jobs (na výchozí stránku se nyní není jak dostat, což nám nevadí)
			app.UseRewriter(new Microsoft.AspNetCore.Rewrite.RewriteOptions().AddRedirect("^hangfire(/)?$", "hangfire/recurring"));

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers().RequireRateLimiting("DefaultAPI");

				endpoints.MapHangfireDashboard("/hangfire", new DashboardOptions
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
			});

			app.UseCustomizedOpenApiSwaggerUI();
		}
		catch (Exception exception)
		{
			exceptionMonitoringService.HandleException(exception);
			throw;
		}
	}

}
