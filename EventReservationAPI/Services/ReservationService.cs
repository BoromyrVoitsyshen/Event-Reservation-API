using EventReservationAPI.Data;
using EventReservationAPI.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace EventReservationAPI.Services
{
    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            AppDbContext context, 
            ILogger<ReservationService> logger
            )
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Reservation> CreateReservationAsync(int userId, ReservationCreateDto dto)
        {
            var eventItem = await _context.Events.FindAsync(dto.EventId);

            if (eventItem == null)
            {
                _logger.LogWarning("Attempted to create a reservation for a non-existent event with ID: {EventId}.", dto.EventId);
                throw new InvalidOperationException("Event not found");
            }

            if (eventItem.AvailableSeats < dto.SeatsCount)
            {
                _logger.LogWarning("Not enough seats to reserve for event {EventId} by user {UserId}. Requested: {RequestedSeats}, Available: {AvailableSeats}", dto.EventId, userId, dto.SeatsCount, eventItem.AvailableSeats);
                throw new InvalidOperationException("Not enough seats to reserve");
            }

            eventItem.AvailableSeats -= dto.SeatsCount;

            var reservation = new Reservation
            {
                EventId = dto.EventId,
                UserId = userId,
                SeatsCount = dto.SeatsCount,
                Status = Reservation.Statuses.Pending,

                ExpiresAt = DateTime.UtcNow.AddSeconds(30)
            };

            _context.Reservations.Add(reservation);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Reservation created for event {EventId} by user {UserId}.", dto.EventId, userId);
                return reservation;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Concurrency conflict when trying to reserve seats for event {EventId} by user {UserId}.", dto.EventId, userId);
                throw new InvalidOperationException("Sorry someone else has already reserved seats for this event.");
            }
        }

        public async Task<Reservation> ConfirmReservationAsync(int userId, int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);

            if (reservation == null)
            {
                _logger.LogWarning("Reservation {ReservationId} not found.", reservationId);
                throw new InvalidOperationException("Reservation not found.");
            }

            if (reservation.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to confirm reservation {ReservationId} belonging to another user.", userId, reservationId);
                throw new InvalidOperationException("Reservation not found.");
            }

            if (reservation.Status != Reservation.Statuses.Pending)
            {
                _logger.LogWarning("Reservation {ReservationId} is already in status {Status}.", reservationId, reservation.Status);
                throw new InvalidOperationException($"Cannot confirm reservation. Current status is {reservation.Status}.");
            }

            if (DateTime.UtcNow > reservation.ExpiresAt)
            {
                _logger.LogWarning("Reservation {ReservationId} expired at {ExpiresAt}.", reservationId, reservation.ExpiresAt);
                reservation.Status = Reservation.Statuses.Expired;

                var eventItem = await _context.Events.FindAsync(reservation.EventId);
                if (eventItem != null)
                {
                    eventItem.AvailableSeats += reservation.SeatsCount;
                }

                await _context.SaveChangesAsync();
                throw new InvalidOperationException("Reservation expired. Seats have been returned to the event.");
            }

            reservation.Status = Reservation.Statuses.Confirmed;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Reservation {ReservationId} confirmed by user {UserId}.", reservationId, userId);

            return reservation;
        }

        public async Task<Reservation> CancelReservationAsync(int userId, int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);

            if (reservation == null)
            {
                _logger.LogWarning("Reservation {ReservationId} not found.", reservationId);
                throw new InvalidOperationException("Reservation not found.");
            }

            if (reservation.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to cancel reservation {ReservationId} belonging to another user.", userId, reservationId);
                throw new InvalidOperationException("Reservation not found.");
            }

            if (reservation.Status == Reservation.Statuses.Cancelled || reservation.Status == Reservation.Statuses.Expired)
            {
                _logger.LogWarning("Reservation {ReservationId} cannot be cancelled because it is already {Status}.", reservationId, reservation.Status);
                throw new InvalidOperationException($"Cannot cancel reservation. It is already {reservation.Status}.");
            }

            var eventItem = await _context.Events.FindAsync(reservation.EventId);
            if (eventItem != null)
            {
                eventItem.AvailableSeats += reservation.SeatsCount;
            }

            reservation.Status = Reservation.Statuses.Cancelled;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reservation {ReservationId} cancelled by user {UserId}. Returned {SeatsCount} seats to Event {EventId}.",
                reservationId, userId, reservation.SeatsCount, reservation.EventId);

            return reservation;
        }
    }
}
