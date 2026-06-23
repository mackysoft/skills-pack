using MackySoft.SkillsPack.Hosting.Cli.Common.Text;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal static class SkillsPackSkillTierLiterals
{
    public static IReadOnlyList<string> Defined { get; } = ContractLiteralCodec.GetLiterals<SkillsPackSkillTier>();
}
