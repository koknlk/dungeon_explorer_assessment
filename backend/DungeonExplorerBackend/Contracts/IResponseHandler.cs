using DungeonExplorerBackend.Models.ApiResponseM;
using Microsoft.AspNetCore.Mvc;

namespace DungeonExplorerBackend.Contracts
    {
    public interface IResponseHandler
        {
        ActionResult<ApiResponse<T>> HandleSuccess<T>(T data, string message = "");

        ActionResult<ApiResponse<T>> HandleError<T>(string message, int statusCode = 400, string? details = null, int? dungeonId = null);

        ActionResult<ApiResponse<T>> HandleException<T>(Exception ex, ILogger logger, string context);
        }
    }