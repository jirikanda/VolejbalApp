using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KandaEu.Volejbal.Services.Mailing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions;

public static class MailingConfig
{
    public static void AddCustomizedMailing(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MailingOptions>(configuration.GetSection("AppSettings:MailingOptions"));
    }
}
