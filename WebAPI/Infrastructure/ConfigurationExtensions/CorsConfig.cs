using Microsoft.Extensions.Options;

namespace KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions;

public static class CorsConfig
{
	public static void AddCustomizedCors(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<KandaEu.Volejbal.WebAPI.Infrastructure.Cors.CorsOptions>(configuration.GetSection("AppSettings:Cors"));
		services.AddCors();
	}

	public static void UseCustomizedCors(this IApplicationBuilder app, IOptions<KandaEu.Volejbal.WebAPI.Infrastructure.Cors.CorsOptions> corsOptions)
	{
		string[] allowedOrigins = corsOptions.Value.AllowedOrigins.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(item => item.Trim()).ToArray();
		app.UseCors(policy =>
		{
			policy
				.AllowAnyOrigin()
			.WithHeaders("Accept", "Content-Type", "Origin", "Authorization")
			//.AllowCredentials()
			.AllowAnyMethod()
			.SetPreflightMaxAge(TimeSpan.FromHours(1));
		});
	}
}
