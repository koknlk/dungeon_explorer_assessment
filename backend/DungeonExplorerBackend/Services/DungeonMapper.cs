using DungeonExplorerBackend.Models.Entities;
using DungeonExplorerBackend.Models.Responses;

namespace DungeonExplorerBackend.Services
    {
    public class DungeonMapper
        {
        public DungeonResponse MapToResponse(Dungeon dungeon, DungeonSolution? solution = null)
            {
            var start = dungeon.Start ?? new Position { X = 0, Y = 0 };
            var goal = dungeon.Goal ?? new Position { X = 0, Y = 0 };

            solution ??= dungeon.Solutions?
                .OrderByDescending(s => s.SolvedAt)
                .FirstOrDefault();

            return new DungeonResponse
                {
                Id = dungeon.Id,
                Name = dungeon.Name,
                Width = dungeon.Width,
                Height = dungeon.Height,
                Start = new PositionResponse { X = start.X, Y = start.Y },
                Goal = new PositionResponse { X = goal.X, Y = goal.Y },
                Obstacles = dungeon.Obstacles?
                    .Select(o => new PositionResponse { X = o.X, Y = o.Y })
                    .ToList() ?? new List<PositionResponse>(),
                Solutions = new PathSolutionResponse
                    {
                    Path = solution?.Path?.Select(p => new PositionResponse { X = p.X, Y = p.Y }).ToList() ?? new List<PositionResponse>(),
                    ComputationTimeMs = solution?.ComputationTimeMs ?? 0,
                    Algorithm = "A*",
                    IsOptimal = true
                    }
                };
            }
        }
    }