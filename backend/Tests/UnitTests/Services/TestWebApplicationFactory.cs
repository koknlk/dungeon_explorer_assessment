using DungeonExplorerBackend.Data;
using DungeonExplorerBackend.Models.AuthLayer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Linq;

namespace Tests.UnitTests.Services
    {
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
        {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<DungeonContext>));
                if (descriptor != null) services.Remove(descriptor);

                // Add in-memory DungeonContext
                services.AddDbContext<DungeonContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Build the provider and seed users
                using var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DungeonContext>();

                if (!context.Users.Any())
                    {
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
                            }
                    );
                    context.SaveChanges();
                    }
            });
            }
        }
    }