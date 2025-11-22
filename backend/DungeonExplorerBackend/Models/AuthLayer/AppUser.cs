namespace DungeonExplorerBackend.Models.AuthLayer
    {
    public class AppUser
        {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;
        }
    }