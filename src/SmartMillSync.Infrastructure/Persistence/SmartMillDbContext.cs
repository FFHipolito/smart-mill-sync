using Microsoft.EntityFrameworkCore;
using SmartMillSync.Domain.Entities;

namespace SmartMillSync.Infrastructure.Persistence;

public sealed class SmartMillDbContext(DbContextOptions<SmartMillDbContext> options)
    : DbContext(options)
{
    public DbSet<WoodDelivery> WoodDeliveries => Set<WoodDelivery>();
    public DbSet<GasTelemetryRecord> GasTelemetryRecords => Set<GasTelemetryRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartMillDbContext).Assembly);
    }
}
