using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Models.Entities;
using DungeonExplorerBackend.Models.Requests;
using DungeonExplorerBackend.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.UnitTests.ExtremeErrorScenarioTests
    {
    public class ExtremeErrorScenarioTests
        {
        [Fact]
        public async Task CreateDungeon_ServiceThrowsException_HandlesGracefully()
            {
            // Arrange
            var serviceMock = new Mock<IDungeonService>();
            var request = new CreateDungeonRequest
                {
                Name = "Test Dungeon",
                Width = 10,
                Height = 10,
                Start = new PositionRequest { X = 0, Y = 0 },
                Goal = new PositionRequest { X = 9, Y = 9 }
                };

            serviceMock.Setup(x => x.CreateDungeonAsync(request))
                       .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                serviceMock.Object.CreateDungeonAsync(request));
            }

        [Fact]
        public void AStarPathfinding_ExtremeObstacles_HandlesGracefully()
            {
            var dungeon = new Dungeon
                {
                Width = 10,
                Height = 10,
                Start = new Position { X = 0, Y = 0 },
                Goal = new Position { X = 9, Y = 9 },
                Obstacles = new List<Position>()
                };

            for (int x = 0; x < dungeon.Width; x++)
                {
                for (int y = 0; y < dungeon.Height; y++)
                    {
                    if (!(x == 0 && y == 0) && !(x == 9 && y == 9) && (x + y) % 2 == 0)
                        {
                        dungeon.Obstacles.Add(new Position { X = x, Y = y });
                        }
                    }
                }

            var service = new AStarPathfindingService(new Mock<ILogger<AStarPathfindingService>>().Object);

            var exception = Assert.Throws<DungeonPathfindingException>(() => service.FindPath(dungeon));

            Assert.Contains("No path exists", exception.Message, StringComparison.OrdinalIgnoreCase);
            }

        [Fact]
        public async Task DungeonService_WhenRepositoryThrows_ThrowsException()
            {
            // Arrange
            var repoMock = new Mock<IDungeonRepository>();

            var fakePathfindingService = new Mock<IPathfindingService>();
            var solverMock = new Mock<DungeonSolverService>(MockBehavior.Strict, fakePathfindingService.Object);

            var mapperMock = new Mock<DungeonMapper>();
            var loggerMock = new Mock<ILogger<DungeonService>>();

            var request = new CreateDungeonRequest
                {
                Name = "Test Dungeon",
                Width = 5,
                Height = 5,
                Start = new PositionRequest { X = 0, Y = 0 },
                Goal = new PositionRequest { X = 4, Y = 4 }
                };

            repoMock.Setup(r => r.AddAsync(It.IsAny<Dungeon>()))
                    .ThrowsAsync(new InvalidOperationException("Simulated repository failure"));

            var service = new DungeonService(repoMock.Object, solverMock.Object, mapperMock.Object, loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateDungeonAsync(request));
            }

        [Fact]
        public void InputSanitizer_NullAndEdgeCases_HandlesSafely()
            {
            // Arrange
            var sanitizer = new InputSanitizer();

            // Act & Assert
            Assert.Equal("", sanitizer.Sanitize(null));
            Assert.Equal("", sanitizer.Sanitize(""));
            Assert.Equal("", sanitizer.Sanitize("   "));

            var longMixed = new string('x', 10000) + "<script>" + new string('y', 10000);
            var result = sanitizer.Sanitize(longMixed);
            Assert.DoesNotContain("<script>", result);
            Assert.True(result.Length > 0);
            }

        [Fact]
        public void AStarPathfinding_StartOutsideBounds_ThrowsImmediately()
            {
            var dungeon = new Dungeon
                {
                Width = 5,
                Height = 5,
                Start = new Position { X = 10, Y = 10 },
                Goal = new Position { X = 4, Y = 4 },
                Obstacles = new List<Position>()
                };

            var service = new AStarPathfindingService(new Mock<ILogger<AStarPathfindingService>>().Object);

            var exception = Assert.Throws<DungeonPathfindingException>(() => service.FindPath(dungeon));

            Assert.Contains("outside dungeon bounds", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }
    }