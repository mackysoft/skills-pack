namespace MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

internal static class ApplicationFailureOutcomeResolver
{
    public static ApplicationOutcome Resolve (IReadOnlyList<ApplicationFailure> failures)
    {
        ArgumentNullException.ThrowIfNull(failures);

        if (failures.Count == 0)
        {
            return ApplicationOutcome.Success;
        }

        var hasInvalidArgument = false;
        var hasToolError = false;
        for (var i = 0; i < failures.Count; i++)
        {
            var failure = failures[i] ?? throw new ArgumentException("Failure collection must not contain null entries.", nameof(failures));
            switch (failure.Outcome)
            {
                case ApplicationOutcome.InvalidArgument:
                    hasInvalidArgument = true;
                    break;
                case ApplicationOutcome.ToolError:
                    hasToolError = true;
                    break;
                default:
                    throw new ArgumentException("Failure outcome must not be success.", nameof(failures));
            }
        }

        if (hasToolError)
        {
            return ApplicationOutcome.ToolError;
        }

        if (hasInvalidArgument)
        {
            return ApplicationOutcome.InvalidArgument;
        }

        return ApplicationOutcome.ToolError;
    }
}
