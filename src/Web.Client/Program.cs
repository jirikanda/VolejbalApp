using System.Globalization;
using Havit.Blazor.Storage;
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

		builder.Services.AddApiClients(apiBaseUrl);

		builder.Services.AddHavitBlazorStorage();

		// WASM nemá request localization, kulturu nastavujeme napevno.
		CultureInfo culture = new CultureInfo("cs-CZ");
		CultureInfo.DefaultThreadCurrentCulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;

		await builder.Build().RunAsync();
	}
}
