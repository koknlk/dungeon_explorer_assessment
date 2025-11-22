using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Models.Entities;
using DungeonExplorerBackend.Models.Requests;
using DungeonExplorerBackend.Models.Responses;
using DungeonExplorerBackend.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests.UnitTests.Services
    {
    public class DungeonServiceTests
        {
        private readonly Mock<IDungeonRepository> _dungeonRepoMock;
        private readonly Mock<DungeonMapper> _mapperMock;
        private readonly Mock<ILogger<DungeonService>> _loggerMock;
        private readonly DungeonService _service;

        public DungeonServiceTests()
            {
            _dungeonRepoMock = new Mock<IDungeonRepository>();
            _mapperMock = new Mock<DungeonMapper>();
            _loggerMock = new Mock<ILogger<DungeonService>>();

            // Provide a fake IPathfindingService to construct DungeonSolverService
            var fakePathfindingService = new Mock<IPathfindingService>();
            fakePathfindingService.Setup(p => p.FindPath(It.IsAny<Dungeon>()))
                                  .Returns(new List<Position>
                                  {
                                      new Position { X = 0, Y = 0 },
                                      new Position { X = 1, Y = 0 },
                                      new Position { X = 2, Y = 0 },
                                      new Position { X = 2, Y = 1 },
                                      new Position { X = 2, Y = 2 }
                                  });

            var solverService = new DungeonSolverService(fakePathfindingService.Object);

            _service = new DungeonService(
                _dungeonRepoMock.Object,
                solverService,
                _mapperMock.Object,
                _loggerMock.Object
            );
            }

        [Fact]
        public async Task CreateDungeonAsync_ValidRequest_CallsRepositoryAndReturnsId()
            {
            // Arrange
            var request = new CreateDungeonRequest
                {
                Name = "Test Dungeon",
                Width = 10,
                Height = 10,
                Start = new PositionRequest { X = 0, Y = 0 },
                Goal = new PositionRequest { X = 9, Y = 9 },
                Obstacles = new List<PositionRequest>
                {
                    new PositionRequest { X = 1, Y = 1 },
                    new PositionRequest { X = 2, Y = 2 }
                }
                };

            Dungeon capturedDungeon = null!;
            _dungeonRepoMock.Setup(r => r.AddAsync(It.IsAny<Dungeon>()))
                .Callback<Dungeon>(d => capturedDungeon = d)
                .Returns(Task.CompletedTask);
            _dungeonRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var id = await _service.CreateDungeonAsync(request);

            // Assert
            _dungeonRepoMock.Verify(r => r.AddAsync(It.IsAny<Dungeon>()), Times.Once);
            _dungeonRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.NotNull(capturedDungeon);
            Assert.Equal(request.Name, capturedDungeon.Name);
            Assert.Equal(request.Obstacles.Count, capturedDungeon.Obstacles.Count);
            }

        [Fact]
        public async Task GetDungeonWithSolutionAsync_DungeonNotFound_ThrowsKeyNotFoundException()
            {
            // Arrange
            _dungeonRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Dungeon?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetDungeonWithSolutionAsync(1));
            }

        [Fact]
        public async Task GetDungeonWithSolutionAsync_ComputesPath_WhenNoSolutionExists()
            {
            // Arrange
            var dungeon = new Dungeon
                {
                Id = 1,
                Name = "Test Dungeon",
                Width = 3,
                Height = 3,
                Start = new Position { X = 0, Y = 0 },
                Goal = new Position { X = 2, Y = 2 },
                Obstacles = new List<Position>()
                };

            _dungeonRepoMock.Setup(r => r.GetByIdAsync(dungeon.Id))
                .ReturnsAsync(dungeon);

            _dungeonRepoMock.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.GetDungeonWithSolutionAsync(dungeon.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Solutions.Path.Count); // matches the path returned by fake IPathfindingService
            _dungeonRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            }

        [Fact]
        public async Task GetDungeonWithSolutionAsync_ReturnsExistingSolution_WhenSolutionExists()
            {
            // Arrange
            var dungeon = new Dungeon
                {
                Id = 1,
                Name = "Test Dungeon",
                Width = 3,
                Height = 3,
                Start = new Position { X = 0, Y = 0 },
                Goal = new Position { X = 2, Y = 2 },
                Obstacles = new List<Position>(),
                Solutions = new List<DungeonSolution>
        {
            new DungeonSolution
            {
                Path = new List<Position>
                {
                    new Position { X = 0, Y = 0 },
                    new Position { X = 2, Y = 2 }
                },
                ComputationTimeMs = 10,
                SolvedAt = DateTime.UtcNow
            }
        }
                };

            _dungeonRepoMock.Setup(r => r.GetByIdAsync(dungeon.Id))
                .ReturnsAsync(dungeon);

            // Act
            var result = await _service.GetDungeonWithSolutionAsync(dungeon.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Solutions.Path.Count); // matches the existing solution path
            }
        }
    }