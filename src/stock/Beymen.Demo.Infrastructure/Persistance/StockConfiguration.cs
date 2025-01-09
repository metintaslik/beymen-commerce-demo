using Beymen.Demo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beymen.Demo.Infrastructure.Persistance;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable("stocks");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired().HasColumnName("id").ValueGeneratedNever();

        builder.Property(x => x.ProductId).IsRequired().HasColumnName("product_id");

        builder.Property(x => x.Quantity).IsRequired().HasColumnName("quantity");

        builder.Property(x => x.CreatedAt).IsRequired().HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt).IsRequired(false).HasColumnName("updated_at");

        builder.Property(x => x.IsDeleted).IsRequired().HasColumnName("is_deleted").HasDefaultValue(false);

        builder.HasIndex(x => x.Id).IsUnique().HasDatabaseName("ix_stocks_id");
        builder.HasIndex(x => x.ProductId).IsUnique().HasDatabaseName("ix_stocks_product_id");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}