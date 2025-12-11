using MobyPark_api.Enums;

public sealed class UpdatePaymentDto
{
    public PaymentStatus Status { get; set; }
    public TransactionDataDto Transaction { get; set; } = null!;
}
