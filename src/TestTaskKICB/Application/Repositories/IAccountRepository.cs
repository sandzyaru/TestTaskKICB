using TestTaskKICB.Application.DTO;
using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Application.Repositories;

public interface IAccountRepository
{
    T InTransaction<T>(Func<T> operation) where T : OperationResult;

    bool HasAccounts(long customerId);
    bool AccountExists(long id);
    IReadOnlyList<Account> ListAccounts(long customerId);
    Account? FindAccount(long id);
    Account? LockAccount(long id);
    Account Add(Account account);
    void Update(Account account);

    bool HasActiveCards(long accountId, DateOnly today);
    long? FindCardAccountId(long id);
    IReadOnlyList<Card> ListCards(long accountId);
    Card? FindCard(long id);
    Card Add(Card card);
    void Update(Card card);
    void Remove(Card card);

    Transfer? FindTransfer(long id);
    Transfer SaveTransfer(Account source, Account destination, Transfer transfer);
}
