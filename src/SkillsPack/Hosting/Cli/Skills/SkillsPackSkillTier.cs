using MackySoft.SkillsPack.Hosting.Cli.Common.Text;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal enum SkillsPackSkillTier
{
    [ContractLiteral("general")]
    General = 0,

    [ContractLiteral("development")]
    Development = 1,

    [ContractLiteral("personal")]
    Personal = 2,
}
