namespace DungeonExplorerBackend.Models.ApiResponseM
    {
    public class ApiResponse<T>
        {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public DungeonErrorResponse? Error { get; set; }
        }
    }