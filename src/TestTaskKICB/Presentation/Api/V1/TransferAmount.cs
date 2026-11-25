using System.Globalization;
using System.Text.Json;

namespace TestTaskKICB.Presentation.Api.V1;

public static class TransferAmount
{
    public static string? Read(JsonElement input, out decimal amount)
    {
        amount = 0;
        if (input.ValueKind == JsonValueKind.Undefined)
            return null;

        string? value = null;
        if (input.ValueKind == JsonValueKind.Number)
            value = input.GetRawText();
        else if (input.ValueKind == JsonValueKind.String)
            value = input.GetString();
        if (value == null)
            return "Provide a decimal amount.";

        var precisionError = CheckPrecision(value.AsSpan().Trim());
        if (precisionError != null)
            return precisionError;

        if (!decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out amount))
            return "Provide a decimal amount within the supported range.";
        return null;
    }

    private static string? CheckPrecision(ReadOnlySpan<char> value)
    {
        var exponent = 0;
        var exponentIndex = value.IndexOfAny('e', 'E');
        if (exponentIndex >= 0)
        {
            if (!int.TryParse(value.Slice(exponentIndex + 1), NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out exponent))
                return "Provide a valid decimal exponent.";

            value = value.Slice(0, exponentIndex);
        }

        var point = value.IndexOf('.');
        var fractionalDigits = point < 0 ? 0 : value.Length - point - 1;
        var trailingZeros = 0;
        for (var index = value.Length - 1; index >= 0; index--)
        {
            var character = value[index];
            if (character == '0')
            {
                trailingZeros++;
            }
            else if (character >= '1' && character <= '9')
            {
                var decimalPlaces = (long)fractionalDigits - exponent - trailingZeros;
                return decimalPlaces > 2 ? "Provide a transfer amount without fractional cents." : null;
            }
        }

        return null;
    }
}
