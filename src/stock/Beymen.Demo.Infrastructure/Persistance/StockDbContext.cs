using Beymen.Demo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beymen.Demo.Infrastructure.Persistance;

public class StockDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Stock> Stocks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StockConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}