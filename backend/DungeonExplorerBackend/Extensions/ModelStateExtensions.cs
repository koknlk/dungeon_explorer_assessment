using DungeonExplorerBackend.Models.ApiResponseM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DungeonExplorerBackend.Extensions
    {
    public static class ModelStateExtensions
        {
        public static BadRequestObjectResult ToApiResponse(this ModelStateDictionary modelState)
            {
            var errors = modelState
                .Where(e => e.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp =>
                    {
                        var key = kvp.Key.StartsWith("$.") ? kvp.Key.Substring(2) : kvp.Key;
                        key = key.Replace("[", ".").Replace("]", "");
                        return key;
                    },
                    kvp => kvp.Value.Errors.Select(e =>
                        e.ErrorMessage.Contains("could not be converted")
                            ? "The value must be an integer."
                            : e.ErrorMessage
                    ).ToArray()
                );

            var response = new ApiResponse<object>
                {
                Success = false,
                Message = "Validation failed",
                Error = new DungeonErrorResponse
                    {
                    Code = "VALIDATION_ERROR",
                    Message = "One or more validation errors occurred.",
                    Details = errors
                    }
                };

            return new BadRequestObjectResult(response);
            }
        }
    }