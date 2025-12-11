public class TransactionDataDto
{
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Method { get; set; } = "Ideal";
    public string Issuer { get; set; } = "XYY910HH";
    public string Bank { get; set; } = "ABN-NL";
}