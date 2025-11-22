using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Controllers;
using DungeonExplorerBackend.Data;
using DungeonExplorerBackend.Models.ApiResponseM;
using DungeonExplorerBackend.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.UnitTests.IntegrationTests
    {
    public class CreateDungeonControllerTests
        {
        private readonly CreateDungeonController _controller;
        private readonly DungeonContext _context;
        private readonly Mock<IDungeonService> _serviceMock;
        private readonly Mock<IInputSanitizer> _sanitizerMock;
        private readonly Mock<ILogger<CreateDungeonController>> _loggerMock;
        private readonly Mock<IResponseHandler> _responseHandlerMock;

        public CreateDungeonControllerTests()
            {
            var options = new DbContextOptionsBuilder<DungeonContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DungeonContext(options);
            _serviceMock = new Mock<IDungeonService>();
            _sanitizerMock = new Mock<IInputSanitizer>();
            _loggerMock = new Mock<ILogger<CreateDungeonController>>();
            _responseHandlerMock = new Mock<IResponseHandler>();

            _controller = new CreateDungeonController(
                _serviceMock.Object,
                _sanitizerMock.Object,
                _loggerMock.Object,
                _responseHandlerMock.Object);
            }

        [Fact]
        public async Task CreateDungeon_ValidRequest_ReturnsOkWithId()
            {
            // Arrange
            var request = new CreateDungeonRequest
                {
                Name = "Test Dungeon",
                Width = 10,
                Height = 10,
                Start = new PositionRequest { X = 0, Y = 0 },
                Goal = new PositionRequest { X = 9, Y = 9 },
                Obstacles = new List<PositionRequest>()
                };

            var expectedResponse = new ApiResponse<int> { Success = true, Data = 1, Message = "Dungeon created successfully" };

            _sanitizerMock.Setup(x => x.Sanitize(It.IsAny<string>())).Returns(request.Name);
            _serviceMock.Setup(x => x.CreateDungeonAsync(request)).ReturnsAsync(1);
            _responseHandlerMock.Setup(x => x.HandleSuccess(1, "Dungeon created successfully"))
                              .Returns(new OkObjectResult(expectedResponse));

            // Act
            var result = await _controller.CreateDungeon(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<int>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(1, response.Data);
            _sanitizerMock.Verify(x => x.Sanitize(request.Name), Times.Once);
            }

        [Fact]
        public async Task CreateDungeon_InvalidModel_ReturnsBadRequest_WithProperResponse()
            {
            // Arrange
            var request = new CreateDungeonRequest
                {
                Name = "", // triggers validation error
                Width = 10,
                Height = 10,
                Start = new PositionRequest { X = 0, Y = 0 },
                Goal = new PositionRequest { X = 9, Y = 9 },
                Obstacles = new List<PositionRequest>()
                };

            // Manually invalidate ModelState (this is how it happens in real requests)
            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            // Setup the mock to return a proper BadRequest with our expected response
            var expectedResponse = new ApiResponse<int>
                {
                Success = false,
                Message = "Validation failed"
                };

            _responseHandlerMock
            .Setup(x => x.HandleError<int>(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<int?>()))
            .Returns((string message, int statusCode, string? details, int? dungeonId) =>
                new BadRequestObjectResult(new ApiResponse<int>
                    {
                    Success = false,
                    Message = message
                    }));

            // Act
            var result = await _controller.CreateDungeon(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var actualResponse = Assert.IsType<ApiResponse<int>>(badRequestResult.Value);
            Assert.False(actualResponse.Success);
            Assert.Equal("Validation failed", actualResponse.Message);
            }
        }
    }