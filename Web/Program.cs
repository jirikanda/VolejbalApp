using Blazored.LocalStorage;
using KandaEu.Volejbal.Web.App_Start;
using KandaEu.Volejbal.Web.Components;

namespace KandaEu.Volejbal.Web;
public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Configuration.AddJsonFile("appsettings.Web.json", optional: false, reloadOnChange: false);
		builder.Configuration.AddJsonFile($"appsettings.Web.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
		builder.Configuration.AddEnvironmentVariables();

		// Add services to the container.
		builder.Services.AddRazorComponents()
			.AddInteractiveServerComponents();

		builder.Services.AddLocalization();

		builder.Services.AddCustomizedHttpClient<ISystemWebApiClient, SystemWebApiClient>(builder.Configuration);
		builder.Services.AddCustomizedHttpClient<ITerminWebApiClient, TerminWebApiClient>(builder.Configuration);
		builder.Services.AddCustomizedHttpClient<IOsobaWebApiClient, OsobaWebApiClient>(builder.Configuration);
		builder.Services.AddCustomizedHttpClient<INastenkaWebApiClient, NastenkaWebApiClient>(builder.Configuration);
		builder.Services.AddCustomizedHttpClient<IReportWebApiClient, ReportWebApiClient>(builder.Configuration);

		builder.Services.AddBlazoredLocalStorage();

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		app.UseHttpsRedirection();

		app.UseStaticFiles();
		app.UseAntiforgery();

		app.UseRequestLocalization("cs-CZ"); // https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization?view=aspnetcore-8.0#statically-set-the-server-side-culture
		app.MapRazorComponents<App>()
			.AddInteractiveServerRenderMode();

		app.Run();
	}
}
