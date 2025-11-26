namespace MovieBookingAPI.Models
{
    public class Movie
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int DurationMinutes { get; set; }
        public string PosterUrl { get; set; } = "";

        public List<Showtime> Showtimes { get; set; } = new();
    }
}
