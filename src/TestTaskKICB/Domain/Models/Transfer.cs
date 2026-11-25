namespace TestTaskKICB.Domain.Models;

public class Transfer
{
    public Transfer(
        Account source,
        Account destination,
        decimal amount,
        DateTimeOffset now
        )
    {
        SourceAccountId = source.Id;
        DestinationAccountId = destination.Id;
        Amount = amount;
        Currency = source.Currency;
        CreatedAt = now;
    }

    public Transfer(
        long id,
        long sourceAccountId,
        long destinationAccountId,
        decimal amount,
        Currency currency,
        DateTimeOffset createdAt
        )
    {
        Id = id;
        SourceAccountId = sourceAccountId;
        DestinationAccountId = destinationAccountId;
        Amount = amount;
        Currency = currency;
        CreatedAt = createdAt;
    }

    public long Id { get; }
    public long SourceAccountId { get; private set; }
    public long DestinationAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public Currency Currency { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
