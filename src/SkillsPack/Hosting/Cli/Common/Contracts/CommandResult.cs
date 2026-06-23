namespace MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;

internal sealed record CommandResult (
    int ProtocolVersion,
    string Command,
    string Status,
    int ExitCode,
    string Message,
    object Payload,
    IReadOnlyList<CommandError> Errors)
{
    private static readonly object EmptyPayload = new();
    private static readonly IReadOnlyList<CommandError> EmptyErrors = Array.Empty<CommandError>();

    public static CommandResult Success (
        string command,
        string message,
        object? payload = null)
    {
        return new CommandResult(
            SkillsPackProtocol.CurrentVersion,
            NormalizeCommand(command),
            SkillsPackProtocol.StatusOk,
            (int)CliExitCode.Success,
            NormalizeMessage(message),
            payload ?? EmptyPayload,
            EmptyErrors);
    }

    public static CommandResult InvalidArgument (
        string command,
        string message)
    {
        return Error(
            command,
            message,
            payload: null,
            [new CommandError(SkillsPackCoreErrorCodes.InvalidArgument, NormalizeMessage(message), null)],
            CliExitCode.InvalidArgument);
    }

    public static CommandResult InternalError (
        string command,
        string message)
    {
        return Error(
            command,
            message,
            payload: null,
            [new CommandError(SkillsPackCoreErrorCodes.InternalError, NormalizeMessage(message), null)],
            CliExitCode.ToolError);
    }

    public static CommandResult Canceled (
        string command,
        string message)
    {
        return Error(
            command,
            message,
            payload: null,
            [new CommandError(SkillsPackExecutionErrorCodes.Canceled, NormalizeMessage(message), null)],
            CliExitCode.ToolError);
    }

    public static CommandResult Error (
        string command,
        string message,
        object? payload,
        IReadOnlyList<CommandError> errors,
        CliExitCode exitCode)
    {
        ArgumentNullException.ThrowIfNull(errors);
        if (errors.Count == 0)
        {
            throw new ArgumentException("Error results must include at least one error.", nameof(errors));
        }

        return new CommandResult(
            SkillsPackProtocol.CurrentVersion,
            NormalizeCommand(command),
            SkillsPackProtocol.StatusError,
            (int)exitCode,
            NormalizeMessage(message),
            payload ?? EmptyPayload,
            errors);
    }

    private static string NormalizeCommand (string command)
    {
        return string.IsNullOrWhiteSpace(command) ? SkillsPackCommandNames.Root : command;
    }

    private static string NormalizeMessage (string message)
    {
        return string.IsNullOrWhiteSpace(message) ? "An unknown error occurred." : message;
    }
}
