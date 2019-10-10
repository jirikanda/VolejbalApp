using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.WindsorInstallers
{
	public class WebApiServiceProviderFactory : IServiceProviderFactory<IWindsorContainer>
	{
		private readonly IConfiguration configuration;

		public WebApiServiceProviderFactory(IConfiguration configuration)
		{
			this.configuration = configuration;
		}

		public IWindsorContainer CreateBuilder(IServiceCollection services)
		{
			var container = services.GetSingletonServiceOrNull<IWindsorContainer>();

			if (container == null)
			{
				container = new WindsorContainer();
				container.ConfigureForWebAPI(configuration);
				services.AddSingleton(container);
			}

			container.AddServices(services);

			return container;
		}

		public IServiceProvider CreateServiceProvider(IWindsorContainer containerBuilder)
		{
			return containerBuilder.Resolve<IServiceProvider>();
		}
	}
}
