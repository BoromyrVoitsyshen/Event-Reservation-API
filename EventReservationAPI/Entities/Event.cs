namespace EventReservationAPI.Entities
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartsAt { get; set; }
        public int Capacity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
