using MackySoft.AgentSkills.Tiers;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal sealed record SkillPackageSelection (
    IReadOnlyList<SkillTier> Tiers,
    IReadOnlyList<string> SkillNames);
