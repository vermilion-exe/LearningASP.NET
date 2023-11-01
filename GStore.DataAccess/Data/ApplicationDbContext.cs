using GStore.Models;
using Microsoft.EntityFrameworkCore;

namespace GStore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id=1, DisplayOrder=1, Name="Graphic Cards"},
                new Category { Id=2, DisplayOrder=2, Name="Processors"}
                );
        }
    }
}
