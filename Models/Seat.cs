namespace MovieBookingAPI.Models
{
    public class Seat
    {
        public int Id { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        public int Row { get; set; }
        public int Number { get; set; }

        public List<Booking> Bookings { get; set; } = new();
    }
}
