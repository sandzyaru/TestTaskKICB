using TestTaskKICB.Application.DTO;
using TestTaskKICB.Application.Repositories;
using TestTaskKICB.Application.Validation;
using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Application.Services;

public class AccountService
{
    private readonly IAccountRepository accounts;
    private readonly ICustomerRepository customers;

    public AccountService(IAccountRepository accounts, ICustomerRepository customers)
    {
        this.accounts = accounts;
        this.customers = customers;
    }

    public OperationResult<IReadOnlyList<AccountView>> ListAccounts(long customerId)
    {
        if (!customers.Exists(customerId))
            return OperationResult<IReadOnlyList<AccountView>>.Failed(BusinessError.Missing("customer"));

        var views = accounts.ListAccounts(customerId).Select(View).ToArray();
        return new OperationResult<IReadOnlyList<AccountView>>(views);
    }

    public OperationResult<AccountView> GetAccount(long id)
    {
        var account = accounts.FindAccount(id);
        if (account == null)
            return OperationResult<AccountView>.Failed(BusinessError.Missing("account"));
        return new OperationResult<AccountView>(View(account));
    }

    public OperationResult<AccountView> CreateAccount(long customerId, AccountInput input)
    {
        if (!EnumValue.TryRead<Currency>(input.Currency, out var currency))
            return OperationResult<AccountView>.Failed(BusinessError.Invalid("currency", "Choose KGS, USD, or EUR."));

        var number = input.Number?.Trim();
        var error = InputValidation.CheckNumber(number, 34);
        if (error != null)
            return OperationResult<AccountView>.Failed(error);
        if (!currency.HasValue)
            return OperationResult<AccountView>.Failed(BusinessError.Invalid("currency", "Choose KGS, USD, or EUR."));
        if (!customers.Exists(customerId))
            return OperationResult<AccountView>.Failed(BusinessError.Missing("customer"));

        var account = new Account(customerId, number!.ToUpperInvariant(), currency.Value);
        var saved = accounts.Add(account);
        return new OperationResult<AccountView>(View(saved));
    }

    public OperationResult UpdateAccount(long id, AccountNumberInput input)
    {
        var number = input.Number?.Trim();
        var error = InputValidation.CheckNumber(number, 34);
        if (error != null)
            return new OperationResult(error);

        return ChangeAccount(id, account => account.CorrectNumber(number!.ToUpperInvariant()));
    }

    public OperationResult Close(long id)
    {
        return ChangeAccount(id, account =>
        {
            if (account.ClosedAt.HasValue)
                return null;

            var now = DateTimeOffset.UtcNow;
            var today = DateOnly.FromDateTime(now.UtcDateTime);
            var hasActiveCards = accounts.HasActiveCards(id, today);
            return account.Close(hasActiveCards, now);
        });
    }

    public OperationResult<TransferView> GetTransfer(long id)
    {
        var transfer = accounts.FindTransfer(id);
        if (transfer == null)
            return OperationResult<TransferView>.Failed(BusinessError.Missing("transfer"));
        return new OperationResult<TransferView>(View(transfer));
    }

    public OperationResult<TransferView> Transfer(TransferInput input)
    {
        var sourceId = input.SourceAccountId;
        var destinationId = input.DestinationAccountId;
        var amount = input.Amount;

        if (sourceId <= 0)
            return OperationResult<TransferView>.Failed(BusinessError.Invalid(
                "sourceAccountId",
                "Provide a positive source account ID."
            ));
        if (destinationId <= 0 || sourceId == destinationId)
            return OperationResult<TransferView>.Failed(BusinessError.Invalid(
                "destinationAccountId",
                "Provide a positive destination account ID different from the source."
            ));
        var amountError = Account.ValidateTransferAmount(amount);
        if (amountError != null)
            return OperationResult<TransferView>.Failed(amountError);

        return accounts.InTransaction(() =>
        {
            var lower = accounts.LockAccount(Math.Min(sourceId, destinationId));
            if (lower == null)
                return OperationResult<TransferView>.Failed(BusinessError.Missing("account"));
            var higher = accounts.LockAccount(Math.Max(sourceId, destinationId));
            if (higher == null)
                return OperationResult<TransferView>.Failed(BusinessError.Missing("account"));

            var source = sourceId == lower.Id ? lower : higher;
            var destination = sourceId == lower.Id ? higher : lower;
            var error = source.TransferTo(destination, amount);
            if (error != null)
                return OperationResult<TransferView>.Failed(error);

            var receipt = new Transfer(source, destination, amount, DateTimeOffset.UtcNow);
            var saved = accounts.SaveTransfer(source, destination, receipt);
            return new OperationResult<TransferView>(View(saved));
        });
    }

    public OperationResult<IReadOnlyList<CardView>> ListCards(long accountId)
    {
        if (!accounts.AccountExists(accountId))
            return OperationResult<IReadOnlyList<CardView>>.Failed(BusinessError.Missing("account"));

        var cards = accounts.ListCards(accountId).Select(View).ToArray();
        return new OperationResult<IReadOnlyList<CardView>>(cards);
    }

