using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingAPI.Data;

namespace MovieBookingAPI.Controllers
{
    [ApiController]
    [Route("api/showtimes")]
    public class ShowtimeSeatsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ShowtimeSeatsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /api/showtimes/{id}/seats
        [HttpGet("{id}/seats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSeats(int id)
        {
            var showtime = await _db.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                    .ThenInclude(r => r.Cinema)
                .Include(s => s.Room)
                    .ThenInclude(r => r.Seats)
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.Seats)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (showtime == null)
                return NotFound(new { message = "Không tìm thấy suất chiếu." });

            // Không cho đặt nếu suất chiếu đã diễn ra
            if (showtime.StartTime <= DateTime.UtcNow)
                return BadRequest(new { message = "Suất chiếu đã bắt đầu, không thể chọn ghế." });

            // Lấy danh sách ghế đã được đặt (trừ vé Cancelled)
            var bookedSeatIds = showtime.Bookings
                .Where(b => b.Status != "Cancelled")
                .SelectMany(b => b.Seats)
                .Select(s => s.Id)
                .ToHashSet();

            // Ghế + trạng thái
            var seats = showtime.Room.Seats
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Number)
                .Select(seat => new
                {
                    seatId = seat.Id,
                    row = seat.Row,
                    number = seat.Number,
                    label = $"{(char)('A' + seat.Row - 1)}{seat.Number}", // Ví dụ: A1, C5
                    isBooked = bookedSeatIds.Contains(seat.Id)
                })
                .ToList();

            return Ok(new
            {
                message = "Lấy danh sách ghế của suất chiếu thành công!",
                showtimeId = showtime.Id,
                movie = showtime.Movie.Title,
                cinema = showtime.Room.Cinema.Name,
                room = showtime.Room.Name,
                startTime = showtime.StartTime,
                totalSeats = seats.Count,
                seats = seats
            });
        }
    }
}
