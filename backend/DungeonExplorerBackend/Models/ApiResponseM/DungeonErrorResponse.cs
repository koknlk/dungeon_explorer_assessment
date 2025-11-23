namespace DungeonExplorerBackend.Models.ApiResponseM
    {
    public class DungeonErrorResponse
        {
        public string Code { get; set; } = "VALIDATION_ERROR";
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? Details { get; set; }
        public int? DungeonId { get; set; }
        }
    }