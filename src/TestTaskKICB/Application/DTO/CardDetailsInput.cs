namespace TestTaskKICB.Application.DTO;

public record CardDetailsInput(
    DateOnly? ExpirationDate,
    string? Type
    );
