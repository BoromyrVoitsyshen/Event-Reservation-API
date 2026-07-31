namespace EventReservationAPI.Settings
{
    public class DatabaseSettings
    {
        public int MaxRetryCount { get; set; } = 3;
        public int MaxDelayInSeconds { get; set; } = 30;
    }
}
