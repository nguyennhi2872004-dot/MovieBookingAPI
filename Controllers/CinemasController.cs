using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingAPI.Data;
using MovieBookingAPI.Models;

namespace MovieBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CinemasController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CinemasController(ApplicationDbContext db)
        {
            _db = db;
        }

        // =========================================
        // GET: /api/cinemas
        // Lấy danh sách rạp chiếu
        // =========================================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var cinemas = await _db.Cinemas
                .Include(c => c.Rooms)
                .ToListAsync();

            return Ok(new
            {
                message = "Lấy danh sách rạp chiếu phim thành công!",
                data = cinemas
            });
        }

        // =========================================
        // GET: /api/cinemas/{id}
        // Lấy chi tiết 1 rạp
        // =========================================
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var cinema = await _db.Cinemas
                .Include(c => c.Rooms)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cinema == null)
                return NotFound(new { message = "Không tìm thấy rạp chiếu phim." });

            return Ok(new
            {
                message = "Lấy thông tin rạp chiếu phim thành công!",
                data = cinema
            });
        }

        // =========================================
        // POST: /api/cinemas
        // Tạo rạp chiếu mới
        // =========================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CinemaDto dto)
        {
            var cinema = new Cinema
            {
                Name = dto.Name,
                Address = dto.Address
            };

            _db.Cinemas.Add(cinema);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm rạp chiếu phim thành công!",
                data = cinema
            });
        }

        // =========================================
        // PUT: /api/cinemas/{id}
        // Cập nhật rạp chiếu
        // =========================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, CinemaDto dto)
        {
            var cinema = await _db.Cinemas.FindAsync(id);

            if (cinema == null)
                return NotFound(new { message = "Không tìm thấy rạp để cập nhật." });

            cinema.Name = dto.Name;
            cinema.Address = dto.Address;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật thông tin rạp chiếu phim thành công!",
                data = cinema
            });
        }

        // =========================================
        // DELETE: /api/cinemas/{id}
        // Xóa rạp chiếu (chỉ khi không có phòng)
        // =========================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _db.Cinemas
                .Include(c => c.Rooms)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cinema == null)
                return NotFound(new { message = "Không tìm thấy rạp để xóa." });

            // Nếu rạp còn phòng chiếu => không cho xóa
            if (cinema.Rooms != null && cinema.Rooms.Count > 0)
            {
                return BadRequest(new
                {
                    message = "Không thể xóa rạp vì vẫn còn phòng chiếu bên trong."
                });
            }

            _db.Cinemas.Remove(cinema);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Xóa rạp chiếu phim thành công!" });
        }
    }

    public class CinemaDto
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
    }
}
