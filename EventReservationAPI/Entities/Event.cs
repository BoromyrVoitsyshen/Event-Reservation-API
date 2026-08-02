using System.ComponentModel.DataAnnotations;

namespace EventReservationAPI.Entities
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartsAt { get; set; }
        public int Capacity { get; set; }
        public DateTime CreatedAt { get; set; }

        public int AvailableSeats { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
