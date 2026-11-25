using TestTaskKICB.Application.DTO;
using TestTaskKICB.Application.Services;
using TestTaskKICB.Domain.Models;
using TestTaskKICB.Presentation.Api.V1.DTO;

namespace TestTaskKICB.Presentation.Api.V1;

public static class AccountsEndpoints
{
    public static void MapAccountsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var customerAccounts = endpoints.MapGroup("/api/customers/{customerId:long}/accounts")
            .WithTags("Accounts");
        customerAccounts.MapGet(
            "",
            ListAccounts
        )
            .Produces<IReadOnlyList<AccountView>>();
        customerAccounts.MapPost(
            "",
            CreateAccountForCustomer
        )
            .Produces<AccountView>(StatusCodes.Status201Created);

        var accounts = endpoints.MapGroup("/api/accounts").WithTags("Accounts");
        accounts.MapGet(
            "/{id:long}",
            GetAccount
        )
            .WithName(nameof(GetAccount))
            .Produces<AccountView>();
        accounts.MapPut(
            "/{id:long}",
            UpdateAccountNumber
        )
            .Produces(StatusCodes.Status204NoContent);
        accounts.MapDelete(
            "/{id:long}",
            CloseAccount
        )
            .Produces(StatusCodes.Status204NoContent);

        var transfers = endpoints.MapGroup("/api/transfers").WithTags("Transfers");
        transfers.MapGet(
            "/{id:long}",
            GetTransfer
        )
            .WithName(nameof(GetTransfer))
            .Produces<TransferView>();
        transfers.MapPost(
            "",
            Transfer
        )
            .Produces<TransferView>(StatusCodes.Status201Created);
    }

    private static IResult ListAccounts(long customerId, AccountService accounts)
    {
        return ApiErrors.Read(accounts.ListAccounts(customerId));
    }

    private static IResult GetAccount(long id, AccountService accounts)
    {
        return ApiErrors.Read(accounts.GetAccount(id));
    }

    private static IResult CreateAccountForCustomer(
        long customerId,
        CreateAccount request,
        AccountService accounts
        )
    {
        var input = new AccountInput(request.Number, request.Currency);
        var result = accounts.CreateAccount(customerId, input);
        if (result.Error != null)
            return ApiErrors.Problem(result.Error);

        var account = result.Value!;
        return TypedResults.CreatedAtRoute(
            account,
            nameof(GetAccount),
            new { id = account.Id }
        );
    }

    private static IResult UpdateAccountNumber(
        long id,
        UpdateAccount request,
        AccountService accounts
        )
    {
        var input = new AccountNumberInput(request.Number);
        return ApiErrors.NoContent(accounts.UpdateAccount(id, input));
    }

    private static IResult CloseAccount(long id, AccountService accounts)
    {
        return ApiErrors.NoContent(accounts.Close(id));
    }

    private static IResult GetTransfer(long id, AccountService accounts)
    {
        return ApiErrors.Read(accounts.GetTransfer(id));
    }

    private static IResult Transfer(TransferRequest request, AccountService accounts)
    {
        var amountError = TransferAmount.Read(request.Amount, out var amount);
        if (amountError != null)
            return ApiErrors.Problem(BusinessError.Invalid("amount", amountError));

        var input = new TransferInput(
            request.SourceAccountId,
            request.DestinationAccountId,
            amount
        );
        var result = accounts.Transfer(input);
        if (result.Error != null)
            return ApiErrors.Problem(result.Error);

        var receipt = result.Value!;
        return TypedResults.CreatedAtRoute(
            receipt,
            nameof(GetTransfer),
            new { id = receipt.Id }
        );
    }
}
