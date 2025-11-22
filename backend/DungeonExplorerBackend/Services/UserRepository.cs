using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Data;
using DungeonExplorerBackend.Models.AuthLayer;
using Microsoft.EntityFrameworkCore;

namespace DungeonExplorerBackend.Services
    {
    public class UserRepository : IUserRepository
        {
        private readonly DungeonContext _context;

        public UserRepository(DungeonContext context)
            {
            _context = context;
            }

        public async Task<AppUser?> GetByUsernameAsync(string username)
            {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            }

        public async Task AddAsync(AppUser user)
            {
            await _context.Users.AddAsync(user);
            }

        public async Task SaveChangesAsync()
            {
            await _context.SaveChangesAsync();
            }
        }
    }