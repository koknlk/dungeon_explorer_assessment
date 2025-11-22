using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Models.Entities;
using System.Diagnostics;

namespace DungeonExplorerBackend.Services
    {
    public class DungeonSolverService
        {
        private readonly IPathfindingService _pathfindingService;

        public DungeonSolverService(IPathfindingService pathfindingService)
            {
            _pathfindingService = pathfindingService;
            }

        public (List<Position> Path, long ComputationTimeMs) ComputePath(Dungeon dungeon)
            {
            if (dungeon == null)
                throw new ArgumentNullException(nameof(dungeon));

            var stopwatch = Stopwatch.StartNew();
            var path = _pathfindingService.FindPath(dungeon) ?? new List<Position>();
            stopwatch.Stop();

            return (path, stopwatch.ElapsedMilliseconds);
            }
        }
    }