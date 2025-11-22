using DungeonExplorerBackend.Models.Entities;

namespace DungeonExplorerBackend.Contracts
    {
    public interface IPathfindingService
        {
        List<Position> FindPath(Dungeon dungeon);
        }
    }