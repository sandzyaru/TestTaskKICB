namespace TestTaskKICB.Presentation.Api.V1.DTO;

public record UpdateCard(
    DateOnly? ExpirationDate,
    string? Type
    );
