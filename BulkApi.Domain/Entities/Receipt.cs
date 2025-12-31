namespace BulkApi.Domain.Entities;

public sealed class Receipt
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = default!;
    public string Status { get; private set; } = "Draft";
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private Receipt() { }

    public Receipt(string title, decimal amount, string currency)
    {
        Title = title;
        Amount = amount;
        Currency = currency;
        Status = "Draft";
        CreatedAtUtc = DateTime.UtcNow;
    }
}
