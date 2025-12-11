using MobyPark_api.Enums;
using System.ComponentModel.DataAnnotations.Schema;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Amount {get; set;}
    public DateTime CreatedAt { get; set; }
    public PaymentStatus Status { get; set; }
    public string Hash { get; set; } = "";
    public Guid TransactionId {get; set;}

    [ForeignKey(nameof(TransactionId))]
    public TransactionData Transaction{get; set;} = null!;
    
}