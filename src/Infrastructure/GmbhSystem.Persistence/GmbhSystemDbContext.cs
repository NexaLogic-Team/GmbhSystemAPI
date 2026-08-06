using GmbhSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GmbhSystem.Persistence;

public class GmbhSystemDbContext : DbContext
{
    
    public GmbhSystemDbContext(DbContextOptions<GmbhSystemDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<LeaderItem> LeaderItems { get; set; }
    public DbSet<LeadershipHeader> LeadershipHeaders { get; set; } // ထပ်ပေါင်းရန်
    // Services Section
    public DbSet<ServiceItem> ServiceItems { get; set; }
    public DbSet<ServiceHeader> ServiceHeaders { get; set; }
    public DbSet<HomeSection> HomeSections => Set<HomeSection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GmbhSystemDbContext).Assembly);
    }
}