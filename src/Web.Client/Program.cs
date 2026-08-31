using System.Globalization;
using Blazored.LocalStorage;
using Havit.Blazor.Components.Web;
using KandaEu.Volejbal.Web.Client.App_Start;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace KandaEu.Volejbal.Web.Client;

public class Program
{
	public static async Task Main(string[] args)
	{
		WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

		// Root komponenty (Routes, HeadOutlet) neregistrujeme - aktivuje je blazor.web.js podle markerů vyrenderovaných hostem.

		builder.Services.AddCustomizedHttpClient<ISystemWebApiClient, SystemWebApiClient>(builder.HostEnvironment.BaseAddress);
		builder.Services.AddCustomizedHttpClient<ITerminWebApiClient, TerminWebApiClient>(builder.HostEnvironment.BaseAddress);
		builder.Services.AddCustomizedHttpClient<IOsobaWebApiClient, OsobaWebApiClient>(builder.HostEnvironment.BaseAddress);
		builder.Services.AddCustomizedHttpClient<INastenkaWebApiClient, NastenkaWebApiClient>(builder.HostEnvironment.BaseAddress);
		builder.Services.AddCustomizedHttpClient<IReportWebApiClient, ReportWebApiClient>(builder.HostEnvironment.BaseAddress);

		builder.Services.AddBlazoredLocalStorage();
		builder.Services.AddHxServices();

		// WASM nemá request localization, kulturu nastavujeme napevno.
		CultureInfo culture = new CultureInfo("cs-CZ");
		CultureInfo.DefaultThreadCurrentCulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;

		await builder.Build().RunAsync();
	}
}
