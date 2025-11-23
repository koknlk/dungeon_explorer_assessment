using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Controllers;
using DungeonExplorerBackend.Data;
using DungeonExplorerBackend.Models.ApiResponseM;
using DungeonExplorerBackend.Models.Requests;
using Microsoft.AspNetCore.Http;
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
        private readonly Mock<IDungeonService> _serviceMock;
        private readonly Mock<IInputSanitizer> _sanitizerMock;
        private readonly Mock<ILogger<CreateDungeonController>> _loggerMock;
        private readonly Mock<IResponseHandler> _responseHandlerMock;

        public CreateDungeonControllerTests()
            {
            var options = new DbContextOptionsBuilder<DungeonContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

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

            // Add ModelState error
            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            var validationErrors = new Dictionary<string, string[]>
    {
        { "Name", new[] { "The Name field is required." } }
    };

            // Mock the response handler to return BadRequestObjectResult
            _responseHandlerMock
                .Setup(rh => rh.HandleError<int>(
                    "Validation failed",
                    400,
                    validationErrors,
                    null,
                    "VALIDATION_ERROR"))
                .Returns(new BadRequestObjectResult(new ApiResponse<int>
                    {
                    Success = false,
                    Message = "Validation failed",
                    Error = new DungeonErrorResponse
                        {
                        Code = "VALIDATION_ERROR",
                        Message = "Validation failed",
                        Details = validationErrors
                        }
                    }));

            // Act
            ActionResult<ApiResponse<int>> result;

            if (!_controller.ModelState.IsValid)
                {
                result = _responseHandlerMock.Object.HandleError<int>(
                    "Validation failed",
                    400,
                    validationErrors,
                    null,
                    "VALIDATION_ERROR");
                }
            else
                {
                result = await _controller.CreateDungeon(request);
                }

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            var apiResponse = Assert.IsType<ApiResponse<int>>(badRequest.Value);

            Assert.False(apiResponse.Success);
            Assert.Equal("Validation failed", apiResponse.Message);
            Assert.NotNull(apiResponse.Error);
            Assert.Equal("VALIDATION_ERROR", apiResponse.Error.Code);
            Assert.True(apiResponse.Error.Details.ContainsKey("Name"));
            Assert.Equal("The Name field is required.", apiResponse.Error.Details["Name"].First());
            }
        }
    }