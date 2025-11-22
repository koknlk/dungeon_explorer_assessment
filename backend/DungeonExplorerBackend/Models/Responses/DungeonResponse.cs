namespace DungeonExplorerBackend.Models.Responses
    {
    public class DungeonResponse
        {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public PositionResponse Start { get; set; } = new();
        public PositionResponse Goal { get; set; } = new();
        public List<PositionResponse> Obstacles { get; set; } = new();
        public PathSolutionResponse? Solutions { get; set; }
        }
    }