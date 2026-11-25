using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Infrastructure.Models;

public class PhoneRow
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public string Number { get; set; } = null!;
    public PhoneType Type { get; set; }
}
