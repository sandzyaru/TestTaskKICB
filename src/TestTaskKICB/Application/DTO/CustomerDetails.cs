namespace TestTaskKICB.Application.DTO;

public record CustomerDetails(
    string? LastName,
    string? FirstName,
    string? Patronymic,
    string? Email,
    DateOnly? DateOfBirth
    );
