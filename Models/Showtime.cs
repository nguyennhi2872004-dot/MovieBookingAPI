namespace MovieBookingAPI.Models
{
    public class Showtime
    {
        public int Id { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        public DateTime StartTime { get; set; }

        public List<Booking> Bookings { get; set; } = new();
    }
}
