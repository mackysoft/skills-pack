using MackySoft.SkillsPack.Hosting.Cli.Common.Text;
using MackySoft.SkillsPack.Hosting.Cli.Skills;

namespace SkillsPack.Tests.Hosting.Cli.Common.Text;

public sealed class ContractLiteralCodecTests
{
    [Fact]
    [Trait("Size", "Small")]
    public void ToValue_WhenValueIsMapped_ReturnsCanonicalLiteral ()
    {
        Assert.Equal("development", ContractLiteralCodec.ToValue(SkillsPackSkillTier.Development));
    }

    [Fact]
    [Trait("Size", "Small")]
    public void TryParse_WhenLiteralExactlyMatches_ReturnsEnumValue ()
    {
        var result = ContractLiteralCodec.TryParse("personal", out SkillsPackSkillTier tier);

        Assert.True(result);
        Assert.Equal(SkillsPackSkillTier.Personal, tier);
    }

    [Fact]
    [Trait("Size", "Small")]
    public void GetLiterals_ReturnsMappedLiteralsInDeclarationOrder ()
    {
        Assert.Equal(["general", "development", "personal"], ContractLiteralCodec.GetLiterals<SkillsPackSkillTier>());
    }

    [Theory]
    [Trait("Size", "Small")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("personal ")]
    [InlineData("PERSONAL")]
    [InlineData("unsupported")]
    public void TryParse_WhenLiteralDoesNotExactlyMatch_ReturnsFalse (string? literal)
    {
        var result = ContractLiteralCodec.TryParse<SkillsPackSkillTier>(literal, out var tier);

        Assert.False(result);
        Assert.Equal(default, tier);
    }

    [Theory]
    [Trait("Size", "Small")]
    [MemberData(nameof(InvalidEnumCases))]
    public void GetLiterals_WhenEnumLiteralDefinitionIsInvalid_ThrowsInvalidOperationException (Action getLiterals)
    {
        Assert.Throws<InvalidOperationException>(getLiterals);
    }

    public static IEnumerable<object[]> InvalidEnumCases
    {
        get
        {
            yield return new object[] { (Action)(static () => ContractLiteralCodec.GetLiterals<MissingLiteralEnum>()) };
            yield return new object[] { (Action)(static () => ContractLiteralCodec.GetLiterals<EmptyLiteralEnum>()) };
            yield return new object[] { (Action)(static () => ContractLiteralCodec.GetLiterals<WhitespaceLiteralEnum>()) };
            yield return new object[] { (Action)(static () => ContractLiteralCodec.GetLiterals<DuplicateLiteralEnum>()) };
            yield return new object[] { (Action)(static () => ContractLiteralCodec.GetLiterals<DuplicateEnumValueEnum>()) };
        }
    }

    private enum MissingLiteralEnum
    {
        Value = 0,
    }

    private enum EmptyLiteralEnum
    {
        [ContractLiteral("")]
        Value = 0,
    }

    private enum WhitespaceLiteralEnum
    {
        [ContractLiteral(" value")]
        Value = 0,
    }

    private enum DuplicateLiteralEnum
    {
        [ContractLiteral("value")]
        First = 0,

        [ContractLiteral("value")]
        Second = 1,
    }

    private enum DuplicateEnumValueEnum
    {
        [ContractLiteral("first")]
        First = 0,

        [ContractLiteral("second")]
        Second = 0,
    }
}
