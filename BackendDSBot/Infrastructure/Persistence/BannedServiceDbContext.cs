using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class BannedServiceDbContext : DbContext
{
    public BannedServiceDbContext(DbContextOptions<BannedServiceDbContext> options) : base(options) { }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<PunishmentEntity> Punishments => Set<PunishmentEntity>();
    public DbSet<TicketTransferEntity> TicketTransfers => Set<TicketTransferEntity>();
    public DbSet<PunishmentHistoryEntity> PunishmentHistory => Set<PunishmentHistoryEntity>();
    public DbSet<ConfigEntity> Config => Set<ConfigEntity>();
    public DbSet<EligibleUserEntity> EligibleUsersSnapshot => Set<EligibleUserEntity>();
    public DbSet<RouletteRoundEntity> RouletteRounds => Set<RouletteRoundEntity>();
    public DbSet<BotJobEntity> BotJobs => Set<BotJobEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BannedServiceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
