using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Controllers;
using DungeonExplorerBackend.Models.ApiResponseM;
using DungeonExplorerBackend.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.UnitTests.Controllers
    {
    public class CreateDungeonControllerTests
        {
        private readonly CreateDungeonController _controller;
        private readonly Mock<IDungeonService> _serviceMock;
        private readonly Mock<IInputSanitizer> _sanitizerMock;
        private readonly Mock<ILogger<CreateDungeonController>> _loggerMock;
        private readonly Mock<IResponseHandler> _responseHandlerMock;

        public CreateDungeonControllerTests()
            {
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
        public async Task CreateDungeon_ValidRequest_ReturnsSuccess()
            {
            // Arrange
            var request = new CreateDungeonRequest
                {
                Name = "Test Dungeon",
                Width = 10,
                Height = 10,
                Start = new PositionRequest { X = 0, Y = 0 },
                Goal = new PositionRequest { X = 9, Y = 9 }
                };

            _sanitizerMock.Setup(x => x.Sanitize(It.IsAny<string>())).Returns("Test Dungeon");
            _serviceMock.Setup(x => x.CreateDungeonAsync(request)).ReturnsAsync(1);
            _responseHandlerMock.Setup(x => x.HandleSuccess(It.IsAny<int>(), It.IsAny<string>()))
                              .Returns(new OkObjectResult(new ApiResponse<int> { Data = 1, Success = true }));

            // Act
            var result = await _controller.CreateDungeon(request);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            _sanitizerMock.Verify(x => x.Sanitize("Test Dungeon"), Times.Once);
            _serviceMock.Verify(x => x.CreateDungeonAsync(request), Times.Once);
            }
        }
    }