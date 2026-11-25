using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Infrastructure.Models;

public class AccountRow
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public string Number { get; set; } = null!;
    public Currency Currency { get; set; }
    public decimal Balance { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
}
