using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieBookingAPI.Services;
using System.Security.Claims;

namespace MovieBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentsController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // POST: /api/payments/mock
        [HttpPost("mock")]
        public async Task<IActionResult> MockPay(MockPaymentDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Lấy booking từ service
            var booking = await _paymentService.GetBooking(dto.BookingId);
            if (booking == null)
                return BadRequest(new { message = "Không tìm thấy vé đặt để thanh toán." });

            // Chỉ cho thanh toán đúng vé của mình
            if (booking.UserId != userId)
                return Forbid();

            // Không cho thanh toán lại
            if (booking.Status == "Paid")
                return BadRequest(new { message = "Vé này đã thanh toán rồi." });

            // Xử lý thanh toán
            var ok = await _paymentService.ProcessMockPayment(dto.BookingId, dto.Method);
            if (!ok)
                return BadRequest(new { message = "Thanh toán thất bại." });

            // Trả kết quả chi tiết
            return Ok(new
            {
                message = "Thanh toán thành công!",
                bookingId = booking.Id,
                movie = booking.Showtime.Movie.Title,
                cinema = booking.Showtime.Room.Cinema.Name,
                room = booking.Showtime.Room.Name,
                seats = booking.Seats.Select(s => $"{(char)('A' + s.Row - 1)}{s.Number}"),
                paidAt = DateTime.UtcNow,
                method = dto.Method,
                status = "Paid"
            });
        }
    }

    public class MockPaymentDto
    {
        public int BookingId { get; set; }
        public string Method { get; set; } = "MockGateway";
    }
}
