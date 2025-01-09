using Beymen.Demo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beymen.Demo.Infrastructure.Persistance.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired().HasColumnName("id").UseIdentityColumn().HasIdentityOptions(1, 1);

        builder.Property(x => x.OrderId).IsRequired().HasColumnName("order_id");

        builder.Property(x => x.ProductId).IsRequired().HasColumnName("product_id");

        builder.Property(x => x.Quantity).IsRequired().HasColumnName("quantity");

        builder.HasOne(x => x.Order).WithMany(x => x.OrderItems).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.Id).IsUnique().HasDatabaseName("ix_order_items_id");
        builder.HasIndex(x => x.OrderId).HasDatabaseName("ix_order_items_order_id");
    }
}