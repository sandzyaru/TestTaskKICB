namespace TestTaskKICB.Presentation.Api.V1.DTO;

public record CreateCard(
    string? Number,
    DateOnly? ExpirationDate,
    string? Type
    );
