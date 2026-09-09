using System.Text.Json;
using KandaEu.Volejbal.Contracts.Nastenka;
using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Prihlasky;
using KandaEu.Volejbal.Contracts.Reporty;
using KandaEu.Volejbal.Contracts.System;
using KandaEu.Volejbal.Contracts.Terminy;
using Refit;

namespace KandaEu.Volejbal.Web.Client.App_Start;

/// <summary>
/// Registrace typových klientů API.
/// </summary>
/// <remarks>
/// Klienty generuje Refit z rozhraní I*Api v projektu Contracts. Tatáž rozhraní na serveru implementují
/// fasády, takže rozejití serveru s klientem odhalí kompilátor. Nahrazuje dřívější generování NSwagem
/// z OpenAPI dokumentu (Azure Functions build-time export OpenAPI neumí).
/// </remarks>
public static class ApiClientConfig
{
	public static IServiceCollection AddApiClients(this IServiceCollection services, string apiBaseUrl)
	{
		// Server serializuje camelCase (výchozí nastavení ASP.NET Core); bez sladění by deserializace
		// tiše vracela prázdné objekty.
		RefitSettings refitSettings = new RefitSettings
		{
			ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web))
		};

		services.AddApiClient<INastenkaApi>(refitSettings, apiBaseUrl);
		services.AddApiClient<IOsobaApi>(refitSettings, apiBaseUrl);
		services.AddApiClient<IPrihlaskaApi>(refitSettings, apiBaseUrl);
		services.AddApiClient<ITerminApi>(refitSettings, apiBaseUrl);
		services.AddApiClient<IReportOsobApi>(refitSettings, apiBaseUrl);
		services.AddApiClient<IReportTerminuApi>(refitSettings, apiBaseUrl);
		services.AddApiClient<IDataSeedApi>(refitSettings, apiBaseUrl);

		return services;
	}

	/// <remarks>
	/// AddRefitGeneratedClient, ne AddRefitClient: od Refitu 14 je reflexní request builder v samostatném
	/// balíčku Refit.Reflection a AddRefitClient si ho vyžádá - bez něj skončí registrace za běhu na
	/// NotSupportedException ("This interface needs the reflection request builder"). Generovaná varianta
	/// navíc nesahá na reflexi vůbec, což je pro trimovaný WebAssembly build to, co chceme.
	/// </remarks>
	private static void AddApiClient<TApi>(this IServiceCollection services, RefitSettings refitSettings, string apiBaseUrl)
		where TApi : class
	{
		services
			.AddRefitGeneratedClient<TApi>(refitSettings)
			.ConfigureHttpClient(httpClient => httpClient.BaseAddress = new Uri(apiBaseUrl));
	}
}
