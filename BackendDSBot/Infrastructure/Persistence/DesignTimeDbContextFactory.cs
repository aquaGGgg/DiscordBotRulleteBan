using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BannedServiceDbContext>
{
    public BannedServiceDbContext CreateDbContext(string[] args)
    {
        // Нужен для стабильной работы `dotnet ef ...` (миграции/апдейты)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connStr = configuration.GetConnectionString("Default")
                     ?? throw new InvalidOperationException("Missing connection string: ConnectionStrings:Default");

        var optionsBuilder = new DbContextOptionsBuilder<BannedServiceDbContext>();
        optionsBuilder.UseNpgsql(connStr, npgsql =>
        {
            npgsql.MigrationsAssembly(typeof(BannedServiceDbContext).Assembly.FullName);
        });

        return new BannedServiceDbContext(optionsBuilder.Options);
    }
}
