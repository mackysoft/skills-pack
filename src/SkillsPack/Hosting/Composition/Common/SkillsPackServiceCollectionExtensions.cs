using MackySoft.SkillsPack.Hosting.Cli.Common.Execution;
using MackySoft.SkillsPack.Hosting.Composition.Features;
using Microsoft.Extensions.DependencyInjection;

namespace MackySoft.SkillsPack.Hosting.Composition.Common;

internal static class SkillsPackServiceCollectionExtensions
{
    public static IServiceCollection AddSkillsPackServices (this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ICommandResultWriter, CommandResultWriter>();
        services.AddSkillsPackSkillServices();
        return services;
    }
}
