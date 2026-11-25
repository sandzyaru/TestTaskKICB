namespace TestTaskKICB.Presentation.Api.V1.DTO;

public record CustomerInput(
    string? LastName,
    string? FirstName,
    string? Patronymic,
    string? Email,
    DateOnly? DateOfBirth
    );
