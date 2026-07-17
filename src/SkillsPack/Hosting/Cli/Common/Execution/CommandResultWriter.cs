using System.Text.Json;
using MackySoft.AgentSkills.Shared.Text;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;

namespace MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

internal sealed class CommandResultWriter : ICommandResultWriter
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public void WriteToStandardOutput (CommandResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        Console.Out.WriteLine(JsonSerializer.Serialize(result, Options));
    }

    private static JsonSerializerOptions CreateOptions ()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
        };
        options.Converters.Add(new ContractLiteralJsonConverterFactory());
        return options;
    }
}
