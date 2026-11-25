namespace TestTaskKICB.Infrastructure.Models;

public class CustomerRow
{
    public long Id { get; set; }
    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? Patronymic { get; set; }
    public string Email { get; set; } = null!;
    public string NormalizedEmail { get; set; } = null!;
    public DateOnly DateOfBirth { get; set; }
}
