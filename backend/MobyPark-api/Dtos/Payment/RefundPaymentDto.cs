using MobyPark_api.Enums;

public sealed class RefundPaymentDto
{
    public string TData { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
}
