using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Infrastructure.Models;

public class CardRow
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public string Number { get; set; } = null!;
    public DateOnly ExpirationDate { get; set; }
    public CardType Type { get; set; }
    public bool IsBlocked { get; set; }
}
