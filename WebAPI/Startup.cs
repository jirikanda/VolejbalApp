using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using Castle.Windsor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Havit.NewProjectTemplate.WebAPI.Infrastructure;
using Havit.NewProjectTemplate.WebAPI.Infrastructure.ConfigurationExtensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

namespace Havit.NewProjectTemplate.WebAPI
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
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddOptions(); // Adds services required for using options.
            services.AddMemoryCache(); // ie. IClaimsCacheStorage
			services.AddDataProtection();

	        services.AddCustomizedRequestLocalization();
			services.AddCustomizedMvc(configuration);
            services.AddAuthorization();
            services.AddCustomizedAuthentication(configuration); // musí být voláno až po AddMvc, jinak nejsou volány IClaimsTransformation.
	        services.AddCustomizedMailing(configuration);
	        
			services.AddExceptionMonitoring(configuration);
            services.AddCustomizedCors(configuration);
            services.AddCustomizedSwagger();			

	        services.AddApplicationInsightsTelemetry(configuration);

			services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

			IWindsorContainer windsorContainer = WindsorCastleConfiguration.CreateWindsorContainer(configuration);
            Tools.DataSeed.Run(windsorContainer);
            return services.AddCustomizedServiceProvider(windsorContainer);
        }

        /// <summary>
        /// Configure middleware.
        /// </summary>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<Havit.NewProjectTemplate.WebAPI.Infrastructure.Cors.CorsOptions> corsOptions)
        {
            loggerFactory.AddConsole(configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddEventLog(LogLevel.Error);

			if (env.IsDevelopment())
	        {
		        app.UseDeveloperExceptionPage();
	        }

			app.UseCustomizedCors(corsOptions);
            app.UseStaticFiles();
            app.UseAuthentication();

	        app.UseRequestLocalization();
            app.UseMvc();
            app.UseCustomizedSwaggerAndUI();
        }

    }
}
