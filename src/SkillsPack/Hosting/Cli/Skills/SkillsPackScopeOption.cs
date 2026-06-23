using MackySoft.SkillsPack.Hosting.Cli.Common.Text;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal enum SkillsPackScopeOption
{
    [ContractLiteral("project")]
    Project = 0,

    [ContractLiteral("user")]
    User = 1,
}
