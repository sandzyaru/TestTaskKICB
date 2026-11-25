namespace TestTaskKICB.Domain.Models;

public enum FailureCategory { Validation, NotFound, Conflict }

public record BusinessError(
    FailureCategory Category,
    string Code,
    string Message,
    string? Field = null
    )
{
    public static BusinessError Invalid(string field, string message) =>
        new BusinessError(FailureCategory.Validation, "validation_failed", message, field);

    public static BusinessError Missing(string resource) =>
        new BusinessError(
            FailureCategory.NotFound,
            resource + "_not_found",
            $"The requested {resource} was not found."
        );

    public static BusinessError Conflict(string code, string message) =>
        new BusinessError(FailureCategory.Conflict, code, message);

    public static BusinessError CustomerHasAccounts() =>
        Conflict(
            "customer_has_accounts",
            "Customers with accounts on record cannot be deleted."
        );
}
