using Microsoft.AspNetCore.Diagnostics;
using TestTaskKICB.Application.DTO;
using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Presentation.Api.V1;

public static class ApiErrors
{
    public static IResult Read<T>(OperationResult<T> result)
    {
        return result.Error == null ? Results.Ok(result.Value) : Problem(result.Error);
    }

    public static IResult NoContent(OperationResult result)
    {
        return result.Error == null ? Results.NoContent() : Problem(result.Error);
    }

    public static IResult Problem(BusinessError error)
    {
        var extensions = new Dictionary<string, object?> { ["code"] = error.Code };
        if (error.Category == FailureCategory.Validation)
        {
            var errors = new Dictionary<string, string[]> { [error.Field!] = [error.Message] };
            return Results.ValidationProblem(errors, extensions: extensions);
        }

        var status = error.Category == FailureCategory.NotFound
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status409Conflict;
        return Results.Problem(
            statusCode: status,
            detail: error.Message,
            extensions: extensions
        );
    }

    public static Task Write(
        HttpContext context,
        Func<Exception?, BusinessError?> readDatabaseError
        )
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var error = readDatabaseError(exception);
        var result = error == null
            ? Results.Problem(statusCode: StatusCodes.Status500InternalServerError)
            : Problem(error);
        return result.ExecuteAsync(context);
    }
}
