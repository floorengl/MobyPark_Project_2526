using MobyPark_api.Data.Models;

public class User
{
    public long Id { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = "USER";
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public short? BirthYear { get; set; }
    public bool Active { get; set; } = true;

    public IList<Vehicle>? Vehicles { get; set; }
}
