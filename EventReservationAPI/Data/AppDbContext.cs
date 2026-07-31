using Microsoft.EntityFrameworkCore;
using EventReservationAPI.Entities;
using Microsoft.EntityFrameworkCore.Internal;

namespace EventReservationAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Event> Events { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .HasIndex(e => e.StartsAt);
            modelBuilder.Entity<Event>()
                .HasIndex(e => e.Name);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
