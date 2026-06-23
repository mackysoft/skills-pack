using MackySoft.AgentSkills.Shared;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal static class SkillFailureCommandResultMapper
{
    public static CommandResult Map (
        string command,
        SkillFailure failure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentNullException.ThrowIfNull(failure);

        var code = SkillsPackCode.TryCreate(failure.Code.Value, out var parsedCode)
            ? parsedCode
            : SkillsPackCoreErrorCodes.InternalError;
        var applicationFailure = IsInvalidArgumentFailureCode(failure.Code)
            ? ApplicationFailure.InvalidInput(failure.Message, code)
            : ApplicationFailure.InternalError(failure.Message, code);

        return CommandFailureProjector.Create(command, applicationFailure);
    }

    private static bool IsInvalidArgumentFailureCode (SkillFailureCode code)
    {
        return code == SkillFailureCodes.InputInvalid
            || code == SkillFailureCodes.HostUnsupported
            || code == SkillFailureCodes.ScopeUnsupported
            || code == SkillFailureCodes.PathUnsafe;
    }
}
