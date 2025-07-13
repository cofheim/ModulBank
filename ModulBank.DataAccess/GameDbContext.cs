using Microsoft.EntityFrameworkCore;
using ModulBank.DataAccess.Configurations;
using ModulBank.DataAccess.Entities;

namespace ModulBank.DataAccess;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<GameEntity> Games { get; set; } = null!;
    public DbSet<PlayerEntity> Players { get; set; } = null!;
    public DbSet<CellEntity> Cells { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new GameConfiguration());
        modelBuilder.ApplyConfiguration(new PlayerConfiguration());
        modelBuilder.ApplyConfiguration(new CellConfiguration());
    }
} 