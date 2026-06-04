using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Masarak.DTOs;
using Masarak.Policies;
using Masarak.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Masarak.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(typeof(AuthResponse), 400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // POST /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(typeof(AuthResponse), 401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        // POST /api/auth/refresh
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(typeof(AuthResponse), 400)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // POST /api/auth/logout
        [HttpPost("logout")]
        [AllowAnonymous] // Allow logging out even with expired token
        [ProducesResponseType(typeof(MessageResponse), 200)]
        public async Task<IActionResult> Logout([FromBody] RevokeTokenRequest request)
        {
            var result = await _authService.LogoutAsync(request.RefreshToken);
            return Ok(result);
        }

        // POST /api/auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(MessageResponse), 200)]
        [ProducesResponseType(typeof(MessageResponse), 400)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new MessageResponse { Success = false, Message = "Unauthorized." });

            var result = await _authService.ChangePasswordAsync(userId.Value, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // POST /api/auth/revoke-token
        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(typeof(MessageResponse), 200)]
        [ProducesResponseType(typeof(MessageResponse), 403)]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            if (userId == null || userRole == null)
                return Unauthorized(new MessageResponse { Success = false, Message = "Unauthorized." });

            var result = await _authService.RevokeTokenAsync(request.RefreshToken, userId.Value, userRole);

            if (!result.Success && result.Message == "Unauthorized.")
                return StatusCode(403, result);

            return Ok(result);
        }

        // GET /api/auth/me
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserInfoDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Me()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var user = await _authService.GetCurrentUserAsync(userId.Value);
            return user == null ? NotFound() : Ok(user);
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue("userid")
                        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                        ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(claim, out var id) ? id : null;
        }

        private string? GetCurrentUserRole() =>
            User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role);
    }
}
