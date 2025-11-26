using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingAPI.Data;
using MovieBookingAPI.Models;

namespace MovieBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowtimesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ShowtimesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // =========================================
        // GET: /api/showtimes?date=YYYY-MM-DD
        // Lấy danh sách suất chiếu (lọc theo ngày)
        // =========================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? date = null)
        {
            var query = _db.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                    .ThenInclude(r => r.Cinema)
                .AsQueryable();

            if (date.HasValue)
            {
                var d = date.Value.Date;
                var next = d.AddDays(1);
                query = query.Where(s => s.StartTime >= d && s.StartTime < next);
            }

            var list = await query.ToListAsync();
            return Ok(new
            {
                message = "Lấy danh sách suất chiếu thành công!",
                data = list
            });
        }

        // =========================================
        // GET: /api/showtimes/{id}
        // Lấy thông tin 1 suất chiếu
        // =========================================
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var showtime = await _db.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                    .ThenInclude(r => r.Cinema)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (showtime == null)
                return NotFound(new { message = "Không tìm thấy suất chiếu." });

            return Ok(new
            {
                message = "Lấy thông tin suất chiếu thành công!",
                data = showtime
            });
        }

        // =========================================
        // POST: /api/showtimes
        // Tạo suất chiếu mới (Admin)
        // =========================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateShowtimeDto dto)
        {
            var movie = await _db.Movies.FindAsync(dto.MovieId);
            var room = await _db.Rooms.FindAsync(dto.RoomId);

            if (movie == null || room == null)
                return BadRequest(new { message = "Không tìm thấy phim hoặc phòng chiếu." });

            var showtime = new Showtime
            {
                MovieId = dto.MovieId,
                RoomId = dto.RoomId,
                StartTime = dto.StartTime
            };

            _db.Showtimes.Add(showtime);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Tạo suất chiếu thành công!",
                data = showtime
            });
        }

        // =========================================
        // PUT: /api/showtimes/{id}
        // Cập nhật suất chiếu
        // =========================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, CreateShowtimeDto dto)
        {
            var showtime = await _db.Showtimes.FindAsync(id);

            if (showtime == null)
                return NotFound(new { message = "Không tìm thấy suất chiếu để cập nhật." });

            var movie = await _db.Movies.FindAsync(dto.MovieId);
            var room = await _db.Rooms.FindAsync(dto.RoomId);

            if (movie == null || room == null)
                return BadRequest(new { message = "Phim hoặc phòng chiếu không tồn tại." });

            showtime.MovieId = dto.MovieId;
            showtime.RoomId = dto.RoomId;
            showtime.StartTime = dto.StartTime;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật suất chiếu thành công!",
                data = showtime
            });
        }

        // =========================================
        // DELETE: /api/showtimes/{id}
        // Xóa suất chiếu (chỉ khi chưa có đặt vé)
        // =========================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var showtime = await _db.Showtimes
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (showtime == null)
                return NotFound(new { message = "Không tìm thấy suất chiếu để xóa." });

            // Nếu có booking → không cho xóa
            if (showtime.Bookings.Any())
                return BadRequest(new { message = "Không thể xóa suất chiếu vì đã có người đặt vé." });

            _db.Showtimes.Remove(showtime);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Xóa suất chiếu thành công!" });
        }
    }

    // =============================================
    // DTO
    // =============================================
    public class CreateShowtimeDto
    {
        public int MovieId { get; set; }
        public int RoomId { get; set; }
        public DateTime StartTime { get; set; }
    }
}
