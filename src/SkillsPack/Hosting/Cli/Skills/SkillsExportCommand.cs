using ConsoleAppFramework;
using MackySoft.AgentSkills.Distribution;
using MackySoft.AgentSkills.Hosts.Registration;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal sealed class SkillsExportCommand
{
    private readonly ICommandResultWriter commandResultWriter;
    private readonly SkillExportService exportService;
    private readonly SkillHostAdapterSet hostAdapters;
    private readonly SkillPackageProvider packageProvider;

    public SkillsExportCommand (
        SkillPackageProvider packageProvider,
        SkillHostAdapterSet hostAdapters,
        SkillExportService exportService,
        ICommandResultWriter commandResultWriter)
    {
        this.packageProvider = packageProvider ?? throw new ArgumentNullException(nameof(packageProvider));
        this.hostAdapters = hostAdapters ?? throw new ArgumentNullException(nameof(hostAdapters));
        this.exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        this.commandResultWriter = commandResultWriter ?? throw new ArgumentNullException(nameof(commandResultWriter));
    }

    [Command(SkillsPackCommandNames.ExportSubcommand)]
    public async Task<int> ExportAsync (
        string? host = null,
        string? output = null,
        string? format = null,
        string[]? tier = null,
        string[]? skill = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedHost = SkillsCommandOptionNormalizer.NormalizeHost(
            SkillsPackCommandNames.SkillsExport,
            host,
            hostAdapters,
            out var errorResult);
        if (errorResult is not null)
        {
            commandResultWriter.WriteToStandardOutput(errorResult);
            return errorResult.ExitCode;
        }

        var normalizedFormat = SkillsCommandOptionNormalizer.NormalizeExportFormat(
            SkillsPackCommandNames.SkillsExport,
            format,
            out errorResult);
        if (errorResult is not null)
        {
            commandResultWriter.WriteToStandardOutput(errorResult);
            return errorResult.ExitCode;
        }

        var outputRoot = SkillsCommandOptionNormalizer.NormalizeRequiredFullPath(
            SkillsPackCommandNames.SkillsExport,
            "output",
            output,
            out errorResult);
        if (errorResult is not null)
        {
            commandResultWriter.WriteToStandardOutput(errorResult);
            return errorResult.ExitCode;
        }

        var packageSelection = SkillsCommandOptionNormalizer.NormalizeRequiredPackageSelection(
            SkillsPackCommandNames.SkillsExport,
            tier,
            skill,
            out errorResult);
        if (errorResult is not null)
        {
            commandResultWriter.WriteToStandardOutput(errorResult);
            return errorResult.ExitCode;
        }

        var packagesResult = await packageProvider.GetPackagesAsync(SkillsPackSkillTierLiterals.Defined, packageSelection!.Tiers, packageSelection.SkillNames, cancellationToken).ConfigureAwait(false);
        if (!packagesResult.IsSuccess)
        {
            var packageErrorResult = SkillsCommandResultFactory.CreateSkillFailure(SkillsPackCommandNames.SkillsExport, packagesResult.Failure!);
            commandResultWriter.WriteToStandardOutput(packageErrorResult);
            return packageErrorResult.ExitCode;
        }

        var exportResult = await exportService.ExportAsync(
                packagesResult.Value!,
                normalizedHost!,
                outputRoot!,
                normalizedFormat!.Value,
                cancellationToken)
            .ConfigureAwait(false);
        var reloadGuidance = hostAdapters.GetAdapter(normalizedHost!).Value!.Descriptor.ReloadGuidance;
        var commandResult = SkillsCommandResultFactory.CreateExport(
            exportResult,
            packagesResult.Value!,
            normalizedHost!,
            normalizedFormat.Value,
            reloadGuidance,
            packageSelection.Tiers.Select(static item => item.Value).ToArray(),
            packageSelection.SkillNames);
        commandResultWriter.WriteToStandardOutput(commandResult);
        return commandResult.ExitCode;
    }
}
