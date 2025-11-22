using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Models.Entities;
using Microsoft.Extensions.Logging;

namespace DungeonExplorerBackend.Services
    {
    public class AStarPathfindingService : IPathfindingService
        {
        private readonly ILogger<AStarPathfindingService> _logger;

        public AStarPathfindingService(ILogger<AStarPathfindingService> logger)
            {
            _logger = logger;
            }

        public List<Position> FindPath(Dungeon dungeon)
            {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            try
                {
                ValidateDungeon(dungeon);

                var obstacles = new HashSet<(int, int)>(dungeon.Obstacles.Select(o => (o.X, o.Y)));
                var start = (dungeon.Start.X, dungeon.Start.Y);
                var goal = (dungeon.Goal.X, dungeon.Goal.Y);

                var path = FindPathAStar(start, goal, dungeon.Width, dungeon.Height, obstacles);

                watch.Stop();
                _logger.LogInformation("A* found path in {ms}ms for {width}x{height} grid",
                    watch.ElapsedMilliseconds, dungeon.Width, dungeon.Height);

                return path.Select(p => new Position { X = p.X, Y = p.Y }).ToList();
                }
            catch (DungeonPathfindingException)
                {
                // Already a structured pathfinding exception, just rethrow
                throw;
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Pathfinding failed for dungeon {id}", dungeon.Id);
                throw new DungeonPathfindingException(dungeon.Id, $"Pathfinding failed: {ex.Message}");
                }
            }

        private List<(int X, int Y)> FindPathAStar((int X, int Y) start, (int X, int Y) goal,
            int width, int height, HashSet<(int X, int Y)> obstacles)
            {
            var openSet = new PriorityQueue<(int X, int Y), int>();
            var openSetHash = new HashSet<(int X, int Y)>();
            var closedSet = new HashSet<(int X, int Y)>();
            var gScore = new Dictionary<(int X, int Y), int>();
            var fScore = new Dictionary<(int X, int Y), int>();
            var cameFrom = new Dictionary<(int X, int Y), (int X, int Y)>();

            gScore[start] = 0;
            fScore[start] = ManhattanHeuristic(start, goal);
            openSet.Enqueue(start, fScore[start]);
            openSetHash.Add(start);

            while (openSet.Count > 0)
                {
                var current = openSet.Dequeue();
                openSetHash.Remove(current);

                if (current.Equals(goal))
                    return ReconstructPath(cameFrom, current);

                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(current, width, height, obstacles))
                    {
                    if (closedSet.Contains(neighbor)) continue;

                    var tentativeGScore = gScore[current] + 1;

                    if (!openSetHash.Contains(neighbor) || tentativeGScore < gScore.GetValueOrDefault(neighbor, int.MaxValue))
                        {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + ManhattanHeuristic(neighbor, goal);

                        if (!openSetHash.Contains(neighbor))
                            {
                            openSet.Enqueue(neighbor, fScore[neighbor]);
                            openSetHash.Add(neighbor);
                            }
                        }
                    }
                }

            throw new DungeonPathfindingException(-1, $"No path exists from ({start.X},{start.Y}) to ({goal.X},{goal.Y})");
            }

        private int ManhattanHeuristic((int X, int Y) a, (int X, int Y) b) =>
            Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        private List<(int X, int Y)> GetNeighbors((int X, int Y) node, int width, int height, HashSet<(int X, int Y)> obstacles)
            {
            var neighbors = new List<(int X, int Y)>();
            var directions = new (int X, int Y)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };

            foreach (var (dx, dy) in directions)
                {
                var newX = node.X + dx;
                var newY = node.Y + dy;

                if (newX >= 0 && newX < width && newY >= 0 && newY < height && !obstacles.Contains((newX, newY)))
                    neighbors.Add((newX, newY));
                }

            return neighbors;
            }

        private List<(int X, int Y)> ReconstructPath(Dictionary<(int X, int Y), (int X, int Y)> cameFrom, (int X, int Y) current)
            {
            var path = new List<(int X, int Y)> { current };
            while (cameFrom.ContainsKey(current))
                {
                current = cameFrom[current];
                path.Add(current);
                }
            path.Reverse();
            return path;
            }

        private void ValidateDungeon(Dungeon dungeon)
            {
            if (dungeon.Start.X < 0 || dungeon.Start.X >= dungeon.Width ||
                dungeon.Start.Y < 0 || dungeon.Start.Y >= dungeon.Height)
                throw new DungeonPathfindingException(dungeon.Id, "Start position is outside dungeon bounds");

            if (dungeon.Goal.X < 0 || dungeon.Goal.X >= dungeon.Width ||
                dungeon.Goal.Y < 0 || dungeon.Goal.Y >= dungeon.Height)
                throw new DungeonPathfindingException(dungeon.Id, "Goal position is outside dungeon bounds");

            if (dungeon.Obstacles.Any(o => o.X == dungeon.Start.X && o.Y == dungeon.Start.Y))
                throw new DungeonPathfindingException(dungeon.Id, "Start position cannot be an obstacle");

            if (dungeon.Obstacles.Any(o => o.X == dungeon.Goal.X && o.Y == dungeon.Goal.Y))
                throw new DungeonPathfindingException(dungeon.Id, "Goal position cannot be an obstacle");
            }
        }

    public class DungeonPathfindingException : Exception
        {
        public int DungeonId { get; }

        public DungeonPathfindingException(int dungeonId, string message)
            : base(message)
            {
            DungeonId = dungeonId;
            }
        }
    }