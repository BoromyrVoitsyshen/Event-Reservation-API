namespace EventReservationAPI.Entities
{
    public class FilterEventDto
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}
