using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.App_Start;

public static class WebApiClientConfig
{
    public static void AddCustomizedHttpClient<TClient, TImplementation>(this IServiceCollection services, IConfiguration configuration)
        where TClient : class
        where TImplementation : class, TClient
    {
        string webAPIConnectionString = configuration.GetConnectionString("WebAPI");
        services.AddHttpClient<TClient, TImplementation>().ConfigureHttpClient(c => c.BaseAddress = new Uri(webAPIConnectionString));
    }
}
