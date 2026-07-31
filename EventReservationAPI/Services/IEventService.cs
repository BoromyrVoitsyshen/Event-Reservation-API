using EventReservationAPI.Entities;

namespace EventReservationAPI.Services
{
    public interface IEventService
    {
        Task<Event> CreateEventAsync(EventInputDto dto);
        Task<Event> GetEventAsync(int id);
        Task<Event> UpdateEventAsync(int id, EventInputDto dto);
        Task<bool> DeleteEventAsync(int id);
        Task<List<Event>> GetEventsAsync(EventFilterDto filter);
    }
}
