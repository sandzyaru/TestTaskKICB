using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Application.Validation;

public static class InputValidation
{
    public static BusinessError? CheckText(
        string? value,
        string field,
        int maxLength,
        bool required = true
        )
    {
        if (string.IsNullOrEmpty(value))
            return required ? BusinessError.Invalid(field, "This field is required.") : null;
        if (value.Length > maxLength || value.Contains('\0'))
            return BusinessError.Invalid(
                field,
                $"Use at most {maxLength} characters and omit null characters."
            );
        return null;
    }

    public static BusinessError? CheckNumber(string? number, int maxLength)
    {
        if (string.IsNullOrEmpty(number) || number.Length > maxLength || number.Contains('\0'))
            return BusinessError.Invalid(
                "number",
                $"Provide a number with at most {maxLength} characters and omit null characters."
            );
        return null;
    }
}
