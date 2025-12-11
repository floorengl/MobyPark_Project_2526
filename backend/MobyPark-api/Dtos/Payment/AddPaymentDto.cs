using MobyPark_api.Enums;

public sealed class AddPaymentDto
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public PaymentStatus Status { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string TData { get; set; } = string.Empty;
}