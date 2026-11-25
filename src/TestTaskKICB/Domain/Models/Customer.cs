namespace TestTaskKICB.Domain.Models;

public class Customer
{
    public Customer(
        string lastName,
        string firstName,
        string? patronymic,
        string email,
        DateOnly dateOfBirth
        ) =>
        Update(
            lastName,
            firstName,
            patronymic,
            email,
            dateOfBirth
        );

    public Customer(
        long id,
        string lastName,
        string firstName,
        string? patronymic,
        string email,
        DateOnly dateOfBirth
        ) : this(lastName, firstName, patronymic, email, dateOfBirth)
    {
        Id = id;
    }

    public long Id { get; }
    public string LastName { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string? Patronymic { get; private set; }
    public string Email { get; private set; } = null!;
    public string NormalizedEmail { get; private set; } = null!;
    public DateOnly DateOfBirth { get; private set; }

    public void Update(
        string lastName,
        string firstName,
        string? patronymic,
        string email,
        DateOnly dateOfBirth
        )
    {
        LastName = lastName;
        FirstName = firstName;
        Patronymic = patronymic;
        Email = email;
        NormalizedEmail = NormalizeEmail(email);
        DateOfBirth = dateOfBirth;
    }

    public static string NormalizeEmail(
        string email
        ) => email.ToUpperInvariant();
}
