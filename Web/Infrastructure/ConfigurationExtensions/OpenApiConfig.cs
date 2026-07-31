namespace KandaEu.Volejbal.Web.Infrastructure.ConfigurationExtensions;

public static class OpenApiConfig
{
	public static void AddCustomizedOpenApi(this IServiceCollection services)
	{
		services.AddOpenApiDocument(c =>
		{
			c.DocumentName = "current";
			c.Title = "VolejbalApi";
			c.Version = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(KandaEu.Volejbal.Web.Properties.AssemblyInfo).Assembly.Location).ProductVersion;
		});
	}

	public static void UseCustomizedOpenApiSwaggerUI(this IApplicationBuilder app)
	{
		app.UseOpenApi();
		app.UseSwaggerUi();
	}
}
