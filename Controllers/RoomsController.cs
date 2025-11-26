using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingAPI.Data;
using MovieBookingAPI.Models;

namespace MovieBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public RoomsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // =========================================
        // GET: /api/rooms – Lấy tất cả phòng
        // =========================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var rooms = await _db.Rooms
                .Include(r => r.Cinema)
                .Include(r => r.Seats)
                .ToListAsync();

            return Ok(new
            {
                message = "Lấy danh sách phòng chiếu thành công!",
                data = rooms
            });
        }

        // =========================================
        // GET: /api/rooms/{id} – Lấy chi tiết phòng
        // =========================================
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var room = await _db.Rooms
                .Include(r => r.Cinema)
                .Include(r => r.Seats)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng chiếu." });

            return Ok(new
            {
                message = "Lấy thông tin phòng chiếu thành công!",
                data = room
            });
        }

        // =========================================
        // POST: /api/rooms – Tạo phòng + ghế
        // =========================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateRoomDto dto)
        {
            var cinema = await _db.Cinemas.FindAsync(dto.CinemaId);
            if (cinema == null)
                return BadRequest(new { message = "Không tìm thấy rạp chiếu phim." });

            var room = new Room
            {
                Name = dto.Name,
                CinemaId = dto.CinemaId
            };

            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();

            // ===== Tạo ghế tự động =====
            var seats = new List<Seat>();
            for (int row = 1; row <= dto.Rows; row++)
            {
                for (int number = 1; number <= dto.SeatsPerRow; number++)
                {
                    seats.Add(new Seat
                    {
                        RoomId = room.Id,
                        Row = row,
                        Number = number
                    });
                }
            }

            _db.Seats.AddRange(seats);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Tạo phòng chiếu và tạo ghế tự động thành công!",
                data = room,
                totalSeats = seats.Count
            });
        }

        // =========================================
        // PUT: /api/rooms/{id} – Cập nhật phòng chiếu
        // =========================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, UpdateRoomDto dto)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng chiếu để cập nhật." });

            room.Name = dto.Name ?? room.Name;
            room.CinemaId = dto.CinemaId;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật phòng chiếu thành công!",
                data = room
            });
        }

        // =========================================
        // DELETE: /api/rooms/{id} – Xóa phòng chiếu
        // =========================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _db.Rooms
                .Include(r => r.Seats)
                .Include(r => r.Showtimes)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
                return NotFound(new { message = "Không tìm thấy phòng chiếu để xóa." });

            // Nếu phòng có suất chiếu => không cho xóa
            if (room.Showtimes.Any())
                return BadRequest(new { message = "Không thể xóa phòng vì đang có suất chiếu sử dụng." });

            // Xóa ghế trước
            if (room.Seats.Any())
                _db.Seats.RemoveRange(room.Seats);

            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Xóa phòng chiếu thành công!" });
        }
    }

    // =========================================
    // DTO Models
    // =========================================
    public class CreateRoomDto
    {
        public string Name { get; set; } = "";
        public int CinemaId { get; set; }
        public int Rows { get; set; }
        public int SeatsPerRow { get; set; }
    }

    public class UpdateRoomDto
    {
        public string? Name { get; set; }
        public int CinemaId { get; set; }
    }
}
