using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulBank.DataAccess.Entities;

namespace ModulBank.DataAccess.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<GameEntity>
{
    public void Configure(EntityTypeBuilder<GameEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.RowVersion)
            .IsRowVersion();
            
        builder.HasOne(x => x.Player1)
            .WithMany()
            .HasForeignKey("Player1Id")
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.Player2)
            .WithMany()
            .HasForeignKey("Player2Id")
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(x => x.CurrentPlayer)
            .WithMany()
            .HasForeignKey(x => x.CurrentPlayerId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasMany(x => x.Cells)
            .WithOne(x => x.Game)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 