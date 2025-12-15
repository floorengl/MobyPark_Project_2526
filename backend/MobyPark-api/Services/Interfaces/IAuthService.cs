using MobyPark_api.Dtos.Auth;

public interface IAuthService
{
    Task<long> RegisterAsync(RegisterRequestDto dto, CancellationToken ct);
    Task<ProfileResponseDto?> GetProfileAsync(long userId, CancellationToken ct);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest);
}
