using Brewery.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Brewery.Persistence.Data
{
    public class BreweryDbContext : DbContext
    {
        public BreweryDbContext(DbContextOptions<BreweryDbContext> options) : base(options) { }

        public DbSet<BreweryEntity> Breweries => Set<BreweryEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BreweryEntity>().HasKey(b => b.Id);
            modelBuilder.Entity<BreweryEntity>().HasIndex(b => b.Name);
            modelBuilder.Entity<BreweryEntity>().HasIndex(b => b.City);
        }
    }
}
