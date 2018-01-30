using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Havit.NewProjectTemplate.Web.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Havit.NewProjectTemplate.Web
{
    public class Startup
    {
	    private readonly IConfiguration configuration;

	    public Startup(IConfiguration configuration)
	    {
		    this.configuration = configuration;
	    }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddMemoryCache();
            services.AddMvc();

            services.Configure<WebApiSettings>(configuration.GetSection("AppSettings:WebApi"));

			services.AddExceptionMonitoring(configuration);
	        services.AddApplicationInsightsTelemetry(configuration);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
				{
					HotModuleReplacement = true,
					ReactHotModuleReplacement = true
				});
			}
			else
			{
                app.UseStatusCodePages();
			}

			app.UseStaticFiles();
       
			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Default}/{action=Index}/{id?}");

				routes.MapSpaFallbackRoute(
					name: "spa-fallback",
					defaults: new { controller = "Default", action = "Index" });
			});
		}
    }
}
