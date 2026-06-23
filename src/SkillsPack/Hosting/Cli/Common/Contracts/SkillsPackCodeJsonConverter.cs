using System.Text.Json;
using System.Text.Json.Serialization;

namespace MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;

internal sealed class SkillsPackCodeJsonConverter : JsonConverter<SkillsPackCode>
{
    public override SkillsPackCode Read (
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return new SkillsPackCode(value ?? string.Empty);
    }

    public override void Write (
        Utf8JsonWriter writer,
        SkillsPackCode value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
