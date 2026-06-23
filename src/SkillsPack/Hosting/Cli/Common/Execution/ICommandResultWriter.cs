using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;

namespace MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

internal interface ICommandResultWriter
{
    void WriteToStandardOutput (CommandResult result);
}
