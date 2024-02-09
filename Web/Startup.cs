using System.Globalization;
using KandaEu.Volejbal.Web.WebApiClients;
using KandaEu.Volejbal.Web.App_Start;
using Blazored.LocalStorage;
using Sotsera.Blazor.Toaster.Core.Models;

namespace KandaEu.Volejbal.Web;

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
		services.AddRazorPages();
		services.AddServerSideBlazor();
		services.AddHttpClient();

		services.AddCustomizedHttpClient<ISystemWebApiClient, SystemWebApiClient>(Configuration);
		services.AddCustomizedHttpClient<ITerminWebApiClient, TerminWebApiClient>(Configuration);
		services.AddCustomizedHttpClient<IOsobaWebApiClient, OsobaWebApiClient>(Configuration);
		services.AddCustomizedHttpClient<INastenkaWebApiClient, NastenkaWebApiClient>(Configuration);
		services.AddCustomizedHttpClient<IReportWebApiClient, ReportWebApiClient>(Configuration);

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
