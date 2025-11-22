using DungeonExplorerBackend.Models.Entities;

namespace DungeonExplorerBackend.Contracts
    {
    public interface IDungeonRepository
        {
        Task<Dungeon?> GetByIdAsync(int id);

        Task AddAsync(Dungeon dungeon);

        Task SaveChangesAsync();
        }
    }