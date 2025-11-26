using System;

namespace MovieBookingAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public string Method { get; set; } = string.Empty;

        public string Status { get; set; } = "Success";

        public decimal Amount { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    }
}
