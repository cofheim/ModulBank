using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulBank.DataAccess.Entities;

namespace ModulBank.DataAccess.Configurations;

public class CellConfiguration : IEntityTypeConfiguration<CellEntity>
{
    public void Configure(EntityTypeBuilder<CellEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasIndex(x => new { x.GameId, x.Row, x.Column })
            .IsUnique();
    }
} 