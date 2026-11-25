using TestTaskKICB.Application.DTO;
using TestTaskKICB.Application.Services;
using TestTaskKICB.Presentation.Api.V1.DTO;

namespace TestTaskKICB.Presentation.Api.V1;

public static class PhonesEndpoints
{
    public static void MapPhonesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var customerPhones = endpoints.MapGroup("/api/customers/{customerId:long}/phones")
            .WithTags("Phones");
        customerPhones.MapGet(
            "",
            ListPhones
        )
            .Produces<IReadOnlyList<PhoneView>>();
        customerPhones.MapPost(
            "",
            CreatePhone
        )
            .Produces<PhoneView>(StatusCodes.Status201Created);

        var phones = endpoints.MapGroup("/api/phones").WithTags("Phones");
        phones.MapGet(
            "/{id:long}",
            GetPhone
        )
            .WithName(nameof(GetPhone))
            .Produces<PhoneView>();
        phones.MapPut(
            "/{id:long}",
            UpdatePhone
        )
            .Produces(StatusCodes.Status204NoContent);
        phones.MapDelete(
            "/{id:long}",
            DeletePhone
        )
            .Produces(StatusCodes.Status204NoContent);
    }

    private static IResult ListPhones(long customerId, CustomerService customers)
    {
        return ApiErrors.Read(customers.ListPhones(customerId));
    }

    private static IResult GetPhone(long id, CustomerService customers)
    {
        return ApiErrors.Read(customers.GetPhone(id));
    }

    private static IResult CreatePhone(
        long customerId,
        PhoneRequest request,
        CustomerService customers
        )
    {
        var input = new PhoneInput(request.Number, request.Type);
        var result = customers.CreatePhone(customerId, input);
        if (result.Error != null)
            return ApiErrors.Problem(result.Error);

        var phone = result.Value!;
        return TypedResults.CreatedAtRoute(
            phone,
            nameof(GetPhone),
            new { id = phone.Id }
        );
    }

    private static IResult UpdatePhone(
        long id,
        PhoneRequest request,
        CustomerService customers
        )
    {
        var input = new PhoneInput(request.Number, request.Type);
        return ApiErrors.NoContent(customers.UpdatePhone(id, input));
    }

    private static IResult DeletePhone(long id, CustomerService customers)
    {
        return ApiErrors.NoContent(customers.DeletePhone(id));
    }
}
