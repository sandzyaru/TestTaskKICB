using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Application.DTO;

public record AccountView(
    long Id,
    long CustomerId,
    string Number,
    Currency Currency,
    decimal Balance,
    DateTimeOffset? ClosedAt
    );
