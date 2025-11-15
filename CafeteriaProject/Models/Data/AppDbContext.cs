using Microsoft.EntityFrameworkCore;

namespace CafeteriaProject.Models.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed constant MenuItems (the menu)
            modelBuilder.Entity<MenuItem>().HasData(
                new MenuItem { Id = 1, ItemName = "Burger", Price = 50 },
                new MenuItem { Id = 2, ItemName = "Fries", Price = 30 },
                new MenuItem { Id = 3, ItemName = "Pizza", Price = 100 },
                new MenuItem { Id = 4, ItemName = "Cola", Price = 20 }
            );
        }
    }
}
