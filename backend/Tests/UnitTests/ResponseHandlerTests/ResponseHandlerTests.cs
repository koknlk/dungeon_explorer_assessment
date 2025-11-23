using DungeonExplorerBackend.Models.ApiResponseM;
using DungeonExplorerBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.UnitTests.ResponseHandlerTests;

public class ResponseHandlerTests
    {
    private readonly ResponseHandler _handler = new();

    [Fact]
    public void HandleSuccess_ReturnsOkWithCorrectApiResponse()
        {
        // Arrange
        var data = 999;
        var message = "Dungeon created";

        // Act
        var result = _handler.HandleSuccess(data, message);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ApiResponse<int>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        var response = Assert.IsType<ApiResponse<int>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(data, response.Data);
        Assert.Equal(message, response.Message);
        }

    [Fact]
    public void HandleError_ReturnsCorrectStatusCodeAndResponse()
        {
        // Arrange
        var message = "Invalid input";
        const int statusCode = 400;

        // Act
        var result = _handler.HandleError<string>(message, statusCode);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ApiResponse<string>>>(result);
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(statusCode, objectResult.StatusCode);

        var response = Assert.IsType<ApiResponse<string>>(objectResult.Value);
        Assert.False(response.Success);
        Assert.Equal(message, response.Message);
        }

    [Fact]
    public void HandleException_KeyNotFoundException_ReturnsNotFound()
        {
        // Arrange
        var ex = new KeyNotFoundException("Dungeon not found");
        var loggerMock = new Mock<ILogger>();

        // Act
        var result = _handler.HandleException<string>(ex, loggerMock.Object, "getting dungeon");

        // Assert
        var actionResult = Assert.IsType<ActionResult<ApiResponse<string>>>(result);
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(404, objectResult.StatusCode);

        var response = Assert.IsType<ApiResponse<string>>(objectResult.Value);
        Assert.False(response.Success);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
        }

    [Fact]
    public void HandleException_ArgumentException_ReturnsBadRequest()
        {
        // Arrange
        var ex = new ArgumentException("Width must be positive");
        var loggerMock = new Mock<ILogger>();

        // Act
        var result = _handler.HandleException<string>(ex, loggerMock.Object, "creating dungeon");

        // Assert
        var actionResult = Assert.IsType<ActionResult<ApiResponse<string>>>(result);
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(400, objectResult.StatusCode);
        }

    [Fact]
    public void HandleException_GenericException_ReturnsInternalServerError()
        {
        // Arrange
        var ex = new Exception("Something went terribly wrong");
        var loggerMock = new Mock<ILogger>();

        // Act
        var result = _handler.HandleException<string>(ex, loggerMock.Object, "unknown operation");

        // Assert
        var actionResult = Assert.IsType<ActionResult<ApiResponse<string>>>(result);
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, objectResult.StatusCode);
        }
    }