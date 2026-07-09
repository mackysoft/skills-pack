using MackySoft.AgentSkills.Hosting.Commands;
using MackySoft.AgentSkills.Hosting.Reporting;
using MackySoft.AgentSkills.Hosts.Registration;
using MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal sealed class SkillsPackAgentSkillsCommandResultEmitter : IAgentSkillsCommandResultEmitter
{
    private readonly ICommandResultWriter commandResultWriter;
    private readonly SkillHostAdapterSet hostAdapters;

    public SkillsPackAgentSkillsCommandResultEmitter (
        ICommandResultWriter commandResultWriter,
        SkillHostAdapterSet hostAdapters)
    {
        this.commandResultWriter = commandResultWriter ?? throw new ArgumentNullException(nameof(commandResultWriter));
        this.hostAdapters = hostAdapters ?? throw new ArgumentNullException(nameof(hostAdapters));
    }

    public ValueTask<int> EmitAsync (
        AgentSkillsCommandResult result,
        AgentSkillsCommandOutputOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        var commandResult = SkillsCommandResultFactory.Create(result, hostAdapters);
        commandResultWriter.WriteToStandardOutput(commandResult);
        return ValueTask.FromResult(commandResult.ExitCode);
    }
}
