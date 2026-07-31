using Microsoft.Extensions.DependencyInjection;

namespace KandaEu.Volejbal.Web.Client.App_Start;

public static class WebApiClientConfig
{
	public static void AddCustomizedHttpClient<TClient, TImplementation>(this IServiceCollection services, string baseAddress)
		where TClient : class
		where TImplementation : class, TClient
	{
		services.AddHttpClient<TClient, TImplementation>().ConfigureHttpClient(c => c.BaseAddress = new Uri(baseAddress));
	}
}
