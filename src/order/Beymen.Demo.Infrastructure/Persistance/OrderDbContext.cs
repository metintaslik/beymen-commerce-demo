using Beymen.Demo.Domain.Entities;
using Beymen.Demo.Infrastructure.Persistance.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Beymen.Demo.Infrastructure.Persistance;

public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new BaseEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}