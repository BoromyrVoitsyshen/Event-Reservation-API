using Microsoft.EntityFrameworkCore;

namespace EventReservationAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Entities.Event> Events { get; set; }
    }
}