    public OperationResult<CardView> GetCard(long id)
    {
        var card = accounts.FindCard(id);
        if (card == null)
            return OperationResult<CardView>.Failed(BusinessError.Missing("card"));
        return new OperationResult<CardView>(View(card));
    }

    public OperationResult<CardView> IssueCard(long accountId, CardInput input)
    {
        if (!EnumValue.TryRead<CardType>(input.Type, out var type))
            return OperationResult<CardView>.Failed(BusinessError.Invalid("type", "Choose debit or credit."));

        var number = input.Number?.Trim();
        var numberError = InputValidation.CheckNumber(number, 19);
        if (numberError != null)
            return OperationResult<CardView>.Failed(numberError);

        return accounts.InTransaction(() =>
        {
            var account = accounts.LockAccount(accountId);
            if (account == null)
                return OperationResult<CardView>.Failed(BusinessError.Missing("account"));

            var expirationDate = input.ExpirationDate.GetValueOrDefault();
            var error = ValidateCardInput(input.ExpirationDate, type)
                ?? Card.CheckExpiration(account, expirationDate, Today());
            if (error != null)
                return OperationResult<CardView>.Failed(error);

            var card = new Card(account, number!, expirationDate, type.GetValueOrDefault());
            var saved = accounts.Add(card);
            return new OperationResult<CardView>(View(saved));
        });
    }

    public OperationResult UpdateCard(long id, CardDetailsInput input)
    {
        if (!EnumValue.TryRead<CardType>(input.Type, out var type))
            return new OperationResult(BusinessError.Invalid("type", "Choose debit or credit."));

        return ChangeCard(id, (account, card) =>
        {
            var error = ValidateCardInput(input.ExpirationDate, type);
            if (error != null)
                return new OperationResult(error);

            error = card.Update(
                account,
                input.ExpirationDate.GetValueOrDefault(),
                type.GetValueOrDefault(),
                Today()
            );
            if (error != null)
                return new OperationResult(error);

            accounts.Update(card);
            return new OperationResult();
        });
    }

    public OperationResult DeleteCard(long id)
    {
        return ChangeCard(id, (account, card) =>
        {
            accounts.Remove(card);
            return new OperationResult();
        });
    }

    public OperationResult SetCardBlocked(long id, bool blocked)
    {
        return ChangeCard(id, (account, card) =>
        {
            if (blocked)
            {
                card.Block();
            }
            else
            {
                var error = card.Unblock(account, Today());
                if (error != null)
                    return new OperationResult(error);
            }

            accounts.Update(card);
            return new OperationResult();
        });
    }

    private OperationResult ChangeAccount(long id, Func<Account, BusinessError?> change)
    {
        return accounts.InTransaction(() =>
        {
            var account = accounts.LockAccount(id);
            if (account == null)
                return new OperationResult(BusinessError.Missing("account"));

            var error = change(account);
            if (error != null)
                return new OperationResult(error);

            accounts.Update(account);
            return new OperationResult();
        });
    }

    private OperationResult ChangeCard(long id, Func<Account, Card, OperationResult> change)
    {
        var accountId = accounts.FindCardAccountId(id);
        if (!accountId.HasValue)
            return new OperationResult(BusinessError.Missing("card"));

        return accounts.InTransaction(() =>
        {
            var account = accounts.LockAccount(accountId.Value);
            if (account == null)
                return new OperationResult(BusinessError.Missing("account"));

            var card = accounts.FindCard(id);
            if (card == null)
                return new OperationResult(BusinessError.Missing("card"));
            return change(account, card);
        });
    }

    private static BusinessError? ValidateCardInput(DateOnly? expiration, CardType? type)
    {
        if (!expiration.HasValue || expiration.Value == DateOnly.MaxValue)
            return BusinessError.Invalid("expirationDate", "Provide a valid expiration date.");
        if (!type.HasValue)
            return BusinessError.Invalid("type", "Choose debit or credit.");
        return null;
    }

    private static DateOnly Today()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow);
    }

    private static AccountView View(Account account) =>
        new AccountView(
            account.Id,
            account.CustomerId,
            account.Number,
            account.Currency,
            account.Balance,
            account.ClosedAt
        );

    private static CardView View(Card card)
    {
        var maskedNumber = new string('*', card.Number.Length);
        if (card.Number.Length > 4)
        {
            var hiddenLength = card.Number.Length - 4;
            maskedNumber = new string('*', hiddenLength) + card.Number.Substring(hiddenLength);
        }
        return new CardView(
            card.Id,
            card.AccountId,
            maskedNumber,
            card.ExpirationDate,
            card.Type,
            card.IsBlocked
        );
    }

    private static TransferView View(Transfer transfer) =>
        new TransferView(
            transfer.Id,
            transfer.SourceAccountId,
            transfer.DestinationAccountId,
            transfer.Amount,
            transfer.Currency,
            transfer.CreatedAt
        );
}
