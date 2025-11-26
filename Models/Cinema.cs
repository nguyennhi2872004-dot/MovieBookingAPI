namespace MovieBookingAPI.Models
{
    public class Cinema
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public string Address { get; set; } = "";

        public List<Room> Rooms { get; set; } = new();
    }
}
