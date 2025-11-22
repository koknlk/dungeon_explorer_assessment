using System.ComponentModel.DataAnnotations;

namespace DungeonExplorerBackend.Models.Requests
    {
    public class LoginRequest
        {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;
        }
    }