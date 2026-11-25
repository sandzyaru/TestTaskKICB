using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Application.DTO;

public record OperationResult(
    BusinessError? Error = null
    );

public record OperationResult<T>(
    T? Value,
    BusinessError? Error = null
    ) : OperationResult(Error)
{
    public static OperationResult<T> Failed(BusinessError error)
    {
        return new OperationResult<T>(default, error);
    }
}
