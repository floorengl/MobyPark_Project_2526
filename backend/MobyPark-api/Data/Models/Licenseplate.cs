public class Licenseplate
{
    public long Id { get; set; }
    public string? LicensePlateName { get; set; }

    
    public List<Session> Sessions { get; set; } = new List<Session>();

}