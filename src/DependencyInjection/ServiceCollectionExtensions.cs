using System.Runtime.CompilerServices;
using Havit.Extensions.DependencyInjection;
using Havit.Extensions.DependencyInjection.Abstractions;
using KandaEu.Volejbal.DataLayer;
using KandaEu.Volejbal.DataLayer.Cosmos;
using KandaEu.Volejbal.Services.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KandaEu.Volejbal.DependencyInjection;

public static class ServiceCollectionExtensions
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static IServiceCollection ConfigureForWebAPI(this IServiceCollection services, IConfiguration configuration)
	{
		InstallConfiguration installConfiguration = new InstallConfiguration
		{
			CosmosOptions = GetCosmosOptions(configuration),
			ServiceProfiles = new[] { ServiceAttribute.DefaultProfile }
		};

		services.ConfigureForAll(installConfiguration);

		// Aplikace nemá žádné pravidelné úlohy - termíny se doplňují líně při čtení jejich seznamu
		// (viz Facades/Terminy/TerminFacade.GetTerminyAsync), deaktivace osob je ruční akcí.

		return services;
	}

	/// <summary>
	/// Konfigurace pro MigrationTool - založení databáze a kontejnerů, případně jednorázový převod dat
	/// ze SQL Serveru. Záměrně nepoužívá ConfigureForAll, tool potřebuje jen přístup k datům.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static IServiceCollection ConfigureForMigrationTool(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDataLayerServices(GetCosmosOptions(configuration));

		return services;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static IServiceCollection ConfigureForTests(this IServiceCollection services)
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
			CosmosOptions = GetCosmosOptions(configuration),
			ServiceProfiles = new[] { ServiceAttribute.DefaultProfile }
		};

		return services.ConfigureForAll(installConfiguration);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static IServiceCollection ConfigureForAll(this IServiceCollection services, InstallConfiguration installConfiguration)
	{
		services.AddDataLayerServices(installConfiguration.CosmosOptions);
		InstallTimeProvider(services);
		InstallByServiceAttribute(services, installConfiguration);

		return services;
	}

	private static CosmosOptions GetCosmosOptions(IConfiguration configuration)
	{
		return configuration.GetSection("Cosmos").Get<CosmosOptions>() ?? new CosmosOptions();
	}

	private static void InstallTimeProvider(IServiceCollection services)
	{
		// Pražský čas pro celý server - proces v Azure běží v UTC, viz PragueTimeProvider.
		services.AddSingleton<TimeProvider, PragueTimeProvider>();
	}

	private static void InstallByServiceAttribute(IServiceCollection services, InstallConfiguration configuration)
	{
		// DataLayer se nescanuje - nemá jedinou třídu s [Service], registraci repozitářů obstarává
		// AddDataLayerServices() (viz ConfigureForAll).
		services.AddByServiceAttribute(typeof(KandaEu.Volejbal.Services.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles);
		services.AddByServiceAttribute(typeof(KandaEu.Volejbal.Facades.Properties.AssemblyInfo).Assembly, configuration.ServiceProfiles);
	}
}
