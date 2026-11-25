namespace TestTaskKICB.Application.DTO;

public record CustomerView(
    long Id,
    string LastName,
    string FirstName,
    string? Patronymic,
    string Email,
    DateOnly DateOfBirth
    );
