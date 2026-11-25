using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Application.DTO;

public record CardView(
    long Id,
    long AccountId,
    string MaskedNumber,
    DateOnly ExpirationDate,
    CardType Type,
    bool IsBlocked
    );
