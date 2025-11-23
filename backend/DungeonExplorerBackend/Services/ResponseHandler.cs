using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Models.ApiResponseM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DungeonExplorerBackend.Services
    {
    public class ResponseHandler : IResponseHandler
        {
        public ActionResult<ApiResponse<T>> HandleError<T>(
            string message,
            int statusCode = 400,
            Dictionary<string, string[]>? details = null,
            int? dungeonId = null,
            string errorCode = "DOMAIN_ERROR")
            {
            var normalizedDetails = details?.ToDictionary(
                kvp => kvp.Key.Replace("$.", "").Replace(".", "_").Replace("start", "Start").Replace("x", "X"),
                kvp => kvp.Value
            );

            return new ObjectResult(new ApiResponse<T>
                {
                Success = false,
                Message = message,
                Error = new DungeonErrorResponse
                    {
                    Code = errorCode,
                    Message = "One or more validation errors occurred.",
                    Details = normalizedDetails ?? new Dictionary<string, string[]>(),
                    DungeonId = dungeonId
                    }
                })
                {
                StatusCode = statusCode
                };
            }

        public ActionResult<ApiResponse<T>> HandleException<T>(
            Exception ex,
            ILogger logger,
            string context)
            {
            logger.LogError(ex, "Error in {Context}", context);

            return ex switch
                {
                    DungeonPathfindingException dpe => HandleError<T>(
                        message: dpe.Message,
                        statusCode: 400,
                        details: new Dictionary<string, string[]>
                        {
                        { "Pathfinding", new[] { dpe.Message } }
                        },
                        dungeonId: dpe.DungeonId,
                        errorCode: "PATHFINDING_ERROR"
                    ),

                    KeyNotFoundException => HandleError<T>(
                        message: "Resource not found",
                        statusCode: 404,
                        errorCode: "NOT_FOUND"
                    ),

                    ArgumentException ae => HandleError<T>(
                        message: ae.Message,
                        statusCode: 400,
                        errorCode: "INVALID_ARGUMENT"
                    ),

                    InvalidOperationException ioe => HandleError<T>(
                        message: ioe.Message,
                        statusCode: 400,
                        errorCode: "INVALID_OPERATION"
                    ),

                    _ => HandleError<T>(
                        message: "An unexpected error occurred",
                        statusCode: 500,
                        errorCode: "INTERNAL_SERVER_ERROR"
                    )
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