using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Data;
using DungeonExplorerBackend.Models.AuthLayer;
using Microsoft.EntityFrameworkCore;

namespace DungeonExplorerBackend.Services
    {
    public class AdminSeeder : IDataSeeder
        {
        private readonly DungeonContext _context;

        public AdminSeeder(DungeonContext context)
            {
            _context = context;
            }

        public async Task SeedAsync()
            {
            if (await _context.Users.AnyAsync())
                return;

            var username = Environment.GetEnvironmentVariable("ADMIN_USERNAME");
            var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new Exception("Admin credentials missing in environment variables.");

            _context.Users.Add(new AppUser
                {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "Admin"
                });

            await _context.SaveChangesAsync();
            }
        }
    }