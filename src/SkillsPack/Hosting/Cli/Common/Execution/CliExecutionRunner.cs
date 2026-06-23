using ConsoleAppFramework;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Common.Startup;
using MackySoft.SkillsPack.Hosting.Composition.Common;
using Microsoft.Extensions.DependencyInjection;

namespace MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

internal sealed class CliExecutionRunner
{
    private const string InternalErrorMessage = "An unexpected internal error occurred.";
    private const string CanceledMessage = "Command execution was canceled.";

    public async Task<int> RunAsync (
        string[] args,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(args);

        using var serviceProvider = CreateServiceProvider();
        var writer = serviceProvider.GetRequiredService<ICommandResultWriter>();
        var app = SkillsPackCommandCatalog.RegisterCommands(ConsoleApp.Create());
        var previousServiceProvider = ConsoleApp.ServiceProvider;
        ConsoleApp.ServiceProvider = serviceProvider;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await app.RunAsync(args, disposeServiceProvider: false).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            var canceled = CommandResult.Canceled(SkillsPackCommandNames.Root, CanceledMessage);
            writer.WriteToStandardOutput(canceled);
            Environment.ExitCode = canceled.ExitCode;
        }
        catch (Exception)
        {
            var internalError = CommandResult.InternalError(SkillsPackCommandNames.Root, InternalErrorMessage);
            writer.WriteToStandardOutput(internalError);
            Environment.ExitCode = internalError.ExitCode;
        }
        finally
        {
            ConsoleApp.ServiceProvider = previousServiceProvider;
        }

        return Environment.ExitCode;
    }

    private static ServiceProvider CreateServiceProvider ()
    {
        var services = new ServiceCollection();
        services.AddSkillsPackServices();
        return services.BuildServiceProvider();
    }
}
