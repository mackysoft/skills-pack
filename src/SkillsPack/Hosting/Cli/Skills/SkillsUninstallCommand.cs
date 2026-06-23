using ConsoleAppFramework;
using MackySoft.AgentSkills.Distribution;
using MackySoft.AgentSkills.Hosts.Registration;
using MackySoft.AgentSkills.Installation.Requests;
using MackySoft.AgentSkills.Installation.Services;
using MackySoft.AgentSkills.Installation.Targeting;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal sealed class SkillsUninstallCommand
{
    private readonly ICommandResultWriter commandResultWriter;
    private readonly SkillHostAdapterSet hostAdapters;
    private readonly SkillPackageProvider packageProvider;
    private readonly SkillUninstallService uninstallService;

    public SkillsUninstallCommand (
        SkillPackageProvider packageProvider,
        SkillHostAdapterSet hostAdapters,
        SkillUninstallService uninstallService,
        ICommandResultWriter commandResultWriter)
    {
        this.packageProvider = packageProvider ?? throw new ArgumentNullException(nameof(packageProvider));
        this.hostAdapters = hostAdapters ?? throw new ArgumentNullException(nameof(hostAdapters));
        this.uninstallService = uninstallService ?? throw new ArgumentNullException(nameof(uninstallService));
        this.commandResultWriter = commandResultWriter ?? throw new ArgumentNullException(nameof(commandResultWriter));
    }

    [Command(SkillsPackCommandNames.UninstallSubcommand)]
    public async Task<int> UninstallAsync (
        string? host = null,
        string? scope = null,
        string? repoRoot = null,
        string? targetDir = null,
        bool dryRun = false,
        bool force = false,
        string[]? tier = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var target = SkillsTargetOptionResolver.Resolve(
            SkillsPackCommandNames.SkillsUninstall,
            host,
            scope,
            repoRoot,
            targetDir,
            tier,
            hostAdapters,
            out var errorResult);
        if (errorResult is not null)
        {
            commandResultWriter.WriteToStandardOutput(errorResult);
            return errorResult.ExitCode;
        }

        var packagesResult = await packageProvider.GetPackagesAsync(SkillsPackSkillTierLiterals.Defined, target!.Tiers, cancellationToken).ConfigureAwait(false);
        if (!packagesResult.IsSuccess)
        {
            var packageErrorResult = SkillsCommandResultFactory.CreateSkillFailure(SkillsPackCommandNames.SkillsUninstall, packagesResult.Failure!);
            commandResultWriter.WriteToStandardOutput(packageErrorResult);
            return packageErrorResult.ExitCode;
        }

        var uninstallResult = await uninstallService.UninstallAsync(
                new SkillUninstallInput(
                    packagesResult.Value!,
                    new SkillInstallRequest(target.Host, target.Scope, target.RepositoryRoot, target.TargetDir),
                    dryRun,
                    force),
                cancellationToken)
            .ConfigureAwait(false);
        var reloadGuidance = hostAdapters.GetAdapter(target.Host).Value!.Descriptor.ReloadGuidance;
        var commandResult = SkillsCommandResultFactory.CreateUninstall(
            uninstallResult,
            target.Host,
            target.Scope,
            target.RepositoryRoot,
            reloadGuidance,
            target.Tiers.Select(static item => item.Value).ToArray());
        commandResultWriter.WriteToStandardOutput(commandResult);
        return commandResult.ExitCode;
    }
}
