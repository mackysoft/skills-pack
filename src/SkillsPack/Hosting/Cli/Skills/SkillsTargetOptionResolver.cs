using MackySoft.AgentSkills.Hosts.Registration;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal static class SkillsTargetOptionResolver
{
    public static SkillsTargetOptions? Resolve (
        string command,
        string? host,
        string? scope,
        string? repoRoot,
        string? targetDir,
        string[]? tier,
        SkillHostAdapterSet hostAdapters,
        out CommandResult? errorResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentNullException.ThrowIfNull(hostAdapters);

        var normalizedHost = SkillsCommandOptionNormalizer.NormalizeHost(
            command,
            host,
            hostAdapters,
            out errorResult);
        if (errorResult is not null)
        {
            return null;
        }

        var normalizedScope = SkillsCommandOptionNormalizer.NormalizeScope(
            command,
            scope,
            out errorResult);
        if (errorResult is not null)
        {
            return null;
        }

        var repositoryRoot = SkillsCommandOptionNormalizer.NormalizeRepositoryRootForScope(
            command,
            normalizedScope!.Value,
            repoRoot,
            out errorResult);
        if (errorResult is not null)
        {
            return null;
        }

        var normalizedTiers = SkillsCommandOptionNormalizer.NormalizeTiers(
            command,
            tier,
            out errorResult);
        if (errorResult is not null)
        {
            return null;
        }

        return new SkillsTargetOptions(
            normalizedHost!,
            normalizedScope.Value,
            repositoryRoot,
            targetDir,
            normalizedTiers!);
    }
}
