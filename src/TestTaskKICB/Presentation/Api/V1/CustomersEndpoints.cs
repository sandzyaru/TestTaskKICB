using TestTaskKICB.Application.DTO;
using TestTaskKICB.Application.Services;
using TestTaskKICB.Presentation.Api.V1.DTO;

namespace TestTaskKICB.Presentation.Api.V1;

public static class CustomersEndpoints
{
    public static void MapCustomersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var customers = endpoints.MapGroup("/api/customers").WithTags("Customers");
        customers.MapGet("", ListCustomers);
        customers.MapGet(
            "/{id:long}",
            GetCustomer
        )
            .WithName(nameof(GetCustomer))
            .Produces<CustomerView>();
        customers.MapPost(
            "",
            CreateCustomer
        )
            .Produces<CustomerView>(StatusCodes.Status201Created);
        customers.MapPut(
            "/{id:long}",
            UpdateCustomer
        )
            .Produces(StatusCodes.Status204NoContent);
        customers.MapDelete(
            "/{id:long}",
            DeleteCustomer
        )
            .Produces(StatusCodes.Status204NoContent);
    }

    private static IReadOnlyList<CustomerView> ListCustomers(CustomerService customers)
    {
        return customers.ListCustomers();
    }

    private static IResult GetCustomer(long id, CustomerService customers)
    {
        return ApiErrors.Read(customers.GetCustomer(id));
    }

    private static IResult CreateCustomer(CustomerInput request, CustomerService customers)
    {
        var result = customers.CreateCustomer(ToInput(request));
        if (result.Error != null)
            return ApiErrors.Problem(result.Error);

        var customer = result.Value!;
        return TypedResults.CreatedAtRoute(
            customer,
            nameof(GetCustomer),
            new { id = customer.Id }
        );
    }

    private static IResult UpdateCustomer(
        long id,
        CustomerInput request,
        CustomerService customers
        )
    {
        return ApiErrors.NoContent(customers.UpdateCustomer(id, ToInput(request)));
    }

    private static IResult DeleteCustomer(long id, CustomerService customers)
    {
        return ApiErrors.NoContent(customers.DeleteCustomer(id));
    }

    private static CustomerDetails ToInput(CustomerInput request) =>
        new CustomerDetails(
            request.LastName,
            request.FirstName,
            request.Patronymic,
            request.Email,
            request.DateOfBirth
        );
}
