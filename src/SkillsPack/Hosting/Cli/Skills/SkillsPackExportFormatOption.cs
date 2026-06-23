using MackySoft.SkillsPack.Hosting.Cli.Common.Text;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal enum SkillsPackExportFormatOption
{
    [ContractLiteral("directory")]
    Directory = 0,

    [ContractLiteral("zip")]
    Zip = 1,
}
