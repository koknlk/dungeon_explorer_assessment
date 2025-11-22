namespace DungeonExplorerBackend.Models.Entities
    {
    public class Dungeon
        {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public Position Start { get; set; } = new();
        public Position Goal { get; set; } = new();
        public List<Position> Obstacles { get; set; } = new();
        public List<DungeonSolution> Solutions { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }
    }