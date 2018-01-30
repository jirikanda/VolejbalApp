using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Havit.Data.Entity.Patterns.Windsor;
using Havit.Data.Entity.Patterns.Windsor.Installers;
using Havit.Services.TimeServices;
using Havit.Diagnostics.Contracts;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.Extensions.DependencyInjection.CastleWindsor;
using Havit.Extensions.DependencyInjection.CastleWindsor.AspNetCore;
using Havit.NewProjectTemplate.DataLayer.DataSources.Localizations;
using Havit.NewProjectTemplate.Entity;
using Havit.NewProjectTemplate.Model.Localizations;
using Havit.NewProjectTemplate.Services.Infrastructure;
using Havit.NewProjectTemplate.Services.Infrastructure.TimeService;
using Havit.Services;
using Havit.Services.FileStorage;
using Microsoft.Extensions.Configuration;
	
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
	            ModelStoreDirectory = @"App_Data\EF_ModelStore",
                ServiceProfiles = new[] { ServiceAttribute.DefaultProfile, ServiceProfiles.WebAPI },
                ScopedLifestyle = lf => lf.PerAspNetCoreRequest(),
	        };

            return container.ConfigureForAll(installConfiguration);
	    }

		[MethodImpl(MethodImplOptions.NoInlining)]
	    public static IWindsorContainer ConfigureForTests(this IWindsorContainer container)
	    {
	        InstallConfiguration installConfiguration = new InstallConfiguration
	        {
	            DatabaseConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnectionString"]?.ConnectionString,
	            ModelStoreDirectory = "EF_ModelStore",
                ServiceProfiles = new[] { ServiceAttribute.DefaultProfile },
	            ScopedLifestyle = lf => lf.PerThread,
			};
			
			return container.ConfigureForAll(installConfiguration);
	    }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IWindsorContainer ConfigureForAll(this IWindsorContainer container, InstallConfiguration configuration)
		{
			// umožní resolvovat i kolekce závislostí - IEnumerable<IDependency>
			container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));
			
			// facilities
			container.AddFacility<TypedFactoryFacility>();
            container.Register(Component.For(typeof(IServiceFactory<>)).AsFactory());

            InstallHavitServices(container);
		    InstallHavitEntityFramework(container, configuration);

            container.InstallByServiceAttibute(typeof(Havit.NewProjectTemplate.DataLayer.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles, configuration.ScopedLifestyle);
            container.InstallByServiceAttibute(typeof(Havit.NewProjectTemplate.Services.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles, configuration.ScopedLifestyle);
            container.InstallByServiceAttibute(typeof(Havit.NewProjectTemplate.Facades.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles, configuration.ScopedLifestyle);

			return container;
		}

	    private static void InstallHavitServices(IWindsorContainer container)
	    {
	        // HAVIT .NET Framework Extensions
	        container.Register(Component.For<ITimeService>().ImplementedBy<ApplicationTimeService>().LifestyleSingleton());
	    }

        private static void InstallHavitEntityFramework(IWindsorContainer container, InstallConfiguration configuration)
	    {
	        // HAVIT Entity Framework
	        container.WithEntityPatternsInstaller(new ComponentRegistrationOptions { GeneralLifestyle = configuration.ScopedLifestyle })
	            .RegisterEntityPatterns()
	            .RegisterLocalizationServices<Language>()
	            .RegisterDataLayer(typeof(ILanguageDataSource).Assembly);

	        container.Register(
	            Classes.From(typeof(NewProjectTemplateDbContext))
	                .Pick()
	                .WithService.Select((type, baseTypes) => new[] { typeof(Havit.Data.Entity.IDbContext) })
	                .Configure(c =>
	                {
	                    configuration.ScopedLifestyle(c.LifeStyle)
	                        .DependsOn(Dependency.OnValue("connectionString", configuration.DatabaseConnectionString));
	                }));
	        
            DbConfiguration.SetConfiguration(new NewProjectTemplateDbConfiguration(configuration.DatabaseConnectionString, configuration.ModelStoreDirectory));
	        NewProjectTemplateDbContext.SetEntityFrameworkMigrations();
        }
	}
}