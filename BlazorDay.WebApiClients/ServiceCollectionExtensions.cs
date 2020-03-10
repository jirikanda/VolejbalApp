using BlazorDay.WebApiClients;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static void AddWebApiClients(this IServiceCollection services, string baseUrl = "https://blazorday-api.azurewebsites.net")
		{
			services.AddHttpClient();
			services.AddHttpClient<ISystemWebApiClient, SystemWebApiClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));
			services.AddHttpClient<ITerminWebApiClient, TerminWebApiClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));
			services.AddHttpClient<IOsobaWebApiClient, OsobaWebApiClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));
			services.AddHttpClient<INastenkaWebApiClient, NastenkaWebApiClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));
			services.AddHttpClient<IReportWebApiClient, ReportWebApiClient>().ConfigureHttpClient(c => c.BaseAddress = new Uri(baseUrl));
		}
	}
}
