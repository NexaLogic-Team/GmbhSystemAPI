using GmbhSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GmbhSystem.Persistence;

public class GmbhSystemDbContext : DbContext
{
    
    public GmbhSystemDbContext(DbContextOptions<GmbhSystemDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<LeaderItem> LeaderItems => Set<LeaderItem>(); // Added

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GmbhSystemDbContext).Assembly);
    }
}