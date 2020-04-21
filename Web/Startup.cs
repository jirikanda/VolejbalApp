using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using Blazored.LocalStorage;
using Sotsera.Blazor.Toaster.Core.Models;
using System.Net.Http;
using KandaEu.Volejbal.Contracts.Terminy;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text;
using ProtoBuf.Grpc.Client;
using KandaEu.Volejbal.Contracts.Nastenka;
using KandaEu.Volejbal.Contracts.Prihlasky;
using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Reporty;
using KandaEu.Volejbal.Contracts.System;
using KandaEu.Volejbal.Web.Infrastructure;

namespace KandaEu.Volejbal.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<EventAggregator.Blazor.IEventAggregator, EventAggregator.Blazor.EventAggregator>();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpClient();

            services.AddBlazoredLocalStorage();

            services.AddToaster(config =>
            {
                config.PositionClass = Defaults.Classes.Position.TopRight;
                config.PreventDuplicates = false;
                config.NewestOnTop = true;
                config.ShowProgressBar = true;
                config.ShowCloseIcon = false;
                config.RequireInteraction = false;
                config.ShowTransitionDuration = 0;
                config.HideTransitionDuration = 0;
                config.VisibleStateDuration = 4000;
            });

            services.AddGrpcWebProxy<INastenkaFacade>();
            services.AddGrpcWebProxy<ITerminFacade>();
            services.AddGrpcWebProxy<IPrihlaskaFacade>();
            services.AddGrpcWebProxy<IOsobaFacade>();
            services.AddGrpcWebProxy<IReportOsobFacade>();
            services.AddGrpcWebProxy<IReportTerminuFacade>();
            services.AddGrpcWebProxy<IDataSeedFacade>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

			app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

			var cultureInfo = new CultureInfo("cs-CZ");
			CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
			CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
		}
    }
}
