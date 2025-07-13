using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulBank.DataAccess.Entities;

namespace ModulBank.DataAccess.Configurations;

public class PlayerConfiguration : IEntityTypeConfiguration<PlayerEntity>
{
    public void Configure(EntityTypeBuilder<PlayerEntity> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.SessionId)
            .IsRequired();
    }
} 