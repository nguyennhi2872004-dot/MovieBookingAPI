
# 🎬 MovieBookingAPI – Hệ thống Đặt Vé Xem Phim (ASP.NET Core 8 + SQL Server)

Hệ thống API đặt vé xem phim trực tuyến hỗ trợ:
✔ Phân quyền Admin/User  
✔ Tạo phim – rạp – phòng – suất chiếu  
✔ Sinh ghế tự động  
✔ Đặt vé + chống đặt trùng  
✔ Thanh toán mô phỏng  
✔ Thống kê doanh thu dành cho Admin  

---

## 🚀 Công nghệ sử dụng
- ASP.NET Core 8 Web API  
- Entity Framework Core 8  
- SQL Server  
- JWT Authentication  
- Swagger UI  
- Migration Code‑First  

---

## ⚙ Yêu cầu môi trường
- .NET SDK 8 trở lên  
- SQL Server  
- Đã cài EF CLI:
```bash
dotnet tool install --global dotnet-ef
```

---

# 📥 Cách chạy khi tải từ GitHub về

## 1️. Clone dự án
```bash
git clone https://github.com/<your-username>/MovieBookingAPI.git
cd MovieBookingAPI
```

## 2️. Restore packages
```bash
dotnet restore
```

## 3️. Kiểm tra / sửa Connection String
File `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=MovieBookingDB;Trusted_Connection=True;TrustServerCertificate=True"
}
```

## 4️. Tạo Database
Nếu thư mục *Migrations/* đã có:
```bash
dotnet ef database update
```

Nếu chưa có:
```bash
dotnet ef migrations add Init
dotnet ef database update
```

## 5️. Chạy chương trình
```bash
dotnet run
```

Swagger chạy tại:
```
https://localhost:{port}/swagger
```

## 6️⃣ Đăng nhập bằng tài khoản mẫu
| Role  | Username | Password   |
|-------|----------|------------|
| Admin | admin    | Admin@123  |
| User  | user     | User@123   |

---

# 📁 Cấu trúc dự án
```
MovieBookingAPI/
│── Controllers/
│── Models/
│── Data/
│── Seed/
│── Services/
│── DTOs/
│── Program.cs
│── appsettings.json
```

---

# 🔐 1. Authentication – JWT

### Đăng ký  
POST `/api/auth/register`  

### Đăng nhập  
POST `/api/auth/login`

→ Trả về JWT Token → dùng nút **Authorize** trên Swagger.

---

# 🎞 2. Movie Management
GET `/api/movies`  
POST `/api/movies`

Ví dụ thêm phim:
```json
{
  "title": "Inception",
  "duration": 148,
  "description": "A mind-bending thriller",
  "posterUrl": "https://..."
}
```

---

# 🏢 3. Cinema & Room

### Thêm rạp  
POST `/api/cinemas`

### Thêm phòng + tự sinh ghế  
POST `/api/rooms`
```json
{
  "cinemaId": 1,
  "name": "Phòng 1",
  "rows": 5,
  "seatsPerRow": 10
}
```
→ Sinh tự động 50 ghế.

---

# 🕒 4. Tạo suất chiếu
POST `/api/showtimes`
```json
{
  "movieId": 1,
  "roomId": 1,
  "startTime": "2026-01-01T19:00:00"
}
```

---

# 💺 5. Trạng thái ghế theo suất
GET `/api/ShowtimeSeats/{showtimeId}`  
→ Xác định ghế trống / đã đặt.

---

# 🎫 6. Đặt vé
POST `/api/bookings`  
```json
{
  "showtimeId": 1,
  "seatIds": [1, 2]
}
```
✔ Kiểm tra ghế trùng  
✔ Tạo booking status = Pending  

---

# ❌ 7. Hủy vé
PUT `/api/bookings/{id}/cancel`  
✔ Hủy trước giờ chiếu  
❌ Không hủy nếu suất đã diễn ra  

---

# 💳 8. Thanh toán mô phỏng
POST `/api/payments/mock`
```json
{
  "bookingId": 1
}
```
→ Booking = Confirmed  
→ Payment = Success  

---

# 📊 9. API Thống kê (Admin)

- GET `/api/admin/stats/bookings-summary`
- GET `/api/admin/stats/bookings-by-date`
- GET `/api/admin/stats/top-movies`
- GET `/api/admin/stats/revenue-by-movie`
- GET `/api/admin/stats/revenue-by-date`

---

# 🧠 10. Hướng phát triển
- Thêm giao diện React/Next.js  
- Tích hợp VNPay/MoMo  
- QR Code check-in  
- Email xác nhận vé  
- Dashboard real-time  

---

# 🎉 Kết luận
MovieBookingAPI là hệ thống đặt vé hoàn chỉnh, đáp ứng đầy đủ:  
✔ Auth  
✔ Quản lý phim/rạp/phòng/suất  
✔ Ghế tự sinh  
✔ Đặt – Hủy – Thanh toán  
✔ Thống kê Admin  
✔ Dễ mở rộng và triển khai thực tế  
