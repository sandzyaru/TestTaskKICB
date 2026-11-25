using System.Data;
using Microsoft.EntityFrameworkCore;
using TestTaskKICB.Application.DTO;
using TestTaskKICB.Application.Repositories;
using TestTaskKICB.Domain.Models;
using TestTaskKICB.Infrastructure.DB;
using TestTaskKICB.Infrastructure.Models;

namespace TestTaskKICB.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly BankDbContext db;

    public AccountRepository(BankDbContext db)
    {
        this.db = db;
    }

    public T InTransaction<T>(Func<T> operation) where T : OperationResult
    {
        using var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted);
        db.ChangeTracker.Clear();

        var result = operation();
        if (result.Error == null)
            transaction.Commit();

        db.ChangeTracker.Clear();
        return result;
    }

    public bool HasAccounts(long customerId)
    {
        return db.Accounts.Any(account => account.CustomerId == customerId);
    }

    public bool AccountExists(long id)
    {
        return db.Accounts.Any(account => account.Id == id);
    }

    public IReadOnlyList<Account> ListAccounts(long customerId)
    {
        return db.Accounts
            .AsNoTracking()
            .Where(account => account.CustomerId == customerId)
            .OrderBy(account => account.Id)
            .ToList()
            .Select(row => row.ToDomain())
            .ToArray();
    }

    public Account? FindAccount(long id)
    {
        return db.Accounts.Find(id)?.ToDomain();
    }

    public Account? LockAccount(long id)
    {
        var row = db.Accounts
            .FromSqlInterpolated($"SELECT * FROM accounts.accounts WHERE id = {id} FOR UPDATE")
            .AsEnumerable()
            .SingleOrDefault();
        return row?.ToDomain();
    }

    public Account Add(Account account)
    {
        var row = new AccountRow
        {
            CustomerId = account.CustomerId,
            Number = account.Number,
            Currency = account.Currency,
            Balance = account.Balance,
            ClosedAt = account.ClosedAt
        };
        db.Accounts.Add(row);
        db.SaveChanges();
        return row.ToDomain();
    }

    public void Update(Account account)
    {
        SetAccountFields(account);
        db.SaveChanges();
    }

    public bool HasActiveCards(long accountId, DateOnly today)
    {
        return db.Cards.Any(card =>
            card.AccountId == accountId && !card.IsBlocked && card.ExpirationDate >= today);
    }

    public long? FindCardAccountId(long id)
    {
        return db.Cards
            .Where(card => card.Id == id)
            .Select(card => (long?)card.AccountId)
            .SingleOrDefault();
    }

    public IReadOnlyList<Card> ListCards(long accountId)
    {
        return db.Cards
            .AsNoTracking()
            .Where(card => card.AccountId == accountId)
            .OrderBy(card => card.Id)
            .ToList()
            .Select(row => row.ToDomain())
            .ToArray();
    }

    public Card? FindCard(long id)
    {
        return db.Cards.Find(id)?.ToDomain();
    }

    public Card Add(Card card)
    {
        var row = new CardRow
        {
            AccountId = card.AccountId,
            Number = card.Number,
            ExpirationDate = card.ExpirationDate,
            Type = card.Type,
            IsBlocked = card.IsBlocked
        };
        db.Cards.Add(row);
        db.SaveChanges();
        return row.ToDomain();
    }

    public void Update(Card card)
    {
        var row = db.Cards.Find(card.Id)!;
        row.ExpirationDate = card.ExpirationDate;
        row.Type = card.Type;
        row.IsBlocked = card.IsBlocked;
        db.SaveChanges();
    }

    public void Remove(Card card)
    {
        var row = db.Cards.Find(card.Id)!;
        db.Cards.Remove(row);
        db.SaveChanges();
    }

    public Transfer? FindTransfer(long id)
    {
        return db.Transfers.Find(id)?.ToDomain();
    }

    public Transfer SaveTransfer(
        Account source,
        Account destination,
        Transfer transfer
        )
    {
        SetAccountFields(source);
        SetAccountFields(destination);

        var row = new TransferRow
        {
            SourceAccountId = transfer.SourceAccountId,
            DestinationAccountId = transfer.DestinationAccountId,
            Amount = transfer.Amount,
            Currency = transfer.Currency,
            CreatedAt = transfer.CreatedAt
        };
        db.Transfers.Add(row);
        db.SaveChanges();
        return row.ToDomain();
    }

    private void SetAccountFields(Account account)
    {
        var row = db.Accounts.Find(account.Id)!;
        row.Number = account.Number;
        row.Balance = account.Balance;
        row.ClosedAt = account.ClosedAt;
    }
}
