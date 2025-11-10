using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using MobyPark_api.Dtos.Auth;


public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IConfiguration _cfg;

    public AuthService(AppDbContext db, IPasswordHasher<User> hasher, IConfiguration cfg)
        => (_db, _hasher, _cfg) = (db, hasher, cfg);


    // Creating a new user row and returning the id of that user.
    public async Task<long> RegisterAsync(RegisterRequestDto dto, CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(u => u.Username == dto.Username, ct))
            throw new ApplicationException("Username already exists.");

        var user = new User { Username = dto.Username, Role = "User", Active = true };
        user.Password = _hasher.HashPassword(user, dto.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user.Id;
    }


    // Read a user id and return a dto for the api.
    public async Task<ProfileResponseDto?> GetProfileAsync(long userId, CancellationToken ct)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null) return null;

        var dto = new ProfileResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            BirthYear = user.BirthYear,
            Active = user.Active,
            CreatedAtUtc = user.CreatedAtUtc,
            Role = user.Role
        };

        return dto;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest)
    {
        // Try to find the user by username
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.UserName);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid username or password.");
        if (!user.Active)
            throw new UnauthorizedAccessException("User account is inactive.");

        // Verify password
        var verifyResult = _hasher.VerifyHashedPassword(user, user.Password, loginRequest.Password);
        if (verifyResult != PasswordVerificationResult.Success)
            throw new UnauthorizedAccessException("Invalid username or password.");

        var authResponse = CreateJwt(user);
        return authResponse;
    }

    // Builds JWT access token for an user.
    private AuthResponseDto CreateJwt(User u)
    {
        var key = _cfg["Jwt:Key"] ?? "dev-only-change-me";
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                                           SecurityAlgorithms.HmacSha256);
        var minutes = int.TryParse(_cfg["Jwt:Minutes"], out var m) ? m : 60;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
            new Claim(ClaimTypes.Name, u.Username),
            new Claim(ClaimTypes.Role, u.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: creds
        );

        return new AuthResponseDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = token.ValidTo
        };
    }
}
