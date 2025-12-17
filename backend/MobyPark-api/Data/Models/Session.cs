public class Session
{
    public long Id { get; set; }
    public long ParkingLotId { get; set; }
    public long? LicensePlateId { get; set; }

    public DateTime Started { get; set; }
    public DateTime? Stopped { get; set; }
    public short? DurationMinutes { get; set; }
    public float? Cost { get; set; }
    public string PlateText { get; set; } = null!;

    public Licenseplate LicensePlate { get; set; } = null!;
}
