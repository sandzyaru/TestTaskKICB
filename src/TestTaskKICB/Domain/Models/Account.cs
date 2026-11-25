namespace TestTaskKICB.Domain.Models;

public enum Currency { KGS, USD, EUR }

public class Account
{
    public const decimal MaxBalance = 99_999_999_999_999_999.99m;

    public Account(long customerId, string number, Currency currency)
    {
        CustomerId = customerId;
        Number = number;
        Currency = currency;
    }

    public Account(
        long id,
        long customerId,
        string number,
        Currency currency,
        decimal balance,
        DateTimeOffset? closedAt
        ) : this(customerId, number, currency)
    {
        Id = id;
        Balance = balance;
        ClosedAt = closedAt;
    }

    public long Id { get; }
    public long CustomerId { get; private set; }
    public string Number { get; private set; } = null!;
    public Currency Currency { get; private set; }
    public decimal Balance { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }

    public BusinessError? CheckOpen()
    {
        if (ClosedAt is not null)
            return BusinessError.Conflict(
                "account_closed",
                "The account is closed."
            );
        return null;
    }

    public BusinessError? CorrectNumber(string number)
    {
        var error = CheckOpen();
        if (error != null)
            return error;
        Number = number;
        return null;
    }

    public static BusinessError? ValidateTransferAmount(decimal amount)
    {
        if (amount <= 0 || amount > MaxBalance || decimal.Round(amount, 2) != amount)
            return BusinessError.Invalid(
                "amount",
                "Use a positive amount within the supported range and without fractional cents."
            );
        return null;
    }

    public BusinessError? TransferTo(Account destination, decimal amount)
    {
        var amountError = ValidateTransferAmount(amount);
        if (amountError != null)
            return amountError;
        if (Id == destination.Id)
            return BusinessError.Invalid(
                "destinationAccountId",
                "Choose a different destination account."
            );
        var accountError = CheckOpen() ?? destination.CheckOpen();
        if (accountError != null)
            return accountError;
        if (Currency != destination.Currency)
            return BusinessError.Conflict(
                "currency_mismatch",
                "Both accounts must use the same currency."
            );
        if (Balance < amount)
            return BusinessError.Conflict(
                "insufficient_funds",
                "The source account has insufficient funds."
            );
        if (destination.Balance > MaxBalance - amount)
            return BusinessError.Conflict(
                "balance_limit",
                "The transfer exceeds the destination balance limit."
            );

        Balance -= amount;
        destination.Balance += amount;
        return null;
    }

    public BusinessError? Close(
        bool hasActiveCards,
        DateTimeOffset now
        )
    {
        if (ClosedAt is not null)
            return null;
        if (Balance != 0)
            return BusinessError.Conflict(
                "nonzero_balance",
                "The account must have a zero balance."
            );
        if (hasActiveCards)
            return BusinessError.Conflict(
                "active_cards",
                "Block or remove active cards before closing the account."
            );

        ClosedAt = now;
        return null;
    }
}
