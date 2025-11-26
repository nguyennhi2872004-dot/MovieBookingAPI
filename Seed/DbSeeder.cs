using Microsoft.EntityFrameworkCore;
using MovieBookingAPI.Data;
using MovieBookingAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace MovieBookingAPI.Seed
{
    public class DbSeeder
    {
        private readonly ApplicationDbContext _db;

        public DbSeeder(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task SeedAsync()
        {
            await _db.Database.MigrateAsync();

            if (!_db.Users.Any())
            {
                _db.Users.Add(new User
                {
                    UserName = "admin",
                    PasswordHash = Hash("Admin@123"),
                    Role = "Admin"
                });

                _db.Users.Add(new User
                {
                    UserName = "user",
                    PasswordHash = Hash("User@123"),
                    Role = "User"
                });

                await _db.SaveChangesAsync();
            }
        }

        private string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToHexString(sha.ComputeHash(bytes));
        }
    }
}
