namespace DungeonExplorerBackend.Models.Responses
    {
    public class PathSolutionResponse
        {
        public List<PositionResponse> Path { get; set; } = new();
        public long ComputationTimeMs { get; set; }
        public string Algorithm { get; set; } = "A*";
        public bool IsOptimal { get; set; } = true;
        }
    }