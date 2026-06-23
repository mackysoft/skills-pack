using System.Text.Json.Serialization;

namespace MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;

[JsonConverter(typeof(SkillsPackCodeJsonConverter))]
internal readonly record struct SkillsPackCode
{
    public const int MaximumLength = 128;

    public SkillsPackCode (string value)
    {
        if (!IsValidValue(value))
        {
            throw new ArgumentException(InvalidValueMessage, nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public bool IsValid => IsValidValue(Value);

    public static string InvalidValueMessage => $"Code must be an uppercase contract token up to {MaximumLength} characters using letters, digits, underscores, and optional dot-separated segments.";

    public static bool TryCreate (
        string? value,
        out SkillsPackCode code)
    {
        if (!IsValidValue(value))
        {
            code = default;
            return false;
        }

        code = new SkillsPackCode(value!);
        return true;
    }

    public static bool IsValidValue (string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaximumLength)
        {
            return false;
        }

        var segmentStart = true;
        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            if (segmentStart)
            {
                if (!IsUppercaseAsciiLetter(character))
                {
                    return false;
                }

                segmentStart = false;
                continue;
            }

            if (character == '.')
            {
                segmentStart = true;
                continue;
            }

            if (!IsUppercaseAsciiLetter(character) && !IsAsciiDigit(character) && character != '_')
            {
                return false;
            }
        }

        return !segmentStart;
    }

    public static implicit operator string (SkillsPackCode code)
    {
        return code.ToString();
    }

    public override string ToString ()
    {
        return Value ?? string.Empty;
    }

    private static bool IsUppercaseAsciiLetter (char character)
    {
        return character >= 'A' && character <= 'Z';
    }

    private static bool IsAsciiDigit (char character)
    {
        return character >= '0' && character <= '9';
    }
}
