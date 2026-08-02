using EventReservationAPI.Entities;

namespace EventReservationAPI.Services
{
    public interface IReservationService
    {
        Task<Reservation> CreateReservationAsync(int UserId, ReservationCreateDto dto);
        Task<Reservation> ConfirmReservationAsync(int userId, int reservationId);
        Task<Reservation> CancelReservationAsync(int userId, int reservationId);
    }
}
