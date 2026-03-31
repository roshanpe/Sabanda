using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Sabanda.Application.Auth.Commands;
using Sabanda.Application.Auth.DTOs;

namespace Sabanda.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly LoginCommandHandler _loginHandler;
    private readonly LogoutCommandHandler _logoutHandler;

    public AuthController(LoginCommandHandler loginHandler, LogoutCommandHandler logoutHandler)
    {
        _loginHandler = loginHandler;
        _logoutHandler = logoutHandler;
    }

    /// <summary>Authenticate with email and password to receive a JWT token.</summary>
    [HttpPost("login")]
    [EnableRateLimiting("loginPolicy")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _loginHandler.HandleAsync(request);
        return Ok(response);
    }

    /// <summary>Revoke the current session (logout).</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (string.IsNullOrEmpty(jti))
            return BadRequest();

        await _logoutHandler.HandleAsync(jti);
        return NoContent();
    }
}
