using DungeonExplorerBackend.Models.AuthLayer;

namespace DungeonExplorerBackend.Contracts
    {
    public interface IJwtService
        {
        string GenerateToken(AppUser user);
        }
    }