using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Blazor
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<EventAggregator.Blazor.IEventAggregator, EventAggregator.Blazor.EventAggregator>();

			//services.Configure<RequestLocalizationOptions>(options =>
			//{
			//	options.DefaultRequestCulture = new RequestCulture("en-US");
			//});

		}

		public void Configure(IComponentsApplicationBuilder app)
		{
			app.AddComponent<App>("app");

			var cultureInfo = new CultureInfo("cs-CZ");
			CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
			CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
			//app.UseRequestLocalization();
		}
	}
}
