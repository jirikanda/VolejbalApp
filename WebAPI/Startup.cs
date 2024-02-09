using System.Threading.RateLimiting;
using Havit.AspNetCore.ExceptionMonitoring.Services;
using Havit.AspNetCore.Mvc.ExceptionMonitoring.Filters;
using KandaEu.Volejbal.DependencyInjection;
using KandaEu.Volejbal.Services.DeaktivaceOsob;
using KandaEu.Volejbal.Services.Infrastructure;
using KandaEu.Volejbal.Services.Terminy.EnsureTerminy;
using KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions;
using KandaEu.Volejbal.WebAPI.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

[assembly: ApiControllerAttribute]

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

		services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
		services.AddTransient<ErrorMonitoringFilter>();


		services.ConfigureForWebAPI(configuration);
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

			app.UseCustomizedCors(corsOptions);
			app.UseStaticFiles();
			app.UseAuthentication();

			app.UseRequestLocalization();

			app.UseExceptionMonitoring();
			app.UseErrorToJson();
			app.UseRouting();
			app.UseRateLimiter();

			app.UseEndpoints(endpoints => endpoints.MapControllers().RequireRateLimiting("DefaultAPI"));

			app.UseCustomizedOpenApiSwaggerUI();
		}
		catch (Exception exception)
		{
			exceptionMonitoringService.HandleException(exception);
			throw;
		}
	}

}
