using DungeonExplorerBackend.Models.Entities;
using DungeonExplorerBackend.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using Xunit;

namespace Tests.UnitTests.PerformanceStessTests
    {
    public class PerformanceStressTests
        {
        private readonly AStarPathfindingService _pathfindingService;
        private readonly Mock<ILogger<AStarPathfindingService>> _loggerMock;

        public PerformanceStressTests()
            {
            _loggerMock = new Mock<ILogger<AStarPathfindingService>>();
            _pathfindingService = new AStarPathfindingService(_loggerMock.Object);
            }

        [Fact]
        public void FindPath_MaximumGridSize_50x50_PerformanceWithinLimits()
            {
            // Arrange
            var dungeon = new Dungeon
                {
                Width = 50,
                Height = 50,
                Start = new Position { X = 0, Y = 0 },
                Goal = new Position { X = 49, Y = 49 },
                Obstacles = new List<Position>()
                };

            // Add complex obstacle pattern
            for (int i = 10; i < 40; i += 2)
                {
                dungeon.Obstacles.Add(new Position { X = i, Y = 15 });
                dungeon.Obstacles.Add(new Position { X = i, Y = 35 });
                }

            // Act & Measure
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var path = _pathfindingService.FindPath(dungeon);
            watch.Stop();

            // Assert
            Assert.True(watch.ElapsedMilliseconds < 1000,
                $"Pathfinding took {watch.ElapsedMilliseconds}ms, expected < 1000ms for 50x50 grid");
            Assert.NotEmpty(path);
            Assert.True(IsPathValid(path, dungeon));
            }

        [Fact]
        public void FindPath_HighObstacleDensity_PerformanceWithinLimits()
            {
            // Arrange
            var dungeon = new Dungeon
                {
                Width = 20,
                Height = 20,
                Start = new Position { X = 0, Y = 0 },
                Goal = new Position { X = 19, Y = 19 },
                Obstacles = new List<Position>()
                };

            var random = new Random(42);

            for (int x = 0; x < 20; x++)
                {
                for (int y = 0; y < 20; y++)
                    {
                    if ((x == 0 && y == 0) || (x == 19 && y == 19)) continue;
                    if (random.NextDouble() < 0.70)
                        dungeon.Obstacles.Add(new Position { X = x, Y = y });
                    }
                }

            for (int row = 0; row < 20; row++)
                {
                int y = row;
                int startX = row % 2 == 0 ? 0 : 19;
                int endX = row % 2 == 0 ? 19 : 0;
                int step = row % 2 == 0 ? 1 : -1;

                for (int x = startX; row % 2 == 0 ? x <= endX : x >= endX; x += step)
                    {
                    dungeon.Obstacles.RemoveAll(o => o.X == x && o.Y == y);
                    }

                if (row < 19)
                    {
                    int connectorX = row % 2 == 0 ? 19 : 0;
                    dungeon.Obstacles.RemoveAll(o => o.X == connectorX && o.Y == row + 1);
                    }
                }

            dungeon.Obstacles.RemoveAll(o => (o.X == 0 && o.Y == 0) || (o.X == 19 && o.Y == 19));

            // Act
            var watch = Stopwatch.StartNew();
            var path = _pathfindingService.FindPath(dungeon);
            watch.Stop();

            // Assert
            Assert.NotEmpty(path);
            Assert.True(watch.ElapsedMilliseconds < 800,
                $"Pathfinding took {watch.ElapsedMilliseconds}ms — expected < 800ms on high-density 20×20 grid");
            }

        [Fact]
        public void FindPath_MemoryUsage_LargeDungeons_WithinLimits()
            {
            // Arrange
            var dungeon = new Dungeon
                {
                Width = 50,
                Height = 50,
                Start = new Position { X = 0, Y = 0 },
                Goal = new Position { X = 49, Y = 49 },
                Obstacles = new List<Position>()
                };

            // Act
            var startMemory = GC.GetTotalMemory(true);
            var path = _pathfindingService.FindPath(dungeon);
            var endMemory = GC.GetTotalMemory(true);
            var memoryUsed = endMemory - startMemory;

            // Assert
            Assert.True(memoryUsed < 100 * 1024 * 1024,
                $"Memory usage {memoryUsed / 1024 / 1024}MB exceeds 100MB limit");
            Assert.NotEmpty(path);
            }

        private bool IsPathValid(List<Position> path, Dungeon dungeon)
            {
            if (path.First().X != dungeon.Start.X || path.First().Y != dungeon.Start.Y) return false;
            if (path.Last().X != dungeon.Goal.X || path.Last().Y != dungeon.Goal.Y) return false;
            if (path.Any(p => p.X < 0 || p.X >= dungeon.Width || p.Y < 0 || p.Y >= dungeon.Height)) return false;
            if (path.Any(p => dungeon.Obstacles.Any(o => o.X == p.X && o.Y == p.Y))) return false;

            for (int i = 1; i < path.Count; i++)
                {
                var prev = path[i - 1];
                var current = path[i];
                if (Math.Abs(current.X - prev.X) + Math.Abs(current.Y - prev.Y) != 1) return false;
                }
            return true;
            }
        }
    }