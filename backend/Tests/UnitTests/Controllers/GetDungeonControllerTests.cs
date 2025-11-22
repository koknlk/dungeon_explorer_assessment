using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Controllers;
using DungeonExplorerBackend.Models.ApiResponseM;
using DungeonExplorerBackend.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.UnitTests.Controllers
    {
    public class GetDungeonControllerTests
        {
        private readonly GetDungeonController _controller;
        private readonly Mock<IDungeonService> _serviceMock;
        private readonly Mock<ILogger<GetDungeonController>> _loggerMock;
        private readonly Mock<IResponseHandler> _responseHandlerMock;

        public GetDungeonControllerTests()
            {
            _serviceMock = new Mock<IDungeonService>();
            _loggerMock = new Mock<ILogger<GetDungeonController>>();
            _responseHandlerMock = new Mock<IResponseHandler>();

            _controller = new GetDungeonController(
                _serviceMock.Object,
                _loggerMock.Object,
                _responseHandlerMock.Object);
            }

        [Fact]
        public async Task GetDungeon_ValidId_ReturnsDungeon()
            {
            // Arrange
            var dungeonId = 1;
            var dungeonResponse = new DungeonResponse { Id = dungeonId, Name = "Test" };

            _serviceMock.Setup(x => x.GetDungeonWithSolutionAsync(dungeonId))
                       .ReturnsAsync(dungeonResponse);
            _responseHandlerMock.Setup(x => x.HandleSuccess(It.IsAny<DungeonResponse>(), It.IsAny<string>()))
                              .Returns(new OkObjectResult(new ApiResponse<DungeonResponse> { Data = dungeonResponse, Success = true }));

            // Act
            var result = await _controller.GetDungeon(dungeonId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
            _serviceMock.Verify(x => x.GetDungeonWithSolutionAsync(dungeonId), Times.Once);
            }
        }
    }