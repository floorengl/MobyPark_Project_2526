using Microsoft.AspNetCore.Authorization;
using MobyPark_api.Dtos.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    // POST /register
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequestDto dto, CancellationToken ct)
    {
        var id = await _auth.RegisterAsync(dto, ct);
        Response.Headers.Location = "/profile";
        return StatusCode(201);
    }

    // <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <response code="200">Login successful</response>
    /// <response code="400">Invalid request or validation failed.</response>
    /// <response code="500">Internal server error.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var token = await _auth.LoginAsync(request);

        return Ok(new { message = "Logged in successfully", token });
    }

    // GET /profile (JWT required)
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ProfileResponseDto>> Profile(CancellationToken ct)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(idStr, out var userId))
        {
            return Unauthorized();
        }
            
        var profile = await _auth.GetProfileAsync(userId, ct);
        if (profile is null)
        {
            return NotFound();
        }  
        return Ok(profile);
    }
}
