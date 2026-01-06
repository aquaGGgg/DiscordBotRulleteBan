using Application.UseCases.Admin.Config;
using Application.UseCases.Admin.Punishments;
using Application.UseCases.Admin.Roulette;
using Application.UseCases.Admin.Stats;
using Application.UseCases.Admin.Tickets;
using Application.UseCases.Admin.Users;
using Application.UseCases.Bot.Eligible;
using Application.UseCases.Bot.Jobs;
using Application.UseCases.Bot.Me;
using Application.UseCases.Bot.Punishments;
using Application.UseCases.Bot.Tickets;
using Application.UseCases.Bot.Users;
using Application.UseCases.Internal.CreateBotJob;
using Application.UseCases.Internal.Punishments;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Internal
        services.AddScoped<CreateBotJobHandler>();
        services.AddScoped<ExpirePunishmentsHandler>();

        // Bot
        services.AddScoped<UpsertUserHandler>();
        services.AddScoped<SyncEligibleUsersHandler>();
        services.AddScoped<GetMeHandler>();
        services.AddScoped<SelfUnbanHandler>();
        services.AddScoped<TransferTicketsHandler>();
        services.AddScoped<PollJobsHandler>();
        services.AddScoped<MarkJobDoneHandler>();
        services.AddScoped<MarkJobFailedHandler>();

        // Admin
        services.AddScoped<ListUsersHandler>();
        services.AddScoped<AdjustTicketsHandler>();
        services.AddScoped<ManualBanHandler>();
        services.AddScoped<ReleasePunishmentHandler>();
        services.AddScoped<GetConfigHandler>();
        services.AddScoped<UpdateConfigHandler>();
        services.AddScoped<GetStatsHandler>();
        services.AddScoped<RunBanRouletteHandler>();
        services.AddScoped<RunTicketRouletteHandler>();

        return services;
    }
}
