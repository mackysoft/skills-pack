using ConsoleAppFramework;
using MackySoft.AgentSkills.Catalogs;
using MackySoft.AgentSkills.Distribution;
using MackySoft.AgentSkills.Hosts.Registration;
using MackySoft.AgentSkills.Installation.Requests;
using MackySoft.AgentSkills.Installation.Services;
using MackySoft.AgentSkills.Installation.Targeting;
using MackySoft.AgentSkills.Names;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal sealed class SkillsPruneCommand
{
    private readonly ICommandResultWriter commandResultWriter;
    private readonly SkillHostAdapterSet hostAdapters;
    private readonly SkillPackageProvider packageProvider;
    private readonly SkillPruneService pruneService;

    public SkillsPruneCommand (
        SkillPackageProvider packageProvider,
        SkillHostAdapterSet hostAdapters,
        SkillPruneService pruneService,
        ICommandResultWriter commandResultWriter)
    {
        this.packageProvider = packageProvider ?? throw new ArgumentNullException(nameof(packageProvider));
        this.hostAdapters = hostAdapters ?? throw new ArgumentNullException(nameof(hostAdapters));
        this.pruneService = pruneService ?? throw new ArgumentNullException(nameof(pruneService));
        this.commandResultWriter = commandResultWriter ?? throw new ArgumentNullException(nameof(commandResultWriter));
    }

    [Command(SkillsPackCommandNames.PruneSubcommand)]
    public async Task<int> PruneAsync (
        string? host = null,
        string? scope = null,
        string? repoRoot = null,
        string? targetDir = null,
        bool dryRun = false,
        bool force = false,
        string[]? tier = null,
        string[]? skill = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var target = SkillsTargetOptionResolver.ResolvePrune(
            SkillsPackCommandNames.SkillsPrune,
            host,
            scope,
            repoRoot,
            targetDir,
            tier,
            skill,
            hostAdapters,
            out var errorResult);
        if (errorResult is not null)
        {
            commandResultWriter.WriteToStandardOutput(errorResult);
            return errorResult.ExitCode;
        }

        var catalogResult = await packageProvider.GetPackageCatalogAsync(SkillsPackSkillTierLiterals.Defined, cancellationToken).ConfigureAwait(false);
        if (!catalogResult.IsSuccess)
        {
            var packageErrorResult = SkillsCommandResultFactory.CreateSkillFailure(SkillsPackCommandNames.SkillsPrune, catalogResult.Failure!);
            commandResultWriter.WriteToStandardOutput(packageErrorResult);
            return packageErrorResult.ExitCode;
        }

        var pruneResult = await pruneService.PruneAsync(
                new SkillPruneInput(
                    new SkillCatalogId(SkillsPackSkillCatalogLiterals.Official),
                    catalogResult.Value!.Packages,
                    new SkillInstallRequest(target!.Host, target.Scope, target.RepositoryRoot, target.TargetDir),
                    dryRun,
                    force,
                    target.TierFilter,
                    target.SkillNames.Select(static item => new SkillName(item)).ToArray()),
                cancellationToken)
            .ConfigureAwait(false);
        var reloadGuidance = hostAdapters.GetAdapter(target.Host).Value!.Descriptor.ReloadGuidance;
        var commandResult = SkillsCommandResultFactory.CreatePrune(
            pruneResult,
            target.Host,
            target.Scope,
            target.RepositoryRoot,
            reloadGuidance,
            target.ReportTiers.Select(static item => item.Value).ToArray(),
            target.SkillNames);
        commandResultWriter.WriteToStandardOutput(commandResult);
        return commandResult.ExitCode;
    }
}
