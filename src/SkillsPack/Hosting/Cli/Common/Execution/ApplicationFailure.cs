using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;

namespace MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

internal sealed record ApplicationFailure
{
    public ApplicationFailure (
        ApplicationFailureKind kind,
        ApplicationOutcome outcome,
        SkillsPackCode code,
        string message,
        string? opId = null)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Failure kind is not defined.");
        }

        if (!Enum.IsDefined(outcome) || outcome == ApplicationOutcome.Success)
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Failure outcome must be a failure outcome.");
        }

        if (!code.IsValid)
        {
            throw new ArgumentException("Failure code must be valid.", nameof(code));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Kind = kind;
        Outcome = outcome;
        Code = code;
        Message = message;
        OpId = opId;
    }

    public ApplicationFailureKind Kind { get; }

    public ApplicationOutcome Outcome { get; }

    public SkillsPackCode Code { get; }

    public string Message { get; }

    public string? OpId { get; }

    public static ApplicationFailure InvalidInput (
        string message,
        SkillsPackCode? code = null,
        string? opId = null)
    {
        return new ApplicationFailure(
            ApplicationFailureKind.InvalidInput,
            ApplicationOutcome.InvalidArgument,
            ResolveCode(code, SkillsPackCoreErrorCodes.InvalidArgument),
            message,
            opId);
    }

    public static ApplicationFailure InternalError (
        string message,
        SkillsPackCode? code = null,
        string? opId = null)
    {
        return new ApplicationFailure(
            ApplicationFailureKind.InternalError,
            ApplicationOutcome.ToolError,
            ResolveCode(code, SkillsPackCoreErrorCodes.InternalError),
            message,
            opId);
    }

    private static SkillsPackCode ResolveCode (
        SkillsPackCode? code,
        SkillsPackCode fallback)
    {
        return code.HasValue && code.Value.IsValid ? code.Value : fallback;
    }
}
