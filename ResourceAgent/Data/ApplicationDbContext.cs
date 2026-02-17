using Microsoft.EntityFrameworkCore;
using ResourceAgent.Models;

namespace ResourceAgent.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Resource> Resources { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>().HasData(
            new Resource { Id = 1, Type = "Water", Location = "Quartier Nord", Quantity = 100, IsAvailable = true },
            new Resource { Id = 2, Type = "Food", Location = "Quartier Nord", Quantity = 50, IsAvailable = true },
            new Resource { Id = 3, Type = "Blankets", Location = "Quartier Sud", Quantity = 30, IsAvailable = true }
        );
    }
}
