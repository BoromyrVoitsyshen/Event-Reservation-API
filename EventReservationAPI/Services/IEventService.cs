using EventReservationAPI.Entities;

namespace EventReservationAPI.Services
{
    public interface IEventService
    {
        Task<Event> CreateEventAsync(InputEventDto dto);
        Task<Event> GetEventAsync(int id);
        Task<Event> UpdateEventAsync(int id, InputEventDto dto);
        Task<bool> DeleteEventAsync(int id);
        Task<List<Event>> GetEventsAsync(FilterEventDto filter);
    }
}
