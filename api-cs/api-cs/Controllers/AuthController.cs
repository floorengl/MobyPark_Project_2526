using Microsoft.AspNetCore.Mvc;
using ApiCs.DTOs;
using ApiCs.Services;
using ApiCs.Infrastructure;

namespace ApiCs.Controllers;


[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var (ok, msg) = _auth.RegisterAsync(req);
        if (!ok)
        {
            return StatusCode(200, "Username already taken");
        }
        return StatusCode(201, "User created");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        
    }
}