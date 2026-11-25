using TestTaskKICB.Application.DTO;
using TestTaskKICB.Application.Services;
using TestTaskKICB.Presentation.Api.V1.DTO;

namespace TestTaskKICB.Presentation.Api.V1;

public static class CardsEndpoints
{
    public static void MapCardsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var accountCards = endpoints.MapGroup("/api/accounts/{accountId:long}/cards")
            .WithTags("Cards");
        accountCards.MapGet(
            "",
            ListCards
        )
            .Produces<IReadOnlyList<CardView>>();
        accountCards.MapPost(
            "",
            IssueCard
        )
            .Produces<CardView>(StatusCodes.Status201Created);

        var cards = endpoints.MapGroup("/api/cards").WithTags("Cards");
        cards.MapGet(
            "/{id:long}",
            GetCard
        )
            .WithName(nameof(GetCard))
            .Produces<CardView>();
        cards.MapPut(
            "/{id:long}",
            UpdateCardDetails
        )
            .Produces(StatusCodes.Status204NoContent);
        cards.MapDelete(
            "/{id:long}",
            DeleteCard
        )
            .Produces(StatusCodes.Status204NoContent);
        cards.MapPut(
            "/{id:long}/block",
            BlockCard
        )
            .Produces(StatusCodes.Status204NoContent);
        cards.MapDelete(
            "/{id:long}/block",
            UnblockCard
        )
            .Produces(StatusCodes.Status204NoContent);
    }

    private static IResult ListCards(long accountId, AccountService accounts)
    {
        return ApiErrors.Read(accounts.ListCards(accountId));
    }

    private static IResult GetCard(long id, AccountService accounts)
    {
        return ApiErrors.Read(accounts.GetCard(id));
    }

    private static IResult IssueCard(
        long accountId,
        CreateCard request,
        AccountService accounts
        )
    {
        var input = new CardInput(request.Number, request.ExpirationDate, request.Type);
        var result = accounts.IssueCard(accountId, input);
        if (result.Error != null)
            return ApiErrors.Problem(result.Error);

        var card = result.Value!;
        return TypedResults.CreatedAtRoute(
            card,
            nameof(GetCard),
            new { id = card.Id }
        );
    }

    private static IResult UpdateCardDetails(
        long id,
        UpdateCard request,
        AccountService accounts
        )
    {
        var input = new CardDetailsInput(request.ExpirationDate, request.Type);
        return ApiErrors.NoContent(accounts.UpdateCard(id, input));
    }

    private static IResult DeleteCard(long id, AccountService accounts)
    {
        return ApiErrors.NoContent(accounts.DeleteCard(id));
    }

    private static IResult BlockCard(long id, AccountService accounts)
    {
        return ApiErrors.NoContent(accounts.SetCardBlocked(id, true));
    }

    private static IResult UnblockCard(long id, AccountService accounts)
    {
        return ApiErrors.NoContent(accounts.SetCardBlocked(id, false));
    }
}
