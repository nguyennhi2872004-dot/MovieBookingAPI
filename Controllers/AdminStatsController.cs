using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingAPI.Data;

namespace MovieBookingAPI.Controllers
{
    [ApiController]
    [Route("api/admin/stats")]
    [Authorize(Roles = "Admin")] 
    public class AdminStatsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AdminStatsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // 1️. Thống kê tổng số lượng booking: tổng / chờ xử lý / đã hủy / đã xác nhận
        [HttpGet("bookings-summary")]
        public async Task<IActionResult> GetBookingSummary()
        {
            var total = await _db.Bookings.CountAsync();
            var pending = await _db.Bookings.CountAsync(b => b.Status == "Pending");
            var cancelled = await _db.Bookings.CountAsync(b => b.Status == "Cancelled");
            var confirmed = await _db.Bookings.CountAsync(b => b.Status == "Confirmed");

            return Ok(new
            {
                message = "Lấy thống kê tổng quan đặt vé thành công!",
                total,
                pending,
                cancelled,
                confirmed
            });
        }

        // 2️. Thống kê số lượng booking theo ngày tạo
        [HttpGet("bookings-by-date")]
        public async Task<IActionResult> GetBookingsByDate()
        {
            var result = await _db.Bookings
                .GroupBy(b => b.CreatedAt.Date)
                .Select(g => new
                {
                    date = g.Key,
                    count = g.Count()
                })
                .OrderBy(x => x.date)
                .ToListAsync();

            return Ok(new
            {
                message = "Lấy thống kê số lượng đặt vé theo ngày thành công!",
                data = result
            });
        }

        //3️. Top phim bán được nhiều vé nhất
        [HttpGet("top-movies")]
        public async Task<IActionResult> GetTopMovies()
        {
            var result = await _db.Bookings
                .Where(b => b.Status != "Cancelled")
                .Include(b => b.Showtime)
                .ThenInclude(s => s.Movie)
                .Select(b => new
                {
                    movie = b.Showtime.Movie.Title,
                    seats = b.Seats.Count
                })
                .GroupBy(x => x.movie)
                .Select(g => new
                {
                    movie = g.Key,
                    totalTickets = g.Sum(x => x.seats)
                })
                .OrderByDescending(x => x.totalTickets)
                .ToListAsync();

            return Ok(new
            {
                message = "Lấy top phim theo số vé bán ra thành công!",
                data = result
            });
        }

        // 4️. Doanh thu theo từng phim
        [HttpGet("revenue-by-movie")]
        public async Task<IActionResult> GetRevenueByMovie()
        {
            var result = await _db.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Showtime)
                .ThenInclude(s => s.Movie)
                .Where(p => p.Status == "Success")
                .GroupBy(p => p.Booking.Showtime.Movie.Title)
                .Select(g => new
                {
                    movie = g.Key,
                    revenue = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.revenue)
                .ToListAsync();

            return Ok(new
            {
                message = "Lấy doanh thu theo phim thành công!",
                data = result
            });
        }

        // 5️. Doanh thu theo ngày
        [HttpGet("revenue-by-date")]
        public async Task<IActionResult> GetRevenueByDate()
        {
            var result = await _db.Payments
                .Where(p => p.Status == "Success")
                .GroupBy(p => p.PaidAt.Date)    
                .Select(g => new
                {
                    date = g.Key,
                    revenue = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.date)
                .ToListAsync();

            return Ok(new
            {
                message = "Lấy doanh thu theo ngày thành công!",
                data = result
            });
        }
    }
}
