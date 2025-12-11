using MobyPark_api.Enums;

public sealed class RefundPaymentDto
{
    public Guid OriginalPaymentId { get; set; }
    public PaymentStatus Status { get; set; }
}
