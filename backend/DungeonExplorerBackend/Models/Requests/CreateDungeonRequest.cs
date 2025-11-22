using System.ComponentModel.DataAnnotations;

namespace DungeonExplorerBackend.Models.Requests
    {
    public class CreateDungeonRequest
        {
        [Required(ErrorMessage = "Dungeon name is required")]
        [StringLength(100, ErrorMessage = "Dungeon name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Dungeon name can only contain letters, numbers, spaces, hyphens, and underscores")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Width is required")]
        [Range(5, 50, ErrorMessage = "Width must be between 5 and 50")]
        public int Width { get; set; }

        [Required(ErrorMessage = "Height is required")]
        [Range(5, 50, ErrorMessage = "Height must be between 5 and 50")]
        public int Height { get; set; }

        [Required(ErrorMessage = "Start position is required")]
        public PositionRequest Start { get; set; } = new();

        [Required(ErrorMessage = "Goal position is required")]
        public PositionRequest Goal { get; set; } = new();

        public List<PositionRequest> Obstacles { get; set; } = new();
        }
    }