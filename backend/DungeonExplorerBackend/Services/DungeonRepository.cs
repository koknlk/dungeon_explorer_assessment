using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Data;
using DungeonExplorerBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DungeonExplorerBackend.Services
    {
    public class DungeonRepository : IDungeonRepository
        {
        private readonly DungeonContext _context;

        public DungeonRepository(DungeonContext context)
            {
            _context = context;
            }

        public async Task<Dungeon?> GetByIdAsync(int id)
            {
            return await _context.Dungeons
                .Include(d => d.Obstacles)
                .Include(d => d.Solutions)
                    .ThenInclude(s => s.Path)
                .FirstOrDefaultAsync(d => d.Id == id);
            }

        public async Task AddAsync(Dungeon dungeon)
            {
            await _context.Dungeons.AddAsync(dungeon);
            }

        public async Task SaveChangesAsync()
            {
            await _context.SaveChangesAsync();
            }
        }
    }