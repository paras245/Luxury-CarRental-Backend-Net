using Microsoft.EntityFrameworkCore;
using CarRental_Backend_Net.Models;

namespace CarRental_Backend_Net.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<User> Users { get; set; } // Keeping User for future proofing even if Auth is hardcoded

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Vehicle>().ToTable("Vehicles");
            modelBuilder.Entity<User>().ToTable("Users");

            // Relationships
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Vehicles)
                .WithOne(v => v.Category)
                .HasForeignKey(v => v.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
