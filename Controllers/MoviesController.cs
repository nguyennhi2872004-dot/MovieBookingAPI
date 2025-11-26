using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingAPI.Data;
using MovieBookingAPI.Models;

namespace MovieBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public MoviesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /api/movies
        // Lấy danh sách phim
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var movies = await _db.Movies.ToListAsync();
            return Ok(new
            {
                message = "Lấy danh sách phim thành công!",
                data = movies
            });
        }

        // GET: /api/movies/{id}
        // Lấy chi tiết phim theo ID
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var movie = await _db.Movies.FindAsync(id);
            if (movie == null) return NotFound(new { message = "Không tìm thấy phim." });
            return Ok(new
            {
                message = "Lấy thông tin phim thành công!",
                data = movie
            });
        }

        // POST: /api/movies
        // Thêm phim mới (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(MovieDto dto)
        {
            var movie = new Movie
            {
                Title = dto.Title,
                Description = dto.Description,
                DurationMinutes = dto.DurationMinutes,
                PosterUrl = dto.PosterUrl
            };

            _db.Movies.Add(movie);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = movie.Id }, new
            {
                message = "Thêm phim mới thành công!",
                data = movie
            });
        }

        // PUT: /api/movies/{id}
        // Cập nhật thông tin phim (Admin)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, MovieDto dto)
        {
            var movie = await _db.Movies.FindAsync(id);
            if (movie == null) return NotFound(new { message = "Không tìm thấy phim cần cập nhật." });

            movie.Title = dto.Title;
            movie.Description = dto.Description;
            movie.DurationMinutes = dto.DurationMinutes;
            movie.PosterUrl = dto.PosterUrl;

            await _db.SaveChangesAsync();
            return Ok(new
            {
                message = "Cập nhật thông tin phim thành công!",
                data = movie
            });
        }

        // DELETE: /api/movies/{id}
        // Xóa phim (Admin)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _db.Movies.FindAsync(id);
            if (movie == null) return NotFound(new { message = "Không tìm thấy phim để xóa." });

            _db.Movies.Remove(movie);
            await _db.SaveChangesAsync();
            return Ok(new
            {
                message = "Xóa phim thành công!"
            });
        }
    }

    public class MovieDto
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int DurationMinutes { get; set; }
        public string PosterUrl { get; set; } = "";
    }
}
