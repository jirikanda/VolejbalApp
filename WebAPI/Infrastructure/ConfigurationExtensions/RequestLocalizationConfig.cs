using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions;

public static class RequestLocalizationConfig
{
	public static void AddCustomizedRequestLocalization(this IServiceCollection services)
	{
		CultureInfo czechCulture = new CultureInfo("cs-CZ");
		services.Configure<RequestLocalizationOptions>(options =>
		{
			options.DefaultRequestCulture = new RequestCulture(czechCulture, czechCulture);
			options.SupportedCultures = new List<CultureInfo> { czechCulture };
			options.SupportedUICultures = new List<CultureInfo> { czechCulture };
			options.RequestCultureProviders.Clear();
		});
	}
}
