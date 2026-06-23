using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MackySoft.SkillsPack.Hosting.Cli.Common.Text;

internal static class ContractLiteralCodec
{
    public static string ToValue<TEnum> (TEnum value)
        where TEnum : struct, Enum
    {
        if (Cache<TEnum>.Table.TryToValue(value, out var literal))
        {
            return literal;
        }

        throw new ArgumentOutOfRangeException(nameof(value), value, $"Unsupported {typeof(TEnum).Name} value.");
    }

    public static bool TryParse<TEnum> (
        string? literal,
        out TEnum value)
        where TEnum : struct, Enum
    {
        return Cache<TEnum>.Table.TryParse(literal, out value);
    }

    public static IReadOnlyList<string> GetLiterals<TEnum> ()
        where TEnum : struct, Enum
    {
        return Cache<TEnum>.Table.Literals;
    }

    private static class Cache<TEnum>
        where TEnum : struct, Enum
    {
        private static readonly Lazy<Table<TEnum>> TableSource = new(Build);

        public static Table<TEnum> Table => TableSource.Value;

        private static Table<TEnum> Build ()
        {
            var enumType = typeof(TEnum);
            var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            Array.Sort(fields, static (left, right) => left.MetadataToken.CompareTo(right.MetadataToken));

            var valueToLiteral = new Dictionary<TEnum, string>();
            var literalToValue = new Dictionary<string, TEnum>(StringComparer.Ordinal);
            var literals = new List<string>(fields.Length);
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<ContractLiteralAttribute>(inherit: false);
                if (attribute is null)
                {
                    throw new InvalidOperationException($"Enum member '{enumType.FullName}.{field.Name}' is missing ContractLiteralAttribute.");
                }

                var literal = attribute.Literal;
                if (string.IsNullOrWhiteSpace(literal))
                {
                    throw new InvalidOperationException($"Enum member '{enumType.FullName}.{field.Name}' has an empty contract literal.");
                }

                if (literal.Length != literal.Trim().Length)
                {
                    throw new InvalidOperationException($"Enum member '{enumType.FullName}.{field.Name}' has a contract literal with leading or trailing whitespace.");
                }

                var value = (TEnum)field.GetValue(null)!;
                if (valueToLiteral.ContainsKey(value))
                {
                    throw new InvalidOperationException($"Enum type '{enumType.FullName}' defines duplicate enum value '{value}'.");
                }

                if (literalToValue.ContainsKey(literal))
                {
                    throw new InvalidOperationException($"Enum type '{enumType.FullName}' defines duplicate contract literal '{literal}'.");
                }

                valueToLiteral.Add(value, literal);
                literalToValue.Add(literal, value);
                literals.Add(literal);
            }

            return new Table<TEnum>(valueToLiteral, literalToValue, literals.AsReadOnly());
        }
    }

    private sealed class Table<TEnum>
        where TEnum : struct, Enum
    {
        private readonly Dictionary<string, TEnum> literalToValue;
        private readonly IReadOnlyList<string> literals;
        private readonly Dictionary<TEnum, string> valueToLiteral;

        public Table (
            Dictionary<TEnum, string> valueToLiteral,
            Dictionary<string, TEnum> literalToValue,
            IReadOnlyList<string> literals)
        {
            this.valueToLiteral = valueToLiteral;
            this.literalToValue = literalToValue;
            this.literals = literals;
        }

        public IReadOnlyList<string> Literals => literals;

        public bool TryToValue (
            TEnum value,
            [NotNullWhen(true)]
            out string? literal)
        {
            return valueToLiteral.TryGetValue(value, out literal);
        }

        public bool TryParse (
            string? literal,
            out TEnum value)
        {
            if (literal is null)
            {
                value = default;
                return false;
            }

            return literalToValue.TryGetValue(literal, out value);
        }

    }
}
