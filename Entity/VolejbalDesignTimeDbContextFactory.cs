﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace KandaEu.Volejbal.Entity;

public class VolejbalDesignTimeDbContextFactory : IDesignTimeDbContextFactory<VolejbalDbContext>
{
    public VolejbalDbContext CreateDbContext(string[] args)
    {
        // Příkazy pro tooling EF Core Migrations (Add-Migration, ...) tooling získávají DbContext z této metody.
        // Stejně tak  CodeGenerator.
        // InMemory provider lenze pro tooling EF Core Migrations, je potřeba provider pro SqlServer.
        // Provider pro SQL Server nejspíš neumí použít connection string z app.configu, lze řešit přes appSettings.json, pokud je startup projectem ASP.NET Core aplikace.
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (string.IsNullOrEmpty(environment))
        {
            environment = "Development";
        }

        // Current path je pro CodeGenerator DataLayer
        // potřebujeme načíst konfiguraci od Entity, resp. Entity\bin\Debug(Release)\nestandard2.0.
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location))
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", true)
            .Build();

        string connectionString = configuration.GetConnectionString("Database");

        return new VolejbalDbContext(new DbContextOptionsBuilder<VolejbalDbContext>().UseSqlServer(connectionString).Options);
    }
}
