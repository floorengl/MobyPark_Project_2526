public sealed class ProfileResponseDto
{
    public long Id { get; init; }
    public string Username { get; init; } = "";
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public short? BirthYear { get; init; }
    public bool Active { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public string Role { get; init; } = "USER";
}
