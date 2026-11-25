namespace TestTaskKICB.Domain.Models;

public enum PhoneType { Mobile, Home, Work }

public class Phone
{
    public Phone(
        long customerId,
        string number,
        PhoneType type
        )
    {
        CustomerId = customerId;
        Update(number, type);
    }

    public Phone(
        long id,
        long customerId,
        string number,
        PhoneType type
        ) : this(customerId, number, type)
    {
        Id = id;
    }

    public long Id { get; }
    public long CustomerId { get; private set; }
    public string Number { get; private set; } = null!;
    public PhoneType Type { get; private set; }

    public void Update(string number, PhoneType type)
    {
        Number = number;
        Type = type;
    }
}
