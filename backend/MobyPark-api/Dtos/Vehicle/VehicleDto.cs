namespace MobyPark_api.Dtos.Vehicle
{
    public sealed class VehicleDto
    {
        public long Id { get; init; }
        public string LicensePlate { get; init; } = "";
        public string? Make { get; init; }
        public string? Model { get; init; }
        public string? Color { get; init; }
        public DateTime? Year { get; init; }
        public long UserId { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}
