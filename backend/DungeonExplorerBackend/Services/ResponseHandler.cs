using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Models.ApiResponseM;
using Microsoft.AspNetCore.Mvc;

namespace DungeonExplorerBackend.Services
    {
    public class ResponseHandler : IResponseHandler
        {
        public ActionResult<ApiResponse<T>> HandleError<T>(
            string message,
            int statusCode = 400,
            string? details = null,
            int? dungeonId = null)
            {
            return new ObjectResult(new ApiResponse<T>
                {
                Success = false,
                Message = message,
                Error = new DungeonErrorResponse
                    {
                    Message = message,
                    Details = details,
                    DungeonId = dungeonId
                    }
                })
                {
                StatusCode = statusCode
                };
            }

        public ActionResult<ApiResponse<T>> HandleException<T>(Exception ex, ILogger logger, string context)
            {
            logger.LogError(ex, "Error in {Context}", context);

            return ex switch
                {
                    DungeonPathfindingException dpe => HandleError<T>(
                        message: dpe.Message,
                        statusCode: 400,
                        details: ex.ToString(),
                        dungeonId: dpe.DungeonId
                    ),
                    KeyNotFoundException => HandleError<T>(ex.Message, 404, ex.ToString()),
                    ArgumentException => HandleError<T>(ex.Message, 400, ex.ToString()),
                    InvalidOperationException => HandleError<T>(ex.Message, 400, ex.ToString()),
                    _ => HandleError<T>("An unexpected error occurred", 500, ex.ToString())
                    };
            }

        public ActionResult<ApiResponse<T>> HandleSuccess<T>(T data, string message = "")
            {
            return new OkObjectResult(new ApiResponse<T>
                {
                Success = true,
                Data = data,
                Message = message
                });
            }
        }
    }