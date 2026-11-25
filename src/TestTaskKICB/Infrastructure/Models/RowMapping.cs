using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Infrastructure.Models;

public static class RowMapping
{
    public static Customer ToDomain(this CustomerRow row) =>
        new Customer(
            row.Id,
            row.LastName,
            row.FirstName,
            row.Patronymic,
            row.Email,
            row.DateOfBirth
        );

    public static Phone ToDomain(this PhoneRow row) =>
        new Phone(
            row.Id,
            row.CustomerId,
            row.Number,
            row.Type
        );

    public static Account ToDomain(this AccountRow row) =>
        new Account(
            row.Id,
            row.CustomerId,
            row.Number,
            row.Currency,
            row.Balance,
            row.ClosedAt
        );

    public static Card ToDomain(this CardRow row) =>
        new Card(
            row.Id,
            row.AccountId,
            row.Number,
            row.ExpirationDate,
            row.Type,
            row.IsBlocked
        );

    public static Transfer ToDomain(this TransferRow row) =>
        new Transfer(
            row.Id,
            row.SourceAccountId,
            row.DestinationAccountId,
            row.Amount,
            row.Currency,
            row.CreatedAt
        );
}
