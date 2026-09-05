namespace KandaEu.Volejbal.Web.Infrastructure.ConfigurationExtensions;

public static class CorsConfig
{
	public static void AddCustomizedCors(this IServiceCollection services, IConfiguration configuration)
	{
		string[] allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

		services.AddCors(options => options.AddDefaultPolicy(policy => policy
			.WithOrigins(allowedOrigins)
			.AllowAnyHeader()
			.AllowAnyMethod()));
	}
}
