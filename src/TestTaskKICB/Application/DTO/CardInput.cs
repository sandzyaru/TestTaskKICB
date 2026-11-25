namespace TestTaskKICB.Application.DTO;

public record CardInput(
    string? Number,
    DateOnly? ExpirationDate,
    string? Type
    );
