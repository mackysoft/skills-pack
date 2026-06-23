namespace MackySoft.SkillsPack.Hosting.Cli.Common.Text;

internal static class ContractLiteralInputParser
{
    public static bool TryParseTrimmedIgnoreCase<TEnum> (
        string? literal,
        out TEnum value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(literal))
        {
            value = default;
            return false;
        }

        var normalizedLiteral = literal.Trim();
        foreach (var candidateLiteral in ContractLiteralCodec.GetLiterals<TEnum>())
        {
            if (string.Equals(normalizedLiteral, candidateLiteral, StringComparison.OrdinalIgnoreCase))
            {
                return ContractLiteralCodec.TryParse(candidateLiteral, out value);
            }
        }

        value = default;
        return false;
    }
}
