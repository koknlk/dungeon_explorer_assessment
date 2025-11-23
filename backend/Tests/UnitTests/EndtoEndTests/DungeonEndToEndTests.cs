using DungeonExplorerBackend.Controllers;
using DungeonExplorerBackend.Data;
using DungeonExplorerBackend.Models.ApiResponseM;
using DungeonExplorerBackend.Models.Requests;
using DungeonExplorerBackend.Models.Responses;
using DungeonExplorerBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.UnitTests.EndtoEndTests
    {
    public class DungeonEndToEndTests
        {
        private readonly CreateDungeonController _createController;
        private readonly GetDungeonController _getController;
        private readonly DungeonContext _context;
        private readonly DungeonService _dungeonService;
        private readonly AStarPathfindingService _pathfindingService;
        private readonly InputSanitizer _sanitizer;
        private readonly ResponseHandler _responseHandler;

        public DungeonEndToEndTests()
            {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<DungeonContext>()
                .UseInMemoryDatabase(databaseName: "IntegrationTestDb")
                .Options;

            _context = new DungeonContext(options);

            // Setup repository
            var dungeonRepo = new DungeonRepository(_context);

            // Setup solver service
            _pathfindingService = new AStarPathfindingService(new Mock<ILogger<AStarPathfindingService>>().Object);
            var solverService = new DungeonSolverService(_pathfindingService);

            // Initialize DungeonService with repository, solver, mapper, logger
            _dungeonService = new DungeonService(
                dungeonRepo,
                solverService,
                new DungeonMapper(),
                new Mock<ILogger<DungeonService>>().Object
            );

            _sanitizer = new InputSanitizer();
            _responseHandler = new ResponseHandler();

            _createController = new CreateDungeonController(
                _dungeonService,
                _sanitizer,
                new Mock<ILogger<CreateDungeonController>>().Object,
                _responseHandler
            );

            _getController = new GetDungeonController(
                _dungeonService,
                new Mock<ILogger<GetDungeonController>>().Object,
                _responseHandler
            );
            }

        [Fact]
        public async Task CreateThenGetDungeon_ReturnsDungeonWithSolution()
            {
            // Arrange
            var createRequest = new CreateDungeonRequest
                {
                Name = "Integration Test Dungeon",
                Width = 5,
                Height = 5,
                Start = new PositionRequest { X = 0, Y = 0 },
                Goal = new PositionRequest { X = 4, Y = 4 },
                Obstacles = new List<PositionRequest>
                {
                    new PositionRequest { X = 1, Y = 1 },
                    new PositionRequest { X = 2, Y = 2 }
                }
                };

            //Create dungeon
            var createResult = await _createController.CreateDungeon(createRequest);
            var createOkResult = Assert.IsType<OkObjectResult>(createResult.Result);
            var createResponse = Assert.IsType<ApiResponse<int>>(createOkResult.Value);
            var dungeonId = createResponse.Data;

            //Get dungeon
            var getResult = await _getController.GetDungeon(dungeonId);
            var getOkResult = Assert.IsType<OkObjectResult>(getResult.Result);
            var getResponse = Assert.IsType<ApiResponse<DungeonResponse>>(getOkResult.Value);

            // Assert
            Assert.True(getResponse.Success);
            Assert.NotNull(getResponse.Data);
            Assert.Equal(dungeonId, getResponse.Data.Id);
            Assert.NotNull(getResponse.Data.Solutions);
            Assert.NotEmpty(getResponse.Data.Solutions.Path);
            Assert.Equal("A*", getResponse.Data.Solutions.Algorithm);
            }
        }
    }