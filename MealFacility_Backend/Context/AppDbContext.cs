using MealFacility_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace MealFacility_Backend.Context
{
    public class AppDbContext : DbContext
    { 
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { 

        }

        public DbSet<User> Users { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<Coupon> Coupons { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");

            modelBuilder.Entity<Booking>().ToTable("bookings");

            modelBuilder.Entity<Coupon>().ToTable("coupon");

            modelBuilder.Entity<Notification>().ToTable("notification");
        }
    }
}
