using MackySoft.AgentSkills.Hosting.Commands;
using MackySoft.AgentSkills.Hosting.Reporting;
using MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal sealed class SkillsPackAgentSkillsCommandResultEmitter : IAgentSkillsCommandResultEmitter
{
    private readonly ICommandResultWriter commandResultWriter;

    public SkillsPackAgentSkillsCommandResultEmitter (ICommandResultWriter commandResultWriter)
    {
        this.commandResultWriter = commandResultWriter ?? throw new ArgumentNullException(nameof(commandResultWriter));
    }

    public ValueTask<int> EmitAsync (
        AgentSkillsCommandResult result,
        AgentSkillsCommandOutputOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        var commandResult = SkillsCommandResultFactory.Create(result);
        commandResultWriter.WriteToStandardOutput(commandResult);
        return ValueTask.FromResult(commandResult.ExitCode);
    }
}
