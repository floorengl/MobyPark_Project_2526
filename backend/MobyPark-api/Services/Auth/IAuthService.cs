public interface IAuthService
{
    Task<long> RegisterAsync(RegisterRequestDto dto, CancellationToken ct);
    Task<ProfileResponseDto?> GetProfileAsync(long userId, CancellationToken ct);
}
