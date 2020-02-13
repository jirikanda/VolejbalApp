using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions;
using KandaEu.Volejbal.WebAPI.Infrastructure;
using KandaEu.Volejbal.WebAPI.Infrastructure.Tools;
using KandaEu.Volejbal.WebAPI.Infrastructure.Middlewares;
using Microsoft.Extensions.Hosting;
using Havit.AspNetCore.Mvc.ExceptionMonitoring.Filters;
using KandaEu.Volejbal.DependencyInjection;
using KandaEu.Volejbal.Services.DeaktivaceOsob;
using KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

[assembly: ApiControllerAttribute]

namespace KandaEu.Volejbal.WebAPI
{
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
	        services.AddCustomizedMailing(configuration);
	        
			services.AddExceptionMonitoring(configuration);
			services.AddCustomizedErrorToJson();

            services.AddCustomizedCors(configuration);
            services.AddCustomizedOpenApi();			

	        services.AddApplicationInsightsTelemetry(configuration);

			services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
			services.AddTransient<ErrorMonitoringFilter>();

            // background jobs
            services.AddHostedService<DeaktivaceOsobBackgroundService>();
            services.AddHostedService<EnsureTerminyBackgroundService>();

            services.ConfigureForWebAPI(configuration);
		}

        /// <summary>
        /// Configure middleware.
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<KandaEu.Volejbal.WebAPI.Infrastructure.Cors.CorsOptions> corsOptions)
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
			app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseCustomizedOpenApiSwaggerUI();

	        app.UpgradeDatabaseSchemaAndData();
        }

    }
}
