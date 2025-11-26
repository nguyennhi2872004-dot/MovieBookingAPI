using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingAPI.Data;

namespace MovieBookingAPI.Controllers
{
    [ApiController]
    [Route("api/movies")]
    public class MovieFilterController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public MovieFilterController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /api/movies/available?date=2026-01-01
        //lấy danh sách phim chiếu trong ngày
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableMovies([FromQuery] DateTime date)
        {
            var movies = await _db.Showtimes
                .Where(s => s.StartTime.Date == date.Date)
                .Include(s => s.Movie)
                .Select(s => s.Movie)
                .Distinct()
                .ToListAsync();

            return Ok(new
            {
                message = $"Lấy danh sách phim chiếu ngày {date:dd/MM/yyyy} thành công!",
                data = movies
            });
        }
    }
}
