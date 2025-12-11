using System.ComponentModel.DataAnnotations.Schema;

public class TransactionData
{
    public Guid TransactionId{get;set;} = Guid.NewGuid();
    public decimal Amount {get; set;}
    public DateTime Date {get; set;}
    public string Method {get; set;} = "";
    public string Issuer {get; set;} = "";
    public string Bank {get; set;} = "";
    // Foreign key
    
}