namespace TestTaskKICB.Domain.Models;

public enum CardType { Debit, Credit }

public class Card
{
    public Card(
        Account account,
        string number,
        DateOnly expirationDate,
        CardType type
        )
    {
        AccountId = account.Id;
        Number = number;
        ExpirationDate = expirationDate;
        Type = type;
    }

    public Card(
        long id,
        long accountId,
        string number,
        DateOnly expirationDate,
        CardType type,
        bool isBlocked
        )
    {
        Id = id;
        AccountId = accountId;
        Number = number;
        ExpirationDate = expirationDate;
        Type = type;
        IsBlocked = isBlocked;
    }

    public long Id { get; }
    public long AccountId { get; private set; }
    public string Number { get; private set; } = null!;
    public DateOnly ExpirationDate { get; private set; }
    public CardType Type { get; private set; }
    public bool IsBlocked { get; private set; }

    public BusinessError? Update(
        Account account,
        DateOnly expirationDate,
        CardType type,
        DateOnly today
        )
    {
        var error = CheckExpiration(account, expirationDate, today);
        if (error != null)
            return error;

        ExpirationDate = expirationDate;
        Type = type;
        return null;
    }

    public void Block() => IsBlocked = true;

    public BusinessError? Unblock(Account account, DateOnly today)
    {
        var error = account.CheckOpen();
        if (error != null)
            return error;
        if (ExpirationDate < today)
            return BusinessError.Conflict(
                "card_expired",
                "An expired card cannot be unblocked."
            );
        IsBlocked = false;
        return null;
    }

    public static BusinessError? CheckExpiration(
        Account account,
        DateOnly expirationDate,
        DateOnly today
        )
    {
        var error = account.CheckOpen();
        if (error != null)
            return error;
        if (expirationDate < today)
            return BusinessError.Invalid(
                "expirationDate",
                "The expiration date cannot be in the past."
            );
        return null;
    }
}
