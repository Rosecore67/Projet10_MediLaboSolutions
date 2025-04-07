using IdentityAuthService.Models;
using IdentityAuthService.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IdentityAuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthSettings _admin;
        private readonly ITokenService _tokenService;

        public AuthController(IOptions<AuthSettings> adminOptions, ITokenService tokenService)
        {
            _admin = adminOptions.Value;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginModel login)
        {
            if (login.Username != _admin.Username || login.Password != _admin.Password)
                return Unauthorized(new { message = "Identifiants invalides" });

            var token = _tokenService.GenerateToken(login.Username);

            return Ok(new { token });
        }
    }
}
