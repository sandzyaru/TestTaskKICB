using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Application.DTO;

public record TransferView(
    long Id,
    long SourceAccountId,
    long DestinationAccountId,
    decimal Amount,
    Currency Currency,
    DateTimeOffset CreatedAt
    );
