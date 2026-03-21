using Microsoft.EntityFrameworkCore;
using CatalogAPI.Domain.Entities;

namespace CatalogAPI.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<UserGame> UserGames => Set<UserGame>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserGame>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.GameId).IsRequired();
                entity.Property(e => e.Price).IsRequired();
                entity.Property(e => e.PurchasedAt).IsRequired();
            });
        }
    }
}
