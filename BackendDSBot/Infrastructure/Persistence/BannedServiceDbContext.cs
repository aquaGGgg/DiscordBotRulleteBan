using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class BannedServiceDbContext : DbContext
{
    public BannedServiceDbContext(DbContextOptions<BannedServiceDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // На шаге 2 добавим EF entities + configurations и они подхватятся автоматически
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BannedServiceDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
