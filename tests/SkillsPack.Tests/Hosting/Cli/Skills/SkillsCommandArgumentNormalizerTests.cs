using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Skills;

namespace SkillsPack.Tests.Hosting.Cli.Skills;

public sealed class SkillsCommandArgumentNormalizerTests
{
    [Fact]
    [Trait("Size", "Small")]
    public void Normalize_WhenSkillsCommandUsesLegacyOptions_RewritesGeneratedOptionNames ()
    {
        string[] args =
        [
            SkillsPackCommandNames.Skills,
            SkillsPackCommandNames.InstallSubcommand,
            "--repositoryRoot",
            "/repo-from-camel",
            "--repoRoot",
            "/repo",
            "--repo-root",
            "/repo-from-kebab",
            "--targetDir",
            ".agents/skills",
            "--dryRun",
            "--printDiff",
        ];

        var normalized = SkillsCommandArgumentNormalizer.Normalize(args);

        Assert.Equal(
            new[]
            {
                SkillsPackCommandNames.Skills,
                SkillsPackCommandNames.InstallSubcommand,
                "--repository-root",
                "/repo-from-camel",
                "--repository-root",
                "/repo",
                "--repository-root",
                "/repo-from-kebab",
                "--target-dir",
                ".agents/skills",
                "--dry-run",
                "--print-diff",
            },
            normalized);
    }

    [Fact]
    [Trait("Size", "Small")]
    public void Normalize_WhenCommandIsNotSkills_ReturnsOriginalArguments ()
    {
        string[] args = [SkillsPackCommandNames.Root, "--repo-root"];

        var normalized = SkillsCommandArgumentNormalizer.Normalize(args);

        Assert.Same(args, normalized);
    }
}
