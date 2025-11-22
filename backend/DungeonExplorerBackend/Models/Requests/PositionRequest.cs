using System.ComponentModel.DataAnnotations;

namespace DungeonExplorerBackend.Models.Requests
    {
    public class PositionRequest
        {
        [Required(ErrorMessage = "X coordinate is required")]
        [Range(0, 49, ErrorMessage = "X coordinate must be between 0 and 49")]
        public int X { get; set; }

        [Required(ErrorMessage = "Y coordinate is required")]
        [Range(0, 49, ErrorMessage = "Y coordinate must be between 0 and 49")]
        public int Y { get; set; }
        }
    }