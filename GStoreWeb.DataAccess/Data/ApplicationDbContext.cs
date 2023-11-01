using GStoreWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GStoreWeb.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ApplicationUser> applicationUsers { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id=1, DisplayOrder=1, Name="Graphic Cards"},
                new Category { Id=2, DisplayOrder=2, Name="Processors"}
                );
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Nvidia RTX 2060",
                    Description = "An Nvidia RTX 20 series base graphics card",
                    CategoryId = 1,
                    Seller="AMD",
                    Price = 280,
                    ImageUrl=""
                },
                new Product
                {
                    Id = 2,
                    Name = "Nvidia RTX 3060",
                    Description = "An Nvidia RTX 30 series base graphics card",
                    CategoryId = 1,
                    Seller = "Gigabyte",
                    Price = 380,
                    ImageUrl = ""
                },
                new Product
                {
                    Id = 3,
                    Name = "Nvidia RTX 3080",
                    Description = "An Nvidia RTX 30 series powerful graphics card",
                    CategoryId = 1,
                    Seller = "MSI",
                    Price = 1200,
                    ImageUrl = ""
                },
                new Product
                {
                    Id = 4,
                    Name = "Nvidia RTX 3080 Ti",
                    Description = "The Ti version of the RTX 3080 graphics card",
                    CategoryId = 1,
                    Seller = "Gigabyte",
                    Price = 1450,
                    ImageUrl = ""
                },
                new Product
                {
                    Id = 5,
                    Name = "Nvidia RTX 3090",
                    Description = "An Nvidia RTX 30 series most powerful graphics card",
                    CategoryId = 1,
                    Seller = "Gigabyte",
                    Price = 1780,
                    ImageUrl = ""
                },
                new Product
                {
                    Id = 6,
                    Name = "Nvidia RTX 4070",
                    Description = "An Nvidia RTX 40 series base graphic card",
                    CategoryId = 1,
                    Seller = "ASUS",
                    Price = 690,
                    ImageUrl = ""
                }
            );
        }
    }
}
