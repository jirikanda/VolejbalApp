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
using Microsoft.AspNetCore.Http;
using Havit.VolejbalApp.WebAPI.Infrastructure;
using Havit.VolejbalApp.WebAPI.Infrastructure.ConfigurationExtensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Havit.VolejbalApp.WebAPI.Infrastructure.Tools;
using Microsoft.AspNetCore.Mvc;

[assembly: ApiControllerAttribute]

namespace Havit.VolejbalApp.WebAPI
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

	        services.AddCustomizedRequestLocalization();
			services.AddCustomizedMvc(configuration);
            services.AddAuthorization();
            services.AddCustomizedAuthentication(configuration); // musí být voláno až po AddMvc, jinak nejsou volány IClaimsTransformation.
	        services.AddCustomizedMailing(configuration);
	        
			services.AddExceptionMonitoring(configuration);
			services.AddCustomizedErrorToJson();

            services.AddCustomizedCors(configuration);
            services.AddCustomizedSwagger();			

	        services.AddApplicationInsightsTelemetry(configuration);

			services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

			IWindsorContainer windsorContainer = WindsorCastleConfiguration.CreateWindsorContainer(configuration);
            return services.AddCustomizedServiceProvider(windsorContainer);
        }

        /// <summary>
        /// Configure middleware.
        /// </summary>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<Havit.VolejbalApp.WebAPI.Infrastructure.Cors.CorsOptions> corsOptions)
        {
			if (env.IsDevelopment())
	        {
		        app.UseDeveloperExceptionPage();
	        }

			app.UseCustomizedCors(corsOptions);
            app.UseStaticFiles();
            app.UseAuthentication();

	        app.UseRequestLocalization();

			app.UseExceptionMonitoring();
			app.UseErrorToJson();
            app.UseMvc();

            app.UseCustomizedSwaggerAndUI();

	        app.UpgradeDatabaseSchemaAndData();
        }

    }
}
