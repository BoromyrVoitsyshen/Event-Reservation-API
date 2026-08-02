namespace EventReservationAPI.Entities
{
    public class ReservationCreateDto
    {
        public int EventId { get; set; }
        public int SeatsCount { get; set; }
    }
}
