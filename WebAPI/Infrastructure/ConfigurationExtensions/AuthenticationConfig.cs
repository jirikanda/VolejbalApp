using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Havit.NewProjectTemplate.WebAPI.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Havit.NewProjectTemplate.WebAPI.Infrastructure.ConfigurationExtensions
{
    public static class AuthenticationConfig
    {
        public static void AddCustomizedAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Havit.NewProjectTemplate.WebAPI.Infrastructure.Security.JwtBearerSettings>(configuration.GetSection("AppSettings:JwtBearer"));
            JwtBearerSettings jwtBearerSettings = configuration.GetSection("AppSettings:JwtBearer").Get<Havit.NewProjectTemplate.WebAPI.Infrastructure.Security.JwtBearerSettings>();

            var authenticationBuilder = services.AddAuthentication(options => options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme);

            authenticationBuilder = authenticationBuilder.AddJwtBearer(options =>
            {
                options.Authority = jwtBearerSettings.Authority;
                options.Audience = jwtBearerSettings.Audience;
            });

            // Pod IClaimsTransformation je standardně zaregistrováno NoopClaimsTransformation
            // Pokud přidáme naší vlastní službu přes Castle Windsor, je tato až druhá a není tak resolvována (uff),
            // Proto tuto službu, kterou nechceme, odebereme (a použijeme službu zaregistrovanou přes Castle Windsor).
            services.Remove(services.Where(item => item.ImplementationType == typeof(NoopClaimsTransformation)).Single());

            addCustomizedAuthenticationCalled = true;

        }
        private static bool addCustomizedAuthenticationCalled = false;

        public static string[] GetAuthenticationSchemes(IConfiguration configuration)
        {
            if (!addCustomizedAuthenticationCalled)
            {
                throw new InvalidOperationException("Nejdříve je třeba zavolat metodu AddCustomizedAuthentication.");
            }

	        return new string[] { JwtBearerDefaults.AuthenticationScheme };
        }
    }
}
