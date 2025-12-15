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
    private readonly IUserRepository _users;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IConfiguration _cfg;

    public AuthService(IUserRepository users, IPasswordHasher<User> hasher, IConfiguration cfg)
        => (_users, _hasher, _cfg) = (users, hasher, cfg);

    public async Task<long> RegisterAsync(RegisterRequestDto dto, CancellationToken ct)
    {
        if (await _users.UsernameExistsAsync(dto.Username, ct))
            throw new ApplicationException("Username already exists.");

        var user = new User { Username = dto.Username, Role = "User", Active = true };
        user.Password = _hasher.HashPassword(user, dto.Password);
        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);
        return user.Id;
    }

    public async Task<ProfileResponseDto?> GetProfileAsync(long userId, CancellationToken ct)
    {
        var user = await _users.GetByIdNoTrackingAsync(userId, ct);
        if (user == null) return null;
        return new ProfileResponseDto
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
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest)
    {
        var user = await _users.GetByUsernameAsync(loginRequest.UserName, CancellationToken.None);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid username or password.");
        if (!user.Active)
            throw new UnauthorizedAccessException("User account is inactive.");
        var verify = _hasher.VerifyHashedPassword(user, user.Password, loginRequest.Password);
        if (verify != PasswordVerificationResult.Success)
            throw new UnauthorizedAccessException("Invalid username or password.");
        return CreateJwt(user);
    }

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
