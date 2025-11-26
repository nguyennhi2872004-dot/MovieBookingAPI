# 🎬 MovieBookingAPI – Hệ thống Đặt Vé Xem Phim (ASP.NET Core 8 + SQL Server)

Hệ thống API đặt vé xem phim trực tuyến, hỗ trợ **phân quyền Admin/User**, **đặt ghế**, **chống đặt trùng**, **thanh toán mô phỏng**, và **thống kê dành cho Admin**.

---

## 🚀 Công nghệ sử dụng

- **ASP.NET Core 8 Web API**
- **Entity Framework Core 8**
- **SQL Server**
- **JWT Authentication**
- **Swagger UI**
- **LINQ + EF Query**
- **Migration Code-First**

---

## 🧰 Yêu cầu môi trường

- .NET SDK 8 trở lên  
- SQL Server (local hoặc Docker)  
- Entity Framework CLI:

```bash
dotnet tool install --global dotnet-ef
```

---

## 📦 Cài đặt dự án

### 1️⃣ Restore packages
```bash
dotnet restore
```

### 2️⃣ Tạo Database
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3️⃣ Chạy ứng dụng
```bash
dotnet run
```

Ứng dụng chạy tại:

👉 https://localhost:{port}/swagger

---

## 👤 Tài khoản mặc định

| Role  | Username | Password   |
|-------|----------|------------|
| Admin | admin    | Admin@123  |
| User  | user     | User@123   |

---

## 📁 Cấu trúc dự án

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
`POST /api/auth/register`

### Đăng nhập
`POST /api/auth/login`

→ Trả về JWT Token  
→ Dùng nút **Authorize** trong Swagger để đăng nhập.

---

# 🎞 2. Movie Management (Admin)

### Xem danh sách phim
GET `/api/movies`

### Thêm phim
POST `/api/movies`

```json
{
  "title": "Avengers Endgame",
  "duration": 180
}
```

---

# 🏢 3. Cinema & Room Management

### Thêm rạp
POST `/api/cinemas`

### Thêm phòng chiếu (auto generate seat)
POST `/api/rooms`

```json
{
  "cinemaId": 1,
  "name": "Phòng 1",
  "rows": 5,
  "columns": 10
}
```

→ Hệ thống tự tạo 50 ghế (A1 → E10)

---

# 🕒 4. Tạo suất chiếu

POST `/api/showtimes`

```json
{
  "movieId": 1,
  "roomId": 1,
  "startTime": "2025-01-01T18:00:00"
}
```

---

# 💺 5. Hiển thị ghế theo suất chiếu

GET `/api/ShowtimeSeats/{showtimeId}`

- Ghế booked → màu đỏ  
- Ghế trống → màu xanh  
- API chống đặt trùng

---

# 🎫 6. Đặt vé (User)

POST `/api/bookings`

```json
{
  "showtimeId": 1,
  "seatIds": [1,2]
}
```

→ Booking status: **Pending**

---

# 💳 7. Thanh toán mô phỏng

POST `/api/payments/mock`

```json
{
  "bookingId": 1,
  "method": "MockGateway"
}
```

Kết quả:

- Booking → Confirmed  
- Payment → Success  
- Amount → auto generate  
- PaidAt → saved  

---

# ❌ 8. Hủy vé

PUT `/api/bookings/{id}/cancel`

- Trước giờ chiếu → Hủy được  
- Sau giờ chiếu → Không cho hủy  

---

# 📊 9. API Thống kê (Admin)

### 9.1 Tổng quan booking  
GET `/api/admin/stats/bookings-summary`

### 9.2 Booking theo ngày  
GET `/api/admin/stats/bookings-by-date`

### 9.3 Top phim bán chạy  
GET `/api/admin/stats/top-movies`

### 9.4 Doanh thu theo phim  
GET `/api/admin/stats/revenue-by-movie`

### 9.5 Doanh thu theo ngày  
GET `/api/admin/stats/revenue-by-date`

---

# 🧪 10. Kịch bản demo gợi ý

1. Đăng nhập Admin → lấy token  
2. Thêm phim  
3. Thêm rạp  
4. Thêm phòng chiếu (ghế auto-generate)  
5. Tạo suất chiếu  
6. Đăng nhập User → đặt vé → chống trùng ghế  
7. Thanh toán mock → booking confirmed  
8. Hủy vé  
9. Admin xem thống kê  

---

# 🧠 11. Hướng phát triển

- Giao diện frontend React/Next.js  
- Tích hợp VNPay/Momo  
- Gửi email xác nhận vé  
- QR Code check-in  
- Dashboard real-time  

---

# 🎉 Kết luận

MovieBookingAPI là hệ thống đặt vé hoàn chỉnh:

✔ JWT Auth  
✔ Movie / Cinema / Room / Showtime  
✔ Generate Seats  
✔ Booking chống trùng  
✔ Payment mock  
✔ Admin Statistics  
✔ Dễ mở rộng & triển khai thực tế
