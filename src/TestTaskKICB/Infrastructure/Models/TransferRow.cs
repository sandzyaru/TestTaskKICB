using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Infrastructure.Models;

public class TransferRow
{
    public long Id { get; set; }
    public long SourceAccountId { get; set; }
    public long DestinationAccountId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
