namespace TestTaskKICB.Application.Validation;

public static class EnumValue
{
    public static bool TryRead<T>(string? input, out T? value) where T : struct, Enum
    {
        value = null;
        if (input == null)
            return true;

        var name = input.Trim();
        if (!Enum.TryParse<T>(name, ignoreCase: true, out var parsed) ||
            !string.Equals(name, parsed.ToString(), StringComparison.OrdinalIgnoreCase))
            return false;

        value = parsed;
        return true;
    }
}
