using DungeonExplorerBackend.Models.AuthLayer;

namespace DungeonExplorerBackend.Contracts
    {
    public interface IUserRepository
        {
        Task<AppUser?> GetByUsernameAsync(string username);

        Task AddAsync(AppUser user);
        }
    }