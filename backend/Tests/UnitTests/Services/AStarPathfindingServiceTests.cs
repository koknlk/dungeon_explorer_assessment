using DungeonExplorerBackend.Models.Entities;
using DungeonExplorerBackend.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.UnitTests.Services;

public class AStarPathfindingServiceTests
    {
    private readonly AStarPathfindingService _service;
    private readonly Mock<ILogger<AStarPathfindingService>> _loggerMock;

    public AStarPathfindingServiceTests()
        {
        _loggerMock = new Mock<ILogger<AStarPathfindingService>>();
        _service = new AStarPathfindingService(_loggerMock.Object);
        }

    [Fact]
    public void FindPath_StraightLine_ReturnsOptimalPath()
        {
        var dungeon = new Dungeon
            {
            Width = 5,
            Height = 5,
            Start = new Position { X = 0, Y = 0 },
            Goal = new Position { X = 4, Y = 4 },
            Obstacles = new List<Position>()
            };

        var path = _service.FindPath(dungeon);

        Assert.NotEmpty(path);
        Assert.Equal(9, path.Count);
        Assert.Equal(0, path.First().X);
        Assert.Equal(0, path.First().Y);
        Assert.Equal(4, path.Last().X);
        Assert.Equal(4, path.Last().Y);
        Assert.True(IsPathValid(path, dungeon));
        }

    [Fact]
    public void FindPath_WithObstacles_FindsPathAround()
        {
        var dungeon = new Dungeon
            {
            Width = 5,
            Height = 5,
            Start = new Position { X = 0, Y = 0 },
            Goal = new Position { X = 4, Y = 4 },
            Obstacles = new List<Position>
            {
                new Position { X = 1, Y = 1 },
                new Position { X = 2, Y = 2 },
                new Position { X = 3, Y = 3 }
            }
            };

        var path = _service.FindPath(dungeon);

        Assert.NotEmpty(path);
        Assert.Equal(0, path.First().X);
        Assert.Equal(0, path.First().Y);
        Assert.Equal(4, path.Last().X);
        Assert.Equal(4, path.Last().Y);
        Assert.DoesNotContain(path, p => dungeon.Obstacles.Any(o => o.X == p.X && o.Y == p.Y));
        Assert.True(IsPathValid(path, dungeon));
        }

    [Fact]
    public void FindPath_NoPathExists_ThrowsDungeonPathfindingException()
        {
        var dungeon = new Dungeon
            {
            Width = 3,
            Height = 3,
            Start = new Position { X = 0, Y = 0 },
            Goal = new Position { X = 2, Y = 2 },
            Obstacles = new List<Position>
        {
            new Position { X = 1, Y = 0 },
            new Position { X = 0, Y = 1 },
            new Position { X = 1, Y = 1 },
            new Position { X = 2, Y = 1 },
            new Position { X = 1, Y = 2 }
        }
            };

        var exception = Assert.Throws<DungeonPathfindingException>(() => _service.FindPath(dungeon));

        Assert.Contains("No path exists", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

    [Fact]
    public void FindPath_StartEqualsGoal_ReturnsSinglePointPath()
        {
        var dungeon = new Dungeon
            {
            Width = 5,
            Height = 5,
            Start = new Position { X = 2, Y = 2 },
            Goal = new Position { X = 2, Y = 2 },
            Obstacles = new List<Position>()
            };

        var path = _service.FindPath(dungeon);

        Assert.Single(path);
        Assert.Equal(2, path[0].X);
        Assert.Equal(2, path[0].Y);
        }

    [Fact]
    public void FindPath_LargeGrid_Performant()
        {
        var dungeon = new Dungeon
            {
            Width = 50,
            Height = 50,
            Start = new Position { X = 0, Y = 0 },
            Goal = new Position { X = 49, Y = 49 },
            Obstacles = new List<Position>()
            };

        for (int i = 10; i < 40; i++)
            dungeon.Obstacles.Add(new Position { X = i, Y = 25 });

        var path = _service.FindPath(dungeon);

        Assert.NotEmpty(path);
        Assert.True(path.Count > 70);
        Assert.True(IsPathValid(path, dungeon));
        }

    [Fact]
    public void FindPath_StartIsObstacle_ThrowsDungeonPathfindingException()
        {
        var dungeon = new Dungeon
            {
            Width = 5,
            Height = 5,
            Start = new Position { X = 1, Y = 1 },
            Goal = new Position { X = 4, Y = 4 },
            Obstacles = new List<Position> { new Position { X = 1, Y = 1 } }
            };

        var exception = Assert.Throws<DungeonPathfindingException>(() => _service.FindPath(dungeon));

        Assert.Contains("Start position cannot be an obstacle", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

    [Fact]
    public void FindPath_GoalIsObstacle_ThrowsDungeonPathfindingException()
        {
        var dungeon = new Dungeon
            {
            Width = 5,
            Height = 5,
            Start = new Position { X = 0, Y = 0 },
            Goal = new Position { X = 1, Y = 1 },
            Obstacles = new List<Position> { new Position { X = 1, Y = 1 } }
            };

        var exception = Assert.Throws<DungeonPathfindingException>(() => _service.FindPath(dungeon));

        Assert.Contains("Goal position cannot be an obstacle", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

    private bool IsPathValid(List<Position> path, Dungeon dungeon)
        {
        if (path.First().X != dungeon.Start.X || path.First().Y != dungeon.Start.Y) return false;
        if (path.Last().X != dungeon.Goal.X || path.Last().Y != dungeon.Goal.Y) return false;
        if (path.Any(p => p.X < 0 || p.X >= dungeon.Width || p.Y < 0 || p.Y >= dungeon.Height)) return false;
        if (path.Any(p => dungeon.Obstacles.Any(o => o.X == p.X && o.Y == p.Y))) return false;

        for (int i = 1; i < path.Count; i++)
            {
            var dx = Math.Abs(path[i].X - path[i - 1].X);
            var dy = Math.Abs(path[i].Y - path[i - 1].Y);
            if (dx + dy != 1) return false;
            }
        return true;
        }
    }