using EventReservationAPI.Data;
using EventReservationAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventReservationAPI.Services
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<EventService> _logger;

        public EventService(AppDbContext dbContext, ILogger<EventService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Event> CreateEventAsync(InputEventDto dto)
        {
            var createdEvent = new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                Location = dto.Location,
                StartsAt = dto.StartsAt,
                Capacity = dto.Capacity,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Events.Add(createdEvent);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Event created successfully with ID: {EventId}", createdEvent.Id);

            return createdEvent;
        }

        public async Task<Event> GetEventAsync(int id)
        {
            var existingEvent = await _dbContext.Events.FindAsync(id);
            if (existingEvent is not null)
            {
                _logger.LogInformation("Event retrieved successfully with ID: {EventId}", id);
                return existingEvent;
            } 

            _logger.LogWarning("Event not found with ID: {EventId}", id);

            return existingEvent;
        }

        public async Task<Event> UpdateEventAsync(int id, InputEventDto dto)
        {
            var updatedEvent = await _dbContext.Events.FindAsync(id);

            if (updatedEvent is not null)
            {
                updatedEvent.Name = dto.Name;
                updatedEvent.Description = dto.Description;
                updatedEvent.Location = dto.Location;
                updatedEvent.StartsAt = dto.StartsAt;
                updatedEvent.Capacity = dto.Capacity;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Event updated successfully with ID: {EventId}", updatedEvent.Id);

                return updatedEvent;
            }

            _logger.LogWarning("Event not found with ID: {EventId}", id);

            return updatedEvent;
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            var existingEvent = await _dbContext.Events.FindAsync(id);
            if (existingEvent is not null)
            {
                _dbContext.Events.Remove(existingEvent);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Event deleted successfully with ID: {EventId}", id);
                return true;
            }

            _logger.LogWarning("Event not found with ID: {EventId}", id);
            return false;
        }

        public async Task<List<Event>> GetEventsAsync(  )
        {
            var events = await _dbContext.Events.ToListAsync();
            _logger.LogInformation("All events retrieved successfully.");
            return events;
        }
    }
}
