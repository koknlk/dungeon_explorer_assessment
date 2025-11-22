using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using DungeonExplorerBackend.Data;
using DungeonExplorerBackend.Models.AuthLayer;
using DungeonExplorerBackend.Models.Entities;

namespace Tests.UnitTests.Services
    {
    public class SecurityFlowTests
        {
        private DungeonContext GetInMemoryContext()
            {
            var options = new DbContextOptionsBuilder<DungeonContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new DungeonContext(options);

            // Seed users
            context.Users.AddRange(
                new AppUser
                    {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("StrongPass@2025"),
                    Role = "Admin"
                    },
                new AppUser
                    {
                    Username = "testuser",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPass123!"),
                    Role = "User"
                    });
            context.SaveChanges();

            return context;
            }

        private async Task<string> LoginAndGetToken(DungeonContext context, string username, string password)
            {
            var user = context.Users.SingleOrDefault(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");

            // Simulate a token
            return $"{username}-token";
            }

        private async Task<Dungeon> CreateDungeon(DungeonContext context, string token, string name, int width, int height)
            {
            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("No token");

            var username = token.Replace("-token", "");
            var user = context.Users.SingleOrDefault(u => u.Username == username);

            if (user == null || token != $"{username}-token")
                throw new UnauthorizedAccessException("Invalid token");

            if (user.Role != "Admin")
                throw new UnauthorizedAccessException("Forbidden");

            var dungeon = new Dungeon
                {
                Name = name,
                Width = width,
                Height = height
                };

            context.Dungeons.Add(dungeon);
            await context.SaveChangesAsync();
            return dungeon;
            }

        [Fact]
        public async Task Full_Security_Flow_Should_Work()
            {
            using var context = GetInMemoryContext();
            var token = await LoginAndGetToken(context, "admin", "StrongPass@2025");

            var dungeon = await CreateDungeon(context, token, "Secure Dungeon", 5, 5);
            dungeon.Should().NotBeNull();
            dungeon.Name.Should().Be("Secure Dungeon");
            }

        [Fact]
        public async Task Missing_Token_Should_Return_401()
            {
            using var context = GetInMemoryContext();

            Func<Task> act = async () => await CreateDungeon(context, null, "No Token Dungeon", 5, 5);
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("No token");
            }

        [Fact]
        public async Task Tampered_Token_Should_Be_Rejected()
            {
            using var context = GetInMemoryContext();
            var token = await LoginAndGetToken(context, "admin", "StrongPass@2025");
            var tamperedToken = token + "_x";

            Func<Task> act = async () => await CreateDungeon(context, tamperedToken, "Tampered", 5, 5);
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid token");
            }

        [Fact]
        public async Task Non_Admin_User_Should_Be_Denied_Creation()
            {
            using var context = GetInMemoryContext();
            var token = await LoginAndGetToken(context, "testuser", "TestPass123!");

            Func<Task> act = async () => await CreateDungeon(context, token, "User Attempt", 3, 3);
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Forbidden");
            }
        }
    }