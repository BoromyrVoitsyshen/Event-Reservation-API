using EventReservationAPI.Data;
using Microsoft.EntityFrameworkCore;
using EventReservationAPI.Entities;

namespace EventReservationAPI.Services
{
    public class ReservationCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceProvider;
        private readonly ILogger<ReservationCleanupService> _logger;

        public ReservationCleanupService(
            IServiceScopeFactory serviceProvider,
            ILogger<ReservationCleanupService> logger
            )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reservation cleanup service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredReservationAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while cleaning up reservations.");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task CleanupExpiredReservationAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var expiredReservations = await context.Reservations
                .Include(r => r.Event)
                .Where(r => r.Status == Reservation.Statuses.Pending && r.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(stoppingToken);

            if (expiredReservations.Any())
            {
                foreach (var reservation in expiredReservations)
                {
                    reservation.Status = Reservation.Statuses.Expired;

                    if(reservation.Event != null)
                    {
                        reservation.Event.AvailableSeats += reservation.SeatsCount;
                    }

                    _logger.LogInformation("Auto-expired reservation {ReservationId} and returned {SeatsCount} seats to Event {EventId}.",
                                            reservation.Id, reservation.SeatsCount, reservation.EventId);
                }

                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
