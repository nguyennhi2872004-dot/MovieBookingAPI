namespace MovieBookingAPI.Models
{
    public class Room
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public int CinemaId { get; set; }
        public Cinema Cinema { get; set; }

        public List<Seat> Seats { get; set; } = new();
        public List<Showtime> Showtimes { get; set; } = new();
    }
}
