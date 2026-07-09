using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal static class SkillsCommandArgumentNormalizer
{
    private static readonly Dictionary<string, string> LegacyOptionMap = new(StringComparer.Ordinal)
    {
        ["--repositoryRoot"] = "--repository-root",
        ["--repoRoot"] = "--repository-root",
        ["--repo-root"] = "--repository-root",
        ["--targetDir"] = "--target-dir",
        ["--dryRun"] = "--dry-run",
        ["--printDiff"] = "--print-diff",
    };

    public static string[] Normalize (string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length == 0 || !string.Equals(args[0], SkillsPackCommandNames.Skills, StringComparison.Ordinal))
        {
            return args;
        }

        string[]? normalizedArgs = null;
        for (var i = 1; i < args.Length; i++)
        {
            if (LegacyOptionMap.TryGetValue(args[i], out var normalizedOption))
            {
                normalizedArgs ??= args.ToArray();
                normalizedArgs[i] = normalizedOption;
            }
        }

        return normalizedArgs ?? args;
    }
}
