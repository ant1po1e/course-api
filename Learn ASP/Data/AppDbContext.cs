using Learn_ASP.Models;
using Microsoft.EntityFrameworkCore;

namespace Learn_ASP.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Course> Courses { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Purchase> Purchases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Modules)
                .WithOne(m => m.Course)
                .HasForeignKey(m => m.Id);
        }
    }
}
