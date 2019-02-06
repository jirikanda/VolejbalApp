using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Havit.VolejbalApp.Services.Mailing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Havit.VolejbalApp.WebAPI.Infrastructure.ConfigurationExtensions
{
    public static class MailingConfig
    {
	    public static void AddCustomizedMailing(this IServiceCollection services, IConfiguration configuration)
	    {
			services.Configure<MailingOptions>(configuration.GetSection("AppSettings:MailingOptions"));
		}
	}
}
