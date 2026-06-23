using MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

namespace MackySoft.SkillsPack;

internal static class Program
{
    private static async Task<int> Main (string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var runner = new CliExecutionRunner();
        return await runner.RunAsync(args).ConfigureAwait(false);
    }
}
