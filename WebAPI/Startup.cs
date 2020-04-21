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
using Havit.AspNetCore.Mvc.ExceptionMonitoring.Services;
using ProtoBuf.Grpc.Server;
using KandaEu.Volejbal.Facades.Terminy;
using KandaEu.Volejbal.Facades.Osoby;
using KandaEu.Volejbal.Facades.Prihlasky;
using KandaEu.Volejbal.Facades.Nastenka;
using KandaEu.Volejbal.Facades.System;
using KandaEu.Volejbal.Contracts.Terminy;
using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Prihlasky;
using KandaEu.Volejbal.Contracts.Nastenka;
using KandaEu.Volejbal.Contracts.System;

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
            services.AddAuthorization();
            services.AddCustomizedMailing(configuration);

            services.AddExceptionMonitoring(configuration);

            services.AddCustomizedCors(configuration);

            services.AddApplicationInsightsTelemetry(configuration);

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient<ErrorMonitoringFilter>();

            // background jobs
            services.AddHostedService<DeaktivaceOsobBackgroundService>();
            services.AddHostedService<EnsureTerminyBackgroundService>();

            services.ConfigureForWebAPI(configuration);

            services.AddCodeFirstGrpc();
            services.AddGrpcWeb(options => options.GrpcWebEnabled = true);
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
                app.UseRouting();
                app.UseGrpcWeb();
                app.UseEndpoints(endpoints =>
                {                    
                    endpoints.MapGrpcService<ITerminFacade>();
                    endpoints.MapGrpcService<IOsobaFacade>();
                    endpoints.MapGrpcService<IPrihlaskaFacade>();
                    endpoints.MapGrpcService<INastenkaFacade>();
                    endpoints.MapGrpcService<IDataSeedFacade>();
                });               

                app.UpgradeDatabaseSchemaAndData();
            }
            catch (Exception exception)
            {
                exceptionMonitoringService.HandleException(exception);
                throw;
            }
        }

    }
}
