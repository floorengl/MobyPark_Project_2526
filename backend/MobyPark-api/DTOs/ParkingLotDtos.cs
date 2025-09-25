namespace MobyPark_api.DTOs
{
    public record CoordinatesDto(double Lat, double Lng);

    public record CreateParkingLotDto(
        string Name,
        string? Location,
        string? Address,
        int Capacity,
        decimal? Tariff,
        decimal? DayTariff,
        CoordinatesDto Coordinates
    );

    public record UpdateParkingLotDto(
        string Name,
        string? Location,
        string? Address,
        int Capacity,
        decimal? Tariff,
        decimal? DayTariff,
        CoordinatesDto Coordinates
    );

    public record ParkingLotReadDto(
        long Id,
        string Name,
        string? Location,
        string? Address,
        int Capacity,
        decimal? Tariff,
        decimal? DayTariff,
        System.DateTimeOffset CreatedAt,
        CoordinatesDto Coordinates
    );
}
