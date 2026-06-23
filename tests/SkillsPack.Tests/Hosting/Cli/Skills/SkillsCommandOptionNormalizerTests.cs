using MackySoft.AgentSkills.Distribution;
using MackySoft.AgentSkills.Installation.Targeting;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Skills;

namespace SkillsPack.Tests.Hosting.Cli.Skills;

public sealed class SkillsCommandOptionNormalizerTests
{
    [Fact]
    [Trait("Size", "Small")]
    public void NormalizeRequiredPackageSelection_WhenSelectorsAreMissing_ReturnsInvalidArgument ()
    {
        var result = SkillsCommandOptionNormalizer.NormalizeRequiredPackageSelection("skills.install", null, null, out var errorResult);

        Assert.Null(result);
        Assert.NotNull(errorResult);
        Assert.Equal((int)CliExitCode.InvalidArgument, errorResult!.ExitCode);
        Assert.Contains("--tier", errorResult.Message, StringComparison.Ordinal);
        Assert.Contains("--skill", errorResult.Message, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Size", "Small")]
    public void NormalizeOptionalPackageSelection_WhenSelectorsAreMissing_ReturnsAllDefinedTiersWithoutSkillNames ()
    {
        var result = SkillsCommandOptionNormalizer.NormalizeOptionalPackageSelection("skills.list", null, null, out var errorResult);

        Assert.Null(errorResult);
        Assert.NotNull(result);
        Assert.Equal(["general", "development", "personal"], result!.Tiers.Select(static item => item.Value).ToArray());
        Assert.Empty(result.SkillNames);
    }

    [Fact]
    [Trait("Size", "Small")]
    public void NormalizeRequiredPackageSelection_WhenTierArrayContainsMultipleItems_ReturnsDistinctSelectionInInputOrder ()
    {
        var result = SkillsCommandOptionNormalizer.NormalizeRequiredPackageSelection(
            "skills.install",
            ["development", "personal", "development"],
            null,
            out var errorResult);

        Assert.Null(errorResult);
        Assert.NotNull(result);
        Assert.Equal(["development", "personal"], result!.Tiers.Select(static item => item.Value).ToArray());
        Assert.Empty(result.SkillNames);
    }

    [Fact]
    [Trait("Size", "Small")]
    public void NormalizeRequiredPackageSelection_WhenSkillNamesAreSelected_ReturnsDistinctSkillNamesInInputOrder ()
    {
        var result = SkillsCommandOptionNormalizer.NormalizeRequiredPackageSelection(
            "skills.install",
            null,
            ["commit", "changelog", "commit"],
            out var errorResult);

        Assert.Null(errorResult);
        Assert.NotNull(result);
        Assert.Equal(["general", "development", "personal"], result!.Tiers.Select(static item => item.Value).ToArray());
        Assert.Equal(["commit", "changelog"], result.SkillNames);
    }

    [Theory]
    [Trait("Size", "Small")]
    [InlineData("project", SkillScopeKind.Project)]
    [InlineData("USER", SkillScopeKind.User)]
    [InlineData(" user ", SkillScopeKind.User)]
    public void NormalizeScope_WhenValueMatchesContractLiteral_ReturnsScope (
        string value,
        SkillScopeKind expected)
    {
        var result = SkillsCommandOptionNormalizer.NormalizeScope("skills.install", value, out var errorResult);

        Assert.Null(errorResult);
        Assert.Equal(expected, result);
    }

    [Theory]
    [Trait("Size", "Small")]
    [InlineData(null, SkillExportFormat.Directory)]
    [InlineData("", SkillExportFormat.Directory)]
    [InlineData("zip", SkillExportFormat.Zip)]
    [InlineData(" DIRECTORY ", SkillExportFormat.Directory)]
    public void NormalizeExportFormat_WhenValueMatchesContractLiteral_ReturnsFormat (
        string? value,
        SkillExportFormat expected)
    {
        var result = SkillsCommandOptionNormalizer.NormalizeExportFormat("skills.export", value, out var errorResult);

        Assert.Null(errorResult);
        Assert.Equal(expected, result);
    }
}
