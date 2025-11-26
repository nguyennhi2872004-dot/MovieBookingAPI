using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingAPI.Data;
using MovieBookingAPI.Models;
using System.Security.Claims;

namespace MovieBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public BookingsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // =========================================
        // GET: /api/bookings
        // Admin xem tất cả, User chỉ xem của mình
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (userIdStr == null)
                return Unauthorized(new { message = "Bạn chưa đăng nhập." });

            int userId = int.Parse(userIdStr);

            var query = _db.Bookings
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Room)
                        .ThenInclude(r => r.Cinema)
                .Include(b => b.Seats);

            // USER — chỉ xem vé của mình
            if (role != "Admin")
            {
                var raw = await query
                    .Where(b => b.UserId == userId)
                    .Select(b => new
                    {
                        b.Id,
                        b.ShowtimeId,
                        b.Status,
                        Movie = b.Showtime.Movie.Title,
                        Cinema = b.Showtime.Room.Cinema.Name,
                        Room = b.Showtime.Room.Name,
                        Showtime = b.Showtime.StartTime,
                        Seats = b.Seats.ToList()
                    })
                    .ToListAsync();

                var result = raw.Select(b => new
                {
                    bookingId = b.Id,
                    showtimeId = b.ShowtimeId,
                    status = b.Status,
                    movie = b.Movie,
                    cinema = b.Cinema,
                    room = b.Room,
                    showtime = b.Showtime,
                    totalSeats = b.Seats.Count,
                    seats = b.Seats.Select(s => $"{(char)('A' + ((s.Row - 1) % 26))}{s.Number}")
                });

                return Ok(new { message = "Lấy danh sách vé của bạn thành công!", data = result });
            }

            // ADMIN — xem toàn bộ
            var adminRaw = await query
                .Select(b => new
                {
                    b.Id,
                    b.UserId,
                    b.ShowtimeId,
                    b.Status,
                    Movie = b.Showtime.Movie.Title,
                    Cinema = b.Showtime.Room.Cinema.Name,
                    Room = b.Showtime.Room.Name,
                    Showtime = b.Showtime.StartTime,
                    Seats = b.Seats.ToList()
                })
                .ToListAsync();

            var adminResult = adminRaw.Select(b => new
            {
                bookingId = b.Id,
                userId = b.UserId,
                showtimeId = b.ShowtimeId,
                status = b.Status,
                movie = b.Movie,
                cinema = b.Cinema,
                room = b.Room,
                showtime = b.Showtime,
                totalSeats = b.Seats.Count,
                seats = b.Seats.Select(s => $"{(char)('A' + ((s.Row - 1) % 26))}{s.Number}")
            });

            return Ok(new { message = "Lấy danh sách tất cả vé thành công!", data = adminResult });
        }

        // =========================================
        // POST: /api/bookings  (Đặt vé)
        // =========================================
        [HttpPost]
        public async Task<IActionResult> Create(CreateBookingDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null)
                return Unauthorized(new { message = "Bạn chưa đăng nhập." });

            int userId = int.Parse(userIdStr);

            var showtime = await _db.Showtimes
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.Id == dto.ShowtimeId);

            if (showtime == null)
                return BadRequest(new { message = "Suất chiếu không tồn tại." });

            var seats = await _db.Seats
                .Where(s => dto.SeatIds.Contains(s.Id))
                .ToListAsync();

            if (seats.Count != dto.SeatIds.Count)
                return BadRequest(new { message = "Một số ghế không tồn tại." });

            // CHỐNG ĐẶT TRÙNG GHẾ — LẤY DANH SÁCH GHẾ TRÙNG
            var takenSeats = await _db.Bookings
                .Include(b => b.Seats)
                .Where(b =>
                    b.ShowtimeId == dto.ShowtimeId &&
                    b.Status != "Cancelled" &&
                    b.Seats.Any(s => dto.SeatIds.Contains(s.Id)))
                .SelectMany(b => b.Seats)
                .Where(s => dto.SeatIds.Contains(s.Id))
                .ToListAsync();

            if (takenSeats.Any())
            {
                var conflict = takenSeats.Select(s =>
                    $"{(char)('A' + ((s.Row - 1) % 26))}{s.Number}"
                );

                return BadRequest(new
                {
                    message = "Một số ghế đã có người đặt trước.",
                    conflictedSeats = conflict,
                    showtimeId = dto.ShowtimeId
                });
            }


            var booking = new Booking
            {
                UserId = userId,
                ShowtimeId = dto.ShowtimeId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Seats = seats
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            // Load lại booking
            var raw = await _db.Bookings
                .Where(b => b.Id == booking.Id)
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.Showtime).ThenInclude(s => s.Room).ThenInclude(r => r.Cinema)
                .Include(b => b.Seats)
                .Select(b => new
                {
                    b.Id,
                    b.ShowtimeId,
                    b.Status,
                    Movie = b.Showtime.Movie.Title,
                    Cinema = b.Showtime.Room.Cinema.Name,
                    Room = b.Showtime.Room.Name,
                    Showtime = b.Showtime.StartTime,
                    Seats = b.Seats.ToList()
                })
                .FirstAsync();

            var result = new
            {
                bookingId = raw.Id,
                showtimeId = raw.ShowtimeId,
                status = raw.Status,
                movie = raw.Movie,
                cinema = raw.Cinema,
                room = raw.Room,
                showtime = raw.Showtime,
                totalSeats = raw.Seats.Count,
                seats = raw.Seats.Select(s => $"{(char)('A' + ((s.Row - 1) % 26))}{s.Number}")
            };

            return Ok(new { message = "Đặt vé thành công!", data = result });
        }

        // =========================================
        // PUT: /api/bookings/{id}/cancel
        // =========================================
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (userIdStr == null)
                return Unauthorized(new { message = "Bạn chưa đăng nhập." });

            int userId = int.Parse(userIdStr);

            var booking = await _db.Bookings
                .Include(b => b.Showtime)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound(new { message = "Không tìm thấy vé cần hủy." });

            // User thường không được hủy vé của người khác
            if (role != "Admin" && booking.UserId != userId)
                return Forbid("Bạn không có quyền hủy vé này.");

            // Không được hủy sau khi phim bắt đầu
            if (booking.Showtime.StartTime <= DateTime.UtcNow)
                return BadRequest(new { message = "Không thể hủy sau khi phim đã bắt đầu." });

            booking.Status = "Cancelled";
            await _db.SaveChangesAsync();

            return Ok(new { message = "Hủy vé thành công!", bookingId = booking.Id });
        }

        // GET: /api/bookings/my
        // =========================================
        [HttpGet("my")]
        public async Task<IActionResult> MyBookings()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null)
                return Unauthorized(new { message = "Bạn chưa đăng nhập." });

            int userId = int.Parse(userIdStr);

            var raw = await _db.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.Showtime).ThenInclude(s => s.Room).ThenInclude(r => r.Cinema)
                .Include(b => b.Seats)
                .Select(b => new
                {
                    b.Id,
                    b.ShowtimeId,
                    b.Status,
                    Movie = b.Showtime.Movie.Title,
                    Cinema = b.Showtime.Room.Cinema.Name,
                    Room = b.Showtime.Room.Name,
                    Showtime = b.Showtime.StartTime,
                    Seats = b.Seats.ToList()
                })
                .ToListAsync();


            var result = raw.Select(b => new
            {
                bookingId = b.Id,
                showtimeId = b.ShowtimeId,
                status = b.Status,
                movie = b.Movie,
                cinema = b.Cinema,
                room = b.Room,
                showtime = b.Showtime,
                totalSeats = b.Seats.Count,
                seats = b.Seats.Select(s => $"{(char)('A' + ((s.Row - 1) % 26))}{s.Number}")
            });

            return Ok(new { message = "Lấy lịch sử đặt vé thành công!", data = result });
        }

        // =========================================
        // GET: /api/bookings/{id}
        // =========================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (userIdStr == null)
                return Unauthorized(new { message = "Bạn chưa đăng nhập." });

            int userId = int.Parse(userIdStr);

            var booking = await _db.Bookings
                .Include(b => b.Showtime).ThenInclude(s => s.Movie)
                .Include(b => b.Showtime).ThenInclude(s => s.Room).ThenInclude(r => r.Cinema)
                .Include(b => b.Seats)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound(new { message = "Không tìm thấy thông tin vé." });

            if (role != "Admin" && booking.UserId != userId)
                return Forbid("Bạn không có quyền xem vé này.");

            var result = new
            {
                bookingId = booking.Id,
                showtimeId = booking.ShowtimeId,
                status = booking.Status,
                movie = booking.Showtime.Movie.Title,
                cinema = booking.Showtime.Room.Cinema.Name,
                room = booking.Showtime.Room.Name,
                showtime = booking.Showtime.StartTime,
                totalSeats = booking.Seats.Count,
                seats = booking.Seats.Select(s => $"{(char)('A' + ((s.Row - 1) % 26))}{s.Number}")
            };

            return Ok(new { message = "Lấy chi tiết vé thành công!", data = result });
        }


        public class CreateBookingDto
        {
            public int ShowtimeId { get; set; }
            public List<int> SeatIds { get; set; } = new();
        }
    }
}
