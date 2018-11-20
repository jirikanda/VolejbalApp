using System;
using System.IO;
using System.Runtime.CompilerServices;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Havit.NewProjectTemplate.DataLayer.DataSources.Security;
using Havit.NewProjectTemplate.DataLayer.Seeds.Core.Common;
using Havit.NewProjectTemplate.Entity;
using Havit.NewProjectTemplate.Services.Infrastructure;
using Havit.NewProjectTemplate.Services.Infrastructure.TimeService;
using Havit.Data.EntityFrameworkCore.Patterns.Windsor;
using Havit.Data.EntityFrameworkCore.Patterns.Windsor.Installers;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.Extensions.DependencyInjection.CastleWindsor;
using Havit.Extensions.DependencyInjection.CastleWindsor.AspNetCore;
using Havit.Services;
using Havit.Services.TimeServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Havit.Services.Caching;
using System.Runtime.Caching;

namespace Havit.NewProjectTemplate.WindsorInstallers
{
	public static class WindsorInstaller
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static IWindsorContainer ConfigureForWebAPI(this IWindsorContainer container, IConfiguration configuration)
		{
			InstallConfiguration installConfiguration = new InstallConfiguration
			{
				DatabaseConnectionString = configuration.GetConnectionString("Database"),
				ServiceProfiles = new[] { ServiceAttribute.DefaultProfile, ServiceProfiles.WebAPI },
				ScopedLifestyle = lf => lf.PerAspNetCoreRequest()
			};

			return container.ConfigureForAll(installConfiguration);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static IWindsorContainer ConfigureForTests(this IWindsorContainer container)
		{
			string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			if (string.IsNullOrEmpty(environment))
			{
				environment = "Development";
			}

			IConfigurationRoot configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{environment}.json", true)
				.Build();

			InstallConfiguration installConfiguration = new InstallConfiguration
			{
				DatabaseConnectionString = configuration.GetConnectionString("Database"),
				ServiceProfiles = new[] { ServiceAttribute.DefaultProfile },
				ScopedLifestyle = lf => lf.PerThread,
			};

			return container.ConfigureForAll(installConfiguration);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static IWindsorContainer ConfigureForAll(this IWindsorContainer container, InstallConfiguration installConfiguration)
		{
			// umožní resolvovat i kolekce závislostí - IEnumerable<IDependency>
			container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

			// facilities
			container.AddFacility<TypedFactoryFacility>();
			container.Register(Component.For(typeof(IServiceFactory<>)).AsFactory());

			InstallHavitEntityFramework(container, installConfiguration);
			InstallHavitServices(container);
			InstallByServiceAttribute(container, installConfiguration);
			InstallAuthorizationHandlers(container, installConfiguration);

			return container;
		}

		private static void InstallHavitEntityFramework(IWindsorContainer container, InstallConfiguration configuration)
		{
			container.WithEntityPatternsInstaller(new ComponentRegistrationOptions { GeneralLifestyle = configuration.ScopedLifestyle })
				.RegisterEntityPatterns()
				//.RegisterLocalizationServices<Language>()
				.RegisterDbContext<NewProjectTemplateDbContext>(new DbContextOptionsBuilder<NewProjectTemplateDbContext>().UseSqlServer(configuration.DatabaseConnectionString).Options)
				.RegisterDataLayer(typeof(ILoginAccountDataSource).Assembly);
		}

		private static void InstallHavitServices(IWindsorContainer container)
		{
			// HAVIT .NET Framework Extensions
			container.Register(Component.For<ITimeService>().ImplementedBy<ApplicationTimeService>().LifestyleSingleton());
			container.Register(Component.For<ICacheService>().Instance(new ObjectCacheService(new MemoryCache("NewProjectTemplate"), false)));
		}

		private static void InstallByServiceAttribute(IWindsorContainer container, InstallConfiguration configuration)
		{
			container.InstallByServiceAttibute(typeof(Havit.NewProjectTemplate.DataLayer.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles, configuration.ScopedLifestyle);
			container.InstallByServiceAttibute(typeof(Havit.NewProjectTemplate.Services.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles, configuration.ScopedLifestyle);
			container.InstallByServiceAttibute(typeof(Havit.NewProjectTemplate.Facades.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles, configuration.ScopedLifestyle);
		}

		private static void InstallAuthorizationHandlers(IWindsorContainer container, InstallConfiguration installConfiguration)
		{
			container.Register(Classes.FromAssembly(typeof(Havit.NewProjectTemplate.Services.Properties.AssemblyInfo).Assembly)
				.BasedOn<IAuthorizationHandler>()
				.WithService.FromInterface(typeof(IAuthorizationHandler))
					.Configure(configurer => installConfiguration.ScopedLifestyle(configurer.LifeStyle))
			   );
		}
	}
}