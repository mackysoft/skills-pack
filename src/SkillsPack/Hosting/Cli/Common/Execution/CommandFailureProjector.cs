using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;

namespace MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

internal static class CommandFailureProjector
{
    private static readonly object EmptyPayload = new();

    public static CommandResult Create (
        string command,
        string message,
        object? payload,
        IReadOnlyList<ApplicationFailure> failures)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentNullException.ThrowIfNull(failures);
        if (failures.Count == 0)
        {
            throw new ArgumentException("Failure collection must not be empty.", nameof(failures));
        }

        return CommandResult.Error(
            command,
            message,
            payload ?? EmptyPayload,
            CreateErrors(failures),
            ToCliExitCode(ApplicationFailureOutcomeResolver.Resolve(failures)));
    }

    public static CommandResult Create (
        string command,
        ApplicationFailure failure,
        object? payload = null)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return Create(command, failure.Message, payload, [failure]);
    }

    private static IReadOnlyList<CommandError> CreateErrors (IReadOnlyList<ApplicationFailure> failures)
    {
        var errors = new CommandError[failures.Count];
        for (var i = 0; i < failures.Count; i++)
        {
            var failure = failures[i] ?? throw new ArgumentException("Failure collection must not contain null entries.", nameof(failures));
            errors[i] = new CommandError(failure.Code, failure.Message, failure.OpId);
        }

        return errors;
    }

    private static CliExitCode ToCliExitCode (ApplicationOutcome outcome)
    {
        return (CliExitCode)ApplicationOutcomeCliExitCodeMapper.ToExitCode(outcome);
    }
}
