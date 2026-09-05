using System.Globalization;
using Blazored.LocalStorage;
using Havit.Blazor.Components.Web;
using KandaEu.Volejbal.Web.Client.App_Start;
using KandaEu.Volejbal.Web.Client.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace KandaEu.Volejbal.Web.Client;

public class Program
{
	public static async Task Main(string[] args)
	{
		WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

		builder.RootComponents.Add<HeadOutlet>("head::after");
		builder.RootComponents.Add<Routes>("#app");

		string apiBaseUrl = builder.Configuration["ApiBaseUrl"]
			?? throw new InvalidOperationException("Configuration value 'ApiBaseUrl' is not set.");

		builder.Services.AddCustomizedHttpClient<ISystemWebApiClient, SystemWebApiClient>(apiBaseUrl);
		builder.Services.AddCustomizedHttpClient<ITerminWebApiClient, TerminWebApiClient>(apiBaseUrl);
		builder.Services.AddCustomizedHttpClient<IOsobaWebApiClient, OsobaWebApiClient>(apiBaseUrl);
		builder.Services.AddCustomizedHttpClient<INastenkaWebApiClient, NastenkaWebApiClient>(apiBaseUrl);
		builder.Services.AddCustomizedHttpClient<IReportWebApiClient, ReportWebApiClient>(apiBaseUrl);

		builder.Services.AddBlazoredLocalStorage();
		builder.Services.AddHxServices();

		// WASM nemá request localization, kulturu nastavujeme napevno.
		CultureInfo culture = new CultureInfo("cs-CZ");
		CultureInfo.DefaultThreadCurrentCulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;

		await builder.Build().RunAsync();
	}
}
