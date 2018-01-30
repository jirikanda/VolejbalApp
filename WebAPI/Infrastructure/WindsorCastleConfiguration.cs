using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Castle.MicroKernel.Registration;
using Havit.AspNetCore.Mvc.ExceptionMonitoring.Filters;
using Havit.Extensions.DependencyInjection.CastleWindsor.AspNetCore;
using Havit.NewProjectTemplate.Facades.Infrastructure.Security.Authentication;
using Havit.NewProjectTemplate.Facades.Infrastructure.Security.Claims;
using Havit.NewProjectTemplate.WebAPI.Infrastructure.Security;
using Havit.NewProjectTemplate.WindsorInstallers;

namespace Havit.NewProjectTemplate.WebAPI.Infrastructure
{
    public static class WindsorCastleConfiguration
    {
        /// <summary>
        /// Returns new Castle Windsor container configured for WebAPI.
        /// </summary>
        public static IWindsorContainer CreateWindsorContainer(IConfiguration configuration)
        {
            IWindsorContainer container = new WindsorContainer();
            container.ConfigureForWebAPI(configuration);
            container.Register(Component.For<ErrorMonitoringFilter>().LifestylePerAspNetCoreRequest());
            container.Register(Component.For<IApplicationAuthenticationService>().ImplementedBy<ApplicationAuthenticationService>().LifestylePerAspNetCoreRequest());
            container.Register(Component.For<IUserContextInfoBuilder>().ImplementedBy<Infrastructure.Security.UserContextInfoBuilder>().LifestylePerAspNetCoreRequest());
            return container;
        }

        /// <summary>
        /// Returns service provider wrapping Castle Windsor container.
        /// </summary>
        public static IServiceProvider AddCustomizedServiceProvider(this IServiceCollection services, IWindsorContainer container)
        {
            return WindsorRegistrationHelper.CreateServiceProvider(container, services);
        }
    }
}
