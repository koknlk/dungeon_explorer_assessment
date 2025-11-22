using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace DungeonExplorerBackend.Controllers
    {
    [Route("api/v1/dungeons")]
    [ApiController]
    public class LoginController : ControllerBase
        {
        private readonly IUserRepository _userRepo;
        private readonly IJwtService _jwtService;

        public LoginController(IUserRepository userRepo, IJwtService jwtService)
            {
            _userRepo = userRepo;
            _jwtService = jwtService;
            }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
            {
            var user = await _userRepo.GetByUsernameAsync(request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                return Unauthorized(new { message = "Invalid credentials" });
                }

            var token = _jwtService.GenerateToken(user);
            return Ok(new { token });
            }
        }
    }