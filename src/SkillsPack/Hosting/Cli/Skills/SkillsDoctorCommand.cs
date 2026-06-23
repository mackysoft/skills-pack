using ConsoleAppFramework;
using MackySoft.AgentSkills.Distribution;
using MackySoft.AgentSkills.Doctor;
using MackySoft.AgentSkills.Hosts.Registration;
using MackySoft.AgentSkills.Installation.Targeting;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal sealed class SkillsDoctorCommand
{
    private readonly ICommandResultWriter commandResultWriter;
    private readonly SkillDoctorService doctorService;
    private readonly SkillHostAdapterSet hostAdapters;
    private readonly SkillPackageProvider packageProvider;
    private readonly SkillInstallTargetResolver targetResolver;

    public SkillsDoctorCommand (
        SkillPackageProvider packageProvider,
        SkillHostAdapterSet hostAdapters,
        SkillInstallTargetResolver targetResolver,
        SkillDoctorService doctorService,
        ICommandResultWriter commandResultWriter)
    {
        this.packageProvider = packageProvider ?? throw new ArgumentNullException(nameof(packageProvider));
        this.hostAdapters = hostAdapters ?? throw new ArgumentNullException(nameof(hostAdapters));
        this.targetResolver = targetResolver ?? throw new ArgumentNullException(nameof(targetResolver));
        this.doctorService = doctorService ?? throw new ArgumentNullException(nameof(doctorService));
        this.commandResultWriter = commandResultWriter ?? throw new ArgumentNullException(nameof(commandResultWriter));
    }

    [Command(SkillsPackCommandNames.DoctorSubcommand)]
    public async Task<int> DoctorAsync (
        string? host = null,
        string? scope = null,
        string? repoRoot = null,
        string? targetDir = null,
        string[]? tier = null,
        string[]? skill = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var target = SkillsTargetOptionResolver.Resolve(
            SkillsPackCommandNames.SkillsDoctor,
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

        var targetResult = targetResolver.ResolveTarget(new SkillInstallRequest(target!.Host, target.Scope, target.RepositoryRoot, target.TargetDir));
        if (!targetResult.IsSuccess)
        {
            var targetErrorResult = SkillsCommandResultFactory.CreateSkillFailure(SkillsPackCommandNames.SkillsDoctor, targetResult.Failure!);
            commandResultWriter.WriteToStandardOutput(targetErrorResult);
            return targetErrorResult.ExitCode;
        }

        var packagesResult = await packageProvider.GetPackagesAsync(SkillsPackSkillTierLiterals.Defined, target.Tiers, target.SkillNames, cancellationToken).ConfigureAwait(false);
        if (!packagesResult.IsSuccess)
        {
            var packageErrorResult = SkillsCommandResultFactory.CreateSkillFailure(SkillsPackCommandNames.SkillsDoctor, packagesResult.Failure!);
            commandResultWriter.WriteToStandardOutput(packageErrorResult);
            return packageErrorResult.ExitCode;
        }

        var reloadGuidance = hostAdapters.GetAdapter(target.Host).Value!.Descriptor.ReloadGuidance;
        var tierLiterals = target.Tiers.Select(static item => item.Value).ToArray();
        var skillNames = target.SkillNames;
        if (packagesResult.Value!.Count == 0)
        {
            var emptyDoctorResult = new SkillDoctorResult(target.Host, targetResult.Value!.TargetRoot, Array.Empty<SkillDoctorDiagnostic>());
            var emptyCommandResult = SkillsCommandResultFactory.CreateDoctor(emptyDoctorResult, target.Scope, target.RepositoryRoot, reloadGuidance, tierLiterals, skillNames);
            commandResultWriter.WriteToStandardOutput(emptyCommandResult);
            return emptyCommandResult.ExitCode;
        }

        var doctorResult = await doctorService.DiagnoseAsync(
                packagesResult.Value!,
                target.Host,
                targetResult.Value!.TargetRoot,
                cancellationToken)
            .ConfigureAwait(false);
        var commandResult = SkillsCommandResultFactory.CreateDoctor(doctorResult, target.Scope, target.RepositoryRoot, reloadGuidance, tierLiterals, skillNames);
        commandResultWriter.WriteToStandardOutput(commandResult);
        return commandResult.ExitCode;
    }
}
