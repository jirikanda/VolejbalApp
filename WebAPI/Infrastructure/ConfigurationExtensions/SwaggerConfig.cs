using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions
{
    public static class SwaggerConfig
    {
        public static void AddCustomizedSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("current", new Info { Title = "VolejbalApp", Version = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(KandaEu.Volejbal.WebAPI.Properties.AssemblyInfo).Assembly.Location).ProductVersion });
                c.CustomSchemaIds(type => type.FullName);
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Havit.VolejbalApp.WebAPI.xml"));
                c.DescribeAllEnumsAsStrings();
            });

        }

        public static void UseCustomizedSwaggerAndUI(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/current/swagger.json", "Current");
            });
        }
    }
}
