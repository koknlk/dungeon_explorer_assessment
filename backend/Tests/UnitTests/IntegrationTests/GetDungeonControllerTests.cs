using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Controllers;
using DungeonExplorerBackend.Data;
using DungeonExplorerBackend.Models.ApiResponseM;
using DungeonExplorerBackend.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.UnitTests.IntegrationTests
    {
    public class GetDungeonControllerTests
        {
        private readonly GetDungeonController _controller;
        private readonly DungeonContext _context;
        private readonly Mock<IDungeonService> _serviceMock;
        private readonly Mock<ILogger<GetDungeonController>> _loggerMock;
        private readonly Mock<IResponseHandler> _responseHandlerMock;

        public GetDungeonControllerTests()
            {
            var options = new DbContextOptionsBuilder<DungeonContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DungeonContext(options);
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
            var expectedResponse = new ApiResponse<DungeonResponse> { Success = true, Data = dungeonResponse, Message = "Dungeon retrieved successfully" };

            _serviceMock.Setup(x => x.GetDungeonWithSolutionAsync(dungeonId))
                       .ReturnsAsync(dungeonResponse);
            _responseHandlerMock.Setup(x => x.HandleSuccess(dungeonResponse, "Dungeon retrieved successfully"))
                              .Returns(new OkObjectResult(expectedResponse));

            // Act
            var result = await _controller.GetDungeon(dungeonId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<DungeonResponse>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(dungeonId, response.Data.Id);
            }

        [Fact]
        public async Task GetDungeon_InvalidId_ReturnsBadRequest()
            {
            // Arrange
            var invalidId = 0;
            var expectedResponse = new ApiResponse<object> { Success = false, Message = "Invalid dungeon ID" };

            // Fix: match all parameters with It.IsAny<>
            _responseHandlerMock
                .Setup(x => x.HandleError<DungeonResponse>(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string?>(),
                    It.IsAny<int?>()))
                .Returns((string message, int statusCode, string? details, int? dungeonId) =>
                    new BadRequestObjectResult(new ApiResponse<object>
                        {
                        Success = false,
                        Message = message
                        }));

            // Act
            var result = await _controller.GetDungeon(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(badRequestResult.Value);
            Assert.False(response.Success);
            Assert.Contains("Invalid dungeon ID", response.Message);
            }

        [Fact]
        public async Task GetDungeon_NotFound_ReturnsNotFound()
            {
            // Arrange
            var dungeonId = 999;
            var expectedResponse = new ApiResponse<object> { Success = false, Message = "Dungeon not found" };

            _serviceMock.Setup(x => x.GetDungeonWithSolutionAsync(dungeonId))
                       .ThrowsAsync(new KeyNotFoundException("Dungeon not found"));
            _responseHandlerMock.Setup(x => x.HandleException<DungeonResponse>(It.IsAny<Exception>(), It.IsAny<ILogger>(), It.IsAny<string>()))
                              .Returns(new NotFoundObjectResult(expectedResponse));

            // Act
            var result = await _controller.GetDungeon(dungeonId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<object>>(notFoundResult.Value);
            Assert.False(response.Success);
            }
        }
    }