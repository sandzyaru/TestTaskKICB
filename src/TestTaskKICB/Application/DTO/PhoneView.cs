using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Application.DTO;

public record PhoneView(
    long Id,
    long CustomerId,
    string Number,
    PhoneType Type
    );
