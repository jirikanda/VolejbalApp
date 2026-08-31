using System.Runtime.CompilerServices;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.Services.TimeServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Havit.Services.Caching;
using KandaEu.Volejbal.Services.Infrastructure;
using KandaEu.Volejbal.Services.Infrastructure.MigrationTool;
using KandaEu.Volejbal.Entity;
using KandaEu.Volejbal.Services.Infrastructure.TimeService;
using Microsoft.Extensions.DependencyInjection;
using Havit.Extensions.DependencyInjection;
using KandaEu.Volejbal.Services.Jobs;
using Havit.Data.EntityFrameworkCore;
using KandaEu.Volejbal.DataLayer;

namespace KandaEu.Volejbal.DependencyInjection;

public static class ServiceCollectionExtensions
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static IServiceCollection ConfigureForWebAPI(this IServiceCollection services, IConfiguration configuration)
	{
		InstallConfiguration installConfiguration = new InstallConfiguration
		{
			DatabaseConnectionString = configuration.GetConnectionString("Database"),
			ServiceProfiles = new[] { ServiceAttribute.DefaultProfile, ServiceProfiles.WebAPI }
		};

		services.ConfigureForAll(installConfiguration);

		// background jobs
		if (!String.IsNullOrEmpty(installConfiguration.DatabaseConnectionString)) // při spuštění Microsoft.Extensions.ApiDescription.Server nemáme connection string
		{
			services.AddHostedService<RecurringJobsBackgroundService>(); // neblokující — EnsureTerminy (při startu a pak každou hodinu)
		}

		return services;
	}

	/// <summary>
	/// Konfigurace pro MigrationTool - migrace schématu databáze a spuštění data seedů v deployment time.
	/// Záměrně nepoužívá ConfigureForAll, tool potřebuje jen EF Core, DataLayer a MigrationService (bez Services a Facades).
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static IServiceCollection ConfigureForMigrationTool(this IServiceCollection services, IConfiguration configuration)
	{
		InstallConfiguration installConfiguration = new InstallConfiguration
		{
			DatabaseConnectionString = configuration.GetConnectionString("Database"),
			ServiceProfiles = new[] { ServiceAttribute.DefaultProfile }
		};

		InstallHavitEntityFramework(services, installConfiguration);
		InstallHavitServices(services);
		services.AddMemoryCache();
		services.AddByServiceAttribute(typeof(KandaEu.Volejbal.DataLayer.Properties.AssemblyInfo).Assembly, installConfiguration.ServiceProfiles);

		services.AddSingleton<IMigrationService, MigrationService>();

		return services;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
#pragma warning disable IDE0060 // Remove unused parameter
	public static IServiceCollection ConfigureForTests(this IServiceCollection services, bool useInMemoryDb = false)
#pragma warning restore IDE0060 // Remove unused parameter
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
			ServiceProfiles = new[] { ServiceAttribute.DefaultProfile }
		};

		return services.ConfigureForAll(installConfiguration);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static IServiceCollection ConfigureForAll(this IServiceCollection services, InstallConfiguration installConfiguration)
	{
		InstallHavitEntityFramework(services, installConfiguration);
		InstallHavitServices(services);
		InstallByServiceAttribute(services, installConfiguration);

		return services;
	}

	private static void InstallHavitEntityFramework(IServiceCollection services, InstallConfiguration configuration)
	{
		services.AddDbContext<IDbContext, VolejbalDbContext>(optionsBuilder =>
		{
			if (configuration.UseInMemoryDb)
			{
				optionsBuilder.UseInMemoryDatabase(nameof(VolejbalDbContext));
			}
			else
			{
				string databaseConnectionString = configuration.DatabaseConnectionString;
				optionsBuilder.UseSqlServer(databaseConnectionString, c => c.MaxBatchSize(30));
			}
			optionsBuilder.UseDefaultHavitConventions();
		});
		services.AddDataLayerServices();
	}

	private static void InstallHavitServices(IServiceCollection services)
	{
		// HAVIT .NET Framework Extensions
		services.AddSingleton<ITimeService, ApplicationTimeService>();
		services.AddSingleton<ICacheService, MemoryCacheService>();
		services.AddSingleton(new MemoryCacheServiceOptions { UseCacheDependenciesSupport = false });
	}

	private static void InstallByServiceAttribute(IServiceCollection services, InstallConfiguration configuration)
	{
		services.AddByServiceAttribute(typeof(KandaEu.Volejbal.DataLayer.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles);
		services.AddByServiceAttribute(typeof(KandaEu.Volejbal.Services.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles);
		services.AddByServiceAttribute(typeof(KandaEu.Volejbal.Facades.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles);
	}
}
