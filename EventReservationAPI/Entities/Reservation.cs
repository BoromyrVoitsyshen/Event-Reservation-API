namespace EventReservationAPI.Entities
{
    public class Reservation
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public Event? Event { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int SeatsCount { get; set; }

        public string Status { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }

        public class Statuses
        {
            public const string Pending = "Pending";
            public const string Confirmed = "Confirmed";
            public const string Expired = "Expired";
            public const string Cancelled = "Cancelled";
        }
    }
}
