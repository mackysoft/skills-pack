using System.Text.Json;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;

namespace MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

internal sealed class CommandResultWriter : ICommandResultWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public void WriteToStandardOutput (CommandResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        Console.Out.WriteLine(JsonSerializer.Serialize(result, Options));
    }
}
