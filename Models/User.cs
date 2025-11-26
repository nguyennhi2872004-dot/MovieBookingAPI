namespace MovieBookingAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Role { get; set; } = "User"; // User / Admin

        public List<Booking> Bookings { get; set; } = new();
    }
}
