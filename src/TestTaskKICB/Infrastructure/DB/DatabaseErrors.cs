using Microsoft.EntityFrameworkCore;
using Npgsql;
using TestTaskKICB.Domain.Models;
using TestTaskKICB.Infrastructure.Models;

namespace TestTaskKICB.Infrastructure.DB;

public static class DatabaseErrors
{
    public static BusinessError? Read(Exception? exception)
    {
        if (exception is not DbUpdateException update)
            return null;

        if (update is DbUpdateConcurrencyException)
        {
            if (update.Entries.Any(entry => entry.Entity is CustomerRow))
                return BusinessError.Missing("customer");
            if (update.Entries.Any(entry => entry.Entity is PhoneRow))
                return BusinessError.Missing("phone");
        }

        if (update.InnerException is not PostgresException postgres)
            return null;

        if (postgres.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            if (postgres.ConstraintName == "fk_phones_customers")
                return BusinessError.Missing("customer");
            if (postgres.ConstraintName == "fk_accounts_customers")
            {
                if (update.Entries.Any(entry => entry.Entity is CustomerRow))
                    return BusinessError.CustomerHasAccounts();
                return BusinessError.Missing("customer");
            }
        }

        if (postgres.SqlState != PostgresErrorCodes.UniqueViolation)
            return null;

        switch (postgres.ConstraintName)
        {
            case "ux_customers_normalized_email":
                return BusinessError.Conflict(
                    "email_in_use",
                    "That email is already assigned to a customer."
                );
            case "ux_phones_customer_number":
                return BusinessError.Conflict(
                    "phone_in_use",
                    "This customer already has that phone number."
                );
            case "ux_accounts_number":
                return BusinessError.Conflict(
                    "account_number_in_use",
                    "That account number is already in use."
                );
            case "ux_cards_number":
                return BusinessError.Conflict(
                    "card_number_in_use",
                    "That card number is already in use."
                );
            default:
                return null;
        }
    }

}
