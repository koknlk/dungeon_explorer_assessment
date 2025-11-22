namespace DungeonExplorerBackend.Models.ApiResponseM
    {
    public class DungeonErrorResponse
        {
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public int? DungeonId { get; set; }
        }
    }