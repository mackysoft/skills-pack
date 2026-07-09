using ConsoleAppFramework;
using MackySoft.AgentSkills.ConsoleAppFramework;

namespace MackySoft.SkillsPack.Hosting.Cli.Common.Startup;

internal static class SkillsPackCommandCatalog
{
    public static ConsoleApp.ConsoleAppBuilder RegisterCommands (ConsoleApp.ConsoleAppBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.RegisterAgentSkillsCommands();

        return app;
    }
}
