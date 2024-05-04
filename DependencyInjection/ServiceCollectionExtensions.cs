using System.Runtime.CompilerServices;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.Services.TimeServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Havit.Services.Caching;
using KandaEu.Volejbal.Services.Infrastructure;
using KandaEu.Volejbal.Entity;
using KandaEu.Volejbal.Services.Infrastructure.TimeService;
using Microsoft.Extensions.DependencyInjection;
using Havit.Extensions.DependencyInjection;
using Havit.Data.EntityFrameworkCore.Patterns.DependencyInjection;
using KandaEu.Volejbal.Services.DeaktivaceOsob;
using KandaEu.Volejbal.Services.Terminy.EnsureTerminy;
using Hangfire;
using Hangfire.States;
using KandaEu.Volejbal.Services.Mailing;
using Hangfire.Console.Extensions;
using Hangfire.Console;
using Havit.Hangfire.Extensions.Filters;
using Microsoft.ApplicationInsights;
using Havit.AspNetCore.ExceptionMonitoring.Services;

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
			services.AddHostedService<DatabaseMigrationHostedService>();
		}
		services.AddHostedService<DeaktivaceOsobBackgroundService>();
		services.AddHostedService<EnsureTerminyBackgroundService>();

		return services;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static IServiceCollection ConfigureForTests(this IServiceCollection services, bool useInMemoryDb = true)
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
			InstallOnlyLimitedHangfireExtensions = true
		};

		return services.ConfigureForAll(installConfiguration);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static IServiceCollection ConfigureForAll(this IServiceCollection services, InstallConfiguration installConfiguration)
	{
		InstallHavitEntityFramework(services, installConfiguration);
		InstallHavitServices(services);
		InstallHangfire(services, installConfiguration);
		InstallByServiceAttribute(services, installConfiguration);

		return services;
	}

	private static void InstallHavitEntityFramework(IServiceCollection services, InstallConfiguration configuration)
	{
		services.WithEntityPatternsInstaller()
			.AddEntityPatterns()
			//.AddLocalizationServices<Language>()
			.AddDbContext<VolejbalDbContext>(options => _ = configuration.UseInMemoryDb
				? options.UseInMemoryDatabase(nameof(VolejbalDbContext))
				: options.UseSqlServer(configuration.DatabaseConnectionString, c => c.MaxBatchSize(30)))
			.AddDataLayer(typeof(KandaEu.Volejbal.DataLayer.Properties.AssemblyInfo).Assembly);
	}

	private static void InstallHavitServices(IServiceCollection services)
	{
		// HAVIT .NET Framework Extensions
		services.AddSingleton<ITimeService, ApplicationTimeService>();
		services.AddSingleton<ICacheService, MemoryCacheService>();
		services.AddSingleton(new MemoryCacheServiceOptions { UseCacheDependenciesSupport = false });
	}

	private static void InstallHangfire(IServiceCollection services, InstallConfiguration installConfiguration)
	{
		services.AddHangfire((serviceProvider, configuration) =>
		{
			configuration
				.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
				.UseSimpleAssemblyNameTypeSerializer()
				.UseRecommendedSerializerSettings()
				.UseInMemoryStorage()
				//.UseSqlServerStorage(() => new Microsoft.Data.SqlClient.SqlConnection(installConfiguration.DatabaseConnectionString), new SqlServerStorageOptions
				//{
				//	CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
				//	SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
				//	QueuePollInterval = TimeSpan.FromSeconds(5),
				//	UseRecommendedIsolationLevel = true,
				//	DisableGlobalLocks = true,
				//	EnableHeavyMigrations = true
				//})
				.WithJobExpirationTimeout(TimeSpan.FromDays(30)) // historie hangfire
				.UseFilter(new AutomaticRetryAttribute { Attempts = 0 }) // do not retry failed jobs						
				.UseFilter(new ContinuationsSupportAttribute(new HashSet<string> { FailedState.StateName, DeletedState.StateName, SucceededState.StateName })) // only working with AutomaticRetryAttribute with Attempts = 0
				.UseFilter(new CancelRecurringJobWhenAlreadyInQueueOrCurrentlyRunningFilter()); // joby se (v případě "nestihnutí" zpracování) nezařazují opakovaně

			if (!installConfiguration.InstallOnlyLimitedHangfireExtensions)
			{
				// V TestsForLocalDebugging nemáme (a nepotřebujeme) závislost TelemetryClient (ApplicationInsights) ani IExceptionMonitoringService.

				configuration
					//.UseFilter(new SoftErrorNotificationFilter(serviceProvider.GetRequiredService<IMailingService>(), serviceProvider.GetRequiredService<IOptions<SoftErrorNotificationOptions>>()))
					.UseFilter(new ExceptionMonitoringAttribute(serviceProvider.GetRequiredService<IExceptionMonitoringService>())) // zajistíme hlášení chyby v případě selhání jobu
					.UseFilter(new ApplicationInsightAttribute(serviceProvider.GetRequiredService<TelemetryClient>()) { JobNameFunc = backgroundJob => Havit.Hangfire.Extensions.Helpers.JobNameHelper.TryGetSimpleName(backgroundJob.Job, out string simpleName) ? simpleName : backgroundJob.Job.ToString() });
			}

			configuration.UseConsole();
		});

		services.AddHangfireConsoleExtensions();
		//services.AddHangfireSequenceRecurringJobScheduler(); // adds support for SequenceRecurringJobs
	}

	private static void InstallByServiceAttribute(IServiceCollection services, InstallConfiguration configuration)
	{
		services.AddByServiceAttribute(typeof(KandaEu.Volejbal.DataLayer.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles);
		services.AddByServiceAttribute(typeof(KandaEu.Volejbal.Services.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles);
		services.AddByServiceAttribute(typeof(KandaEu.Volejbal.Facades.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles);
	}
}
