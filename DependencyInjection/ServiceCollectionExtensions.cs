﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using Havit.Extensions.DependencyInjection.Abstractions;
using Havit.Services;
using Havit.Services.TimeServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Havit.Services.Caching;
using System.Runtime.Caching;
using KandaEu.Volejbal.Services.Infrastructure;
using KandaEu.Volejbal.Entity;
using KandaEu.Volejbal.Services.Infrastructure.TimeService;
using KandaEu.Volejbal.DataLayer.DataSources;
using Microsoft.Extensions.DependencyInjection;
using Havit.Extensions.DependencyInjection;
using Havit.Data.EntityFrameworkCore.Patterns.DependencyInjection;
using KandaEu.Volejbal.Entity.AadAuthentication;

namespace KandaEu.Volejbal.DependencyInjection
{
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

			return services.ConfigureForAll(installConfiguration);
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
			DbContextOptions options = configuration.UseInMemoryDb
				? new DbContextOptionsBuilder<VolejbalDbContext>().UseInMemoryDatabase(nameof(VolejbalDbContext)).Options
				: new DbContextOptionsBuilder<VolejbalDbContext>().UseSqlServer(configuration.DatabaseConnectionString, c => c.MaxBatchSize(30)).UseSqlServerAadAuthentication().Options;

			services.WithEntityPatternsInstaller()
				.AddEntityPatterns()
				//.AddLocalizationServices<Language>()
				.AddDbContext<VolejbalDbContext>(options)
				.AddDataLayer(typeof(KandaEu.Volejbal.DataLayer.Properties.AssemblyInfo).Assembly);
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
}