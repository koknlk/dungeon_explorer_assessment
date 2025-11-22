namespace DungeonExplorerBackend.Models.Entities
    {
    public class DungeonSolution
        {
        public int Id { get; set; }
        public int DungeonId { get; set; }
        public Dungeon Dungeon { get; set; } = null!;
        public List<Position> Path { get; set; } = new();
        public long ComputationTimeMs { get; set; }
        public DateTime SolvedAt { get; set; } = DateTime.UtcNow;
        }
    }