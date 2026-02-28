using Microsoft.EntityFrameworkCore;
using RuneGames.Domain.Entities;
using RuneGames.Domain.Interfaces;

namespace RuneGames.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<LeaderboardEntry> LeaderboardEntries => Set<LeaderboardEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
