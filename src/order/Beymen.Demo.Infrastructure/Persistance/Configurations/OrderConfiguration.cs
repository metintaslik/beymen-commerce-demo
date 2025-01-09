using Beymen.Demo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Beymen.Demo.Infrastructure.Persistance.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired().HasColumnName("id").ValueGeneratedNever();

        builder.Property(x => x.TotalQuantityCount).IsRequired().HasColumnName("total_quantity_count");

        builder.HasMany(x => x.OrderItems).WithOne(x => x.Order).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.Id).IsUnique().HasDatabaseName("ix_orders_id");
    }
}