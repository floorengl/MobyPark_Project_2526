using MobyPark_api.Enums;

public class Payment
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public PaymentStatus Status { get; set; }
    public string Hash { get; set; } = "";
    public string TData { get; set; } = "";
}