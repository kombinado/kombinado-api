using Kombinado.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kombinado.Api.Data
{
    public class KombinadoDbContext : DbContext
    {
        public KombinadoDbContext(DbContextOptions<KombinadoDbContext> options) : base(options) 
        { 

        }

        // DbSets for each entity
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RideEntity> Rides { get; set; }
        public DbSet<RideRequestEntity> RideRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. UserEntity configuration
            modelBuilder.Entity<UserEntity>(e =>
            {
                e.HasKey(u => u.Id);
                e.HasIndex(u => u.Email).IsUnique(); // Prevents duplicate emails
                e.Property(u => u.Email).IsRequired().HasMaxLength(150);
                e.Property(u => u.Name).IsRequired().HasMaxLength(100);
                e.Property(u => u.PasswordHash).IsRequired();
            });

            // 2. RideEntity configuration
            modelBuilder.Entity<RideEntity>(e =>
            {
                e.HasKey(r => r.Id);
                e.Property(r => r.Origin).IsRequired().HasMaxLength(200);
                e.Property(r => r.Destination).IsRequired().HasMaxLength(200);
                e.Property(r => r.Status).IsRequired().HasMaxLength(50);

                // Relationship: A ride has ONE driver 
                e.HasOne(r => r.Driver)
                 .WithMany(u => u.RidesOffered)
                 .HasForeignKey(r => r.DriverId)
                 .OnDelete(DeleteBehavior.Restrict); // Prevents deleting a user if they have offered rides
            });

            // 3. RideRequestEntity configuration
            modelBuilder.Entity<RideRequestEntity>(e =>
            {
                e.HasKey(req => req.Id);
                e.Property(req => req.MeetingPointSuggestion).HasMaxLength(250);
                e.Property(req => req.Status).IsRequired().HasMaxLength(50);

                // Relationship: Request -> Ride
                e.HasOne(req => req.Ride)
                 .WithMany(r => r.Requests)
                 .HasForeignKey(req => req.RideId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Relationship: Request -> Passenger
                e.HasOne(req => req.Passenger)
                 .WithMany(u => u.RequestsMade)
                 .HasForeignKey(req => req.PassengerId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
