using MackySoft.AgentSkills.Distribution;
using MackySoft.AgentSkills.Hosts.Registration;
using MackySoft.AgentSkills.Installation.Targeting;
using MackySoft.AgentSkills.Tiers;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Common.Text;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal static class SkillsCommandOptionNormalizer
{
    public static IReadOnlyList<SkillTier>? NormalizeTiers (
        string command,
        string[]? tiers,
        out CommandResult? errorResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        errorResult = null;
        if (tiers == null || tiers.Length == 0)
        {
            errorResult = CommandResult.InvalidArgument(command, "Option '--tier' is required.");
            return null;
        }

        var result = SkillTierLiteralParser.ParseSelectedTiers(SkillsPackSkillTierLiterals.Defined, tiers);
        if (!result.IsSuccess)
        {
            errorResult = CommandResult.InvalidArgument(command, result.Failure!.Message);
            return null;
        }

        return result.Value!;
    }

    public static IReadOnlyList<SkillTier>? NormalizeOptionalTiers (
        string command,
        string[]? tiers,
        out CommandResult? errorResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        errorResult = null;
        var result = tiers == null || tiers.Length == 0
            ? SkillTierLiteralParser.ParseDefinedTiers(SkillsPackSkillTierLiterals.Defined)
            : SkillTierLiteralParser.ParseSelectedTiers(SkillsPackSkillTierLiterals.Defined, tiers);
        if (!result.IsSuccess)
        {
            errorResult = CommandResult.InvalidArgument(command, result.Failure!.Message);
            return null;
        }

        return result.Value!;
    }

    public static string? NormalizeHost (
        string command,
        string? host,
        SkillHostAdapterSet hostAdapters,
        out CommandResult? errorResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentNullException.ThrowIfNull(hostAdapters);

        errorResult = null;
        if (string.IsNullOrWhiteSpace(host))
        {
            errorResult = CommandResult.InvalidArgument(command, "Option '--host' is required.");
            return null;
        }

        var adapterResult = hostAdapters.GetAdapter(host);
        if (!adapterResult.IsSuccess)
        {
            errorResult = SkillsCommandResultFactory.CreateSkillFailure(command, adapterResult.Failure!);
            return null;
        }

        return adapterResult.Value!.Descriptor.HostKey;
    }

    public static SkillScopeKind? NormalizeScope (
        string command,
        string? scope,
        out CommandResult? errorResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        errorResult = null;
        if (string.IsNullOrWhiteSpace(scope))
        {
            errorResult = CommandResult.InvalidArgument(command, "Option '--scope' is required.");
            return null;
        }

        if (!ContractLiteralInputParser.TryParseTrimmedIgnoreCase<SkillsPackScopeOption>(scope, out var scopeOption))
        {
            errorResult = CommandResult.InvalidArgument(command, $"Unsupported SKILL scope: {scope}. Supported scopes: {string.Join(", ", ContractLiteralCodec.GetLiterals<SkillsPackScopeOption>())}.");
            return null;
        }

        return scopeOption switch
        {
            SkillsPackScopeOption.Project => SkillScopeKind.Project,
            SkillsPackScopeOption.User => SkillScopeKind.User,
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scopeOption, "Unsupported SKILL scope option."),
        };
    }

    public static string? NormalizeRepositoryRootForScope (
        string command,
        SkillScopeKind scope,
        string? repoRoot,
        out CommandResult? errorResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        errorResult = null;
        if (scope == SkillScopeKind.User)
        {
            if (!string.IsNullOrWhiteSpace(repoRoot))
            {
                errorResult = CommandResult.InvalidArgument(command, "Option '--repo-root' is not supported when '--scope user' is used.");
            }

            return null;
        }

        if (!string.IsNullOrWhiteSpace(repoRoot))
        {
            return NormalizeRequiredFullPath(command, "repo-root", repoRoot, out errorResult);
        }

        return NormalizeCurrentDirectory(command, out errorResult);
    }

    public static SkillExportFormat? NormalizeExportFormat (
        string command,
        string? format,
        out CommandResult? errorResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        errorResult = null;
        if (string.IsNullOrWhiteSpace(format))
        {
            return SkillExportFormat.Directory;
        }

        if (!ContractLiteralInputParser.TryParseTrimmedIgnoreCase<SkillsPackExportFormatOption>(format, out var formatOption))
        {
            errorResult = CommandResult.InvalidArgument(command, $"Unsupported SKILL export format: {format}. Supported formats: {string.Join(", ", ContractLiteralCodec.GetLiterals<SkillsPackExportFormatOption>())}.");
            return null;
        }

        return formatOption switch
        {
            SkillsPackExportFormatOption.Directory => SkillExportFormat.Directory,
            SkillsPackExportFormatOption.Zip => SkillExportFormat.Zip,
            _ => throw new ArgumentOutOfRangeException(nameof(format), formatOption, "Unsupported SKILL export format option."),
        };
    }

    public static string? NormalizeRequiredFullPath (
        string command,
        string optionName,
        string? value,
        out CommandResult? errorResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(optionName);

        errorResult = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            errorResult = CommandResult.InvalidArgument(command, $"Option '--{optionName}' is required.");
            return null;
        }

        try
        {
            return Path.GetFullPath(value);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            errorResult = CommandResult.InvalidArgument(command, $"Option '--{optionName}' is invalid: {ex.Message}");
            return null;
        }
    }

    private static string? NormalizeCurrentDirectory (
        string command,
        out CommandResult? errorResult)
    {
        errorResult = null;
        try
        {
            return Path.GetFullPath(Environment.CurrentDirectory);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            errorResult = CommandResult.InvalidArgument(command, $"Current working directory path is invalid: {Environment.CurrentDirectory}. {ex.Message}");
            return null;
        }
    }
}
