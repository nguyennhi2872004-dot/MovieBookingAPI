public class BookingDetailDto
{
    public int Id { get; set; }
    public UserDto User { get; set; }
    public ShowtimeDto Showtime { get; set; }
    public List<SeatDto> Seats { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; }
}

public class ShowtimeDto
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public MovieDto Movie { get; set; }
    public RoomDto Room { get; set; }
}

public class MovieDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int DurationMinutes { get; set; }
}

public class RoomDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public CinemaDto Cinema { get; set; }
}

public class CinemaDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class SeatDto
{
    public int Id { get; set; }
    public int Row { get; set; }
    public int Number { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
}
