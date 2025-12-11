using MobyPark_api.Enums;

public sealed class AddPaymentDto
{
    public decimal Amount {get; set;}
    public DateTime CreatedAt { get; set; }
    public PaymentStatus Status { get; set; }
    public string? Hash {get; set;}
    public TransactionDataDto Transaction { get; set; } = null!;
}