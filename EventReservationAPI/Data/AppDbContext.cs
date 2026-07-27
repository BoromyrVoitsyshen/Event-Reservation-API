using Microsoft.EntityFrameworkCore;
using EventReservationAPI.Entities;

namespace EventReservationAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Entities.Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .HasIndex(e => e.StartsAt);

            modelBuilder.Entity<Event>()
                .HasIndex(e => e.Name);

            base.OnModelCreating(modelBuilder);
        }
    }
}
