using KandaEu.Volejbal.Services.Mailing;

namespace KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions;

public static class MailingConfig
{
	public static void AddCustomizedMailing(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<MailingOptions>(configuration.GetSection("AppSettings:MailingOptions"));
	}
}
