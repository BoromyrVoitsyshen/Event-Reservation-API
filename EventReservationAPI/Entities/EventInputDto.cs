namespace EventReservationAPI.Entities
{
    public class EventInputDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartsAt { get; set; }
        public int Capacity { get; set; }
    }
}
