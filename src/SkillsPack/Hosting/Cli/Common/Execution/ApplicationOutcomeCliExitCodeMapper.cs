using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;

namespace MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

internal static class ApplicationOutcomeCliExitCodeMapper
{
    public static int ToExitCode (ApplicationOutcome outcome)
    {
        return outcome switch
        {
            ApplicationOutcome.Success => (int)CliExitCode.Success,
            ApplicationOutcome.InvalidArgument => (int)CliExitCode.InvalidArgument,
            ApplicationOutcome.ToolError => (int)CliExitCode.ToolError,
            _ => (int)CliExitCode.ToolError,
        };
    }
}
