using Application.Abstractions.Persistence;
using Application.Abstractions.Random;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Random;
using Infrastructure.Time;
using Infrastructure.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPunishmentRepository, PunishmentRepository>();
        services.AddScoped<ITicketTransferRepository, TicketTransferRepository>();
        services.AddScoped<IPunishmentHistoryRepository, PunishmentHistoryRepository>();
        services.AddScoped<IConfigRepository, ConfigRepository>();
        services.AddScoped<IEligibleUsersRepository, EligibleUsersRepository>();
        services.AddScoped<IRouletteRoundRepository, RouletteRoundRepository>();
        services.AddScoped<IBotJobRepository, BotJobRepository>();

        // Read-model queries
        services.AddScoped<IReadModelsQueries, ReadModelsQueries>();

        // UnitOfWork / Transactions
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // Providers
        services.AddSingleton<ITimeProvider, SystemTimeProvider>();
        services.AddSingleton<IRandomProvider, SystemRandomProvider>();

        return services;
    }
}
