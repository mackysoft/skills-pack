using MackySoft.AgentSkills.Hosting.Composition;
using MackySoft.AgentSkills.Hosting.Reporting;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Skills;
using Microsoft.Extensions.DependencyInjection;

namespace MackySoft.SkillsPack.Hosting.Composition.Features;

internal static class SkillsPackSkillServiceCollectionExtensions
{
    public static IServiceCollection AddSkillsPackSkillServices (this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAgentSkillsCommandRuntime(options =>
        {
            options.ProductName = "SkillsPack";
            options.CatalogId = SkillsPackSkillCatalogLiterals.Official;
            options.Tiers = SkillsPackSkillTierLiterals.Defined;
            options.PackageBaseDirectory = AppContext.BaseDirectory;
            options.CommandRoot = SkillsPackCommandNames.Skills;
        });
        services.AddSingleton<IAgentSkillsCommandResultEmitter, SkillsPackAgentSkillsCommandResultEmitter>();

        return services;
    }
}
