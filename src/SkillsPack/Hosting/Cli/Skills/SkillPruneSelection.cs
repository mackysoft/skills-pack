using MackySoft.AgentSkills.Tiers;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal sealed record SkillPruneSelection (
    IReadOnlyList<SkillTier> ReportTiers,
    IReadOnlyList<SkillTier> TierFilter,
    IReadOnlyList<string> SkillNames);
