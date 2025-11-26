using Microsoft.EntityFrameworkCore;
using MovieBookingAPI.Data;
using MovieBookingAPI.Models;

namespace MovieBookingAPI.Services
{
    public class PaymentService
    {
        private readonly ApplicationDbContext _db;

        public PaymentService(ApplicationDbContext db)
        {
            _db = db;
        }

        // Lấy booking kèm các navigation
        public async Task<Booking?> GetBooking(int bookingId)
        {
            return await _db.Bookings
                .Include(b => b.Seats)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Movie)
                .Include(b => b.Showtime)
                    .ThenInclude(s => s.Room)
                        .ThenInclude(r => r.Cinema)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        // Xử lý thanh toán mock
        public async Task<bool> ProcessMockPayment(int bookingId, string method)
        {
            var booking = await _db.Bookings.FindAsync(bookingId);
            if (booking == null)
                return false;

            // Không cho thanh toán lại vé đã Paid
            if (booking.Status == "Paid")
                return false;

            // Cập nhật trạng thái vé
            booking.Status = "Paid";

            // Tính tổng tiền (tuỳ bạn muốn thay đổi)
            decimal totalAmount = booking.Seats.Count * 75000; 

            // Tạo payment record
            _db.Payments.Add(new Payment
            {
                BookingId = bookingId,
                Method = method,
                Amount = totalAmount,
                Status = "Success",
                PaidAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
