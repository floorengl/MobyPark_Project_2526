using MobyPark_api.Enums;

public sealed class UpdatePaymentDto
{
    public string Hash { get; set; } = string.Empty;
    public string TData { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
}
