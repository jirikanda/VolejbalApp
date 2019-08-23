using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace KandaEu.Blazor
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<EventAggregator.Blazor.IEventAggregator, EventAggregator.Blazor.EventAggregator>();
        }   

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");

            var cultureInfo = new CultureInfo("cs-CZ");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        }
    }
}
