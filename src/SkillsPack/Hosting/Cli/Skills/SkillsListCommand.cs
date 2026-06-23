using ConsoleAppFramework;
using MackySoft.AgentSkills.Distribution;
using MackySoft.AgentSkills.Hosts.Registration;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal sealed class SkillsListCommand
{
    private readonly ICommandResultWriter commandResultWriter;
    private readonly SkillHostAdapterSet hostAdapters;
    private readonly SkillPackageProvider packageProvider;

    public SkillsListCommand (
        SkillPackageProvider packageProvider,
        SkillHostAdapterSet hostAdapters,
        ICommandResultWriter commandResultWriter)
    {
        this.packageProvider = packageProvider ?? throw new ArgumentNullException(nameof(packageProvider));
        this.hostAdapters = hostAdapters ?? throw new ArgumentNullException(nameof(hostAdapters));
        this.commandResultWriter = commandResultWriter ?? throw new ArgumentNullException(nameof(commandResultWriter));
    }

    [Command(SkillsPackCommandNames.ListSubcommand)]
    public async Task<int> ListAsync (
        string[]? tier = null,
        string[]? skill = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var packageSelection = SkillsCommandOptionNormalizer.NormalizeOptionalPackageSelection(
            SkillsPackCommandNames.SkillsList,
            tier,
            skill,
            out var errorResult);
        if (errorResult is not null)
        {
            commandResultWriter.WriteToStandardOutput(errorResult);
            return errorResult.ExitCode;
        }

        var catalogResult = await packageProvider.GetPackageCatalogAsync(
                SkillsPackSkillTierLiterals.Defined,
                packageSelection!.Tiers,
                packageSelection.SkillNames,
                cancellationToken)
            .ConfigureAwait(false);
        var commandResult = catalogResult.IsSuccess
            ? SkillsCommandResultFactory.CreateList(catalogResult.Value!, hostAdapters)
            : SkillsCommandResultFactory.CreateSkillFailure(SkillsPackCommandNames.SkillsList, catalogResult.Failure!);
        commandResultWriter.WriteToStandardOutput(commandResult);
        return commandResult.ExitCode;
    }
}
