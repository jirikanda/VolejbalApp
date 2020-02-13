using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.App_Start
{
	public static class WebApiClientConfig
	{
		public static void AddCustomizedHttpClient<TClient, TImplementation>(this IServiceCollection services)
			where TClient : class
			where TImplementation : class, TClient
		{
			services.AddHttpClient<TClient, TImplementation>()
				.ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:9901/"))
				.AddPolicyHandler()
		}
	}
}
