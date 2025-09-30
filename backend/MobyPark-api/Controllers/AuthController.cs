using Microsoft.AspNetCore.Authorization;
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
