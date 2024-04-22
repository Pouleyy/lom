using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Entities.Context;

public sealed class LomDbContext : DbContext
{
    public LomDbContext()
    {
        Database.EnsureCreated();
        ChangeTracker.LazyLoadingEnabled = false;
    }

    public LomDbContext(DbContextOptions<LomDbContext> options) : base(options)
    {
        Database.EnsureCreated();
        ChangeTracker.LazyLoadingEnabled = false;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Family.OnModelCreating(modelBuilder);
        Player.OnModelCreating(modelBuilder);
        Server.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            options.UseNpgsql("Host=localhost;Username=postgres;Password=postgres;Port=5432;Database=lom");
        }
    }

    public DbSet<Family> Families => Set<Family>();

    public DbSet<Player> Players => Set<Player>();

    public DbSet<Server> Servers => Set<Server>();
}