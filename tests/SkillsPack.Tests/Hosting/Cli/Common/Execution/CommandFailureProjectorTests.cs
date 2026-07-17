using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

namespace SkillsPack.Tests.Hosting.Cli.Common.Execution;

public sealed class CommandFailureProjectorTests
{
    [Fact]
    [Trait("Size", "Small")]
    public void Create_WhenFailureIsInvalidInput_ProjectsInvalidArgumentExitCode ()
    {
        var result = CommandFailureProjector.Create(
            "skills.install",
            ApplicationFailure.InvalidInput("Invalid category."));

        Assert.Equal(SkillsPackProtocol.StatusError, result.Status);
        Assert.Equal((int)CliExitCode.InvalidArgument, result.ExitCode);
        Assert.Single(result.Errors);
        Assert.Equal(SkillsPackCoreErrorCodes.InvalidArgument, result.Errors[0].Code);
    }

    [Fact]
    [Trait("Size", "Small")]
    public void Create_WhenFailuresContainToolError_ProjectsToolErrorExitCode ()
    {
        var result = CommandFailureProjector.Create(
            "skills.doctor",
            "Doctor failed.",
            payload: new { },
            [
                ApplicationFailure.InvalidInput("Invalid target."),
                ApplicationFailure.InternalError("Doctor failed."),
            ]);

        Assert.Equal(SkillsPackProtocol.StatusError, result.Status);
        Assert.Equal((int)CliExitCode.ToolError, result.ExitCode);
        Assert.Equal(2, result.Errors.Count);
    }
}
