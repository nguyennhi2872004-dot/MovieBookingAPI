using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieBookingAPI.Data;
using MovieBookingAPI.Models;
using MovieBookingAPI.Services;
using System.Security.Cryptography;
using System.Text;

namespace MovieBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly JwtTokenService _jwt;

        public AuthController(ApplicationDbContext db, JwtTokenService jwt)
        {
            _db = db;
            _jwt = jwt;
        }
        
        // ĐĂNG KÝ
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.UserName == dto.UserName))
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại!" });

            var user = new User
            {
                UserName = dto.UserName,
                PasswordHash = Hash(dto.Password),
                Role = "User"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công!" });
        }

        // ĐĂNG NHẬP
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);

            if (user == null || user.PasswordHash != Hash(dto.Password))
                return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu!" });

            var token = _jwt.CreateToken(user);

            return Ok(new
            {
                success = true,
                message = "OK",
                token = token
            });
        }

        // HASH PASSWORD
        private string Hash(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }

    // DTOs
    public class RegisterDto
    {
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginDto
    {
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
