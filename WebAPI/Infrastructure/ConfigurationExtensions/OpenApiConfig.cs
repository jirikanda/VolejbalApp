using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions
{
    public static class OpenApiConfig
    {
		public static void AddCustomizedOpenApi(this IServiceCollection services)
		{
			services.AddOpenApiDocument(c =>
			{
				c.DocumentName = "current";
				c.Title = "VolejbalApp";
				c.Version = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(KandaEu.Volejbal.WebAPI.Properties.AssemblyInfo).Assembly.Location).ProductVersion;				
			});
		}

        public static void UseCustomizedOpenApiSwaggerUI(this IApplicationBuilder app)
        {
			app.UseOpenApi();
			app.UseSwaggerUi3();
		}
    }
}
