using Beymen.Demo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beymen.Demo.Infrastructure.Persistance.Configurations;

public class BaseEntityConfiguration : IEntityTypeConfiguration<BaseEntity>
{
    public void Configure(EntityTypeBuilder<BaseEntity> builder)
    {
        builder.Property(x => x.CreatedAt).IsRequired().HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt).IsRequired(false).HasColumnName("updated_at");

        builder.Property(x => x.IsDeleted).IsRequired().HasColumnName("is_deleted").HasDefaultValue(false);
    }
}