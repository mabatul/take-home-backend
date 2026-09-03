namespace LoanApplication.Core.Domain;

public class Application
{
    public Guid Id { get; set; }
    public decimal RequestedAmount { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}