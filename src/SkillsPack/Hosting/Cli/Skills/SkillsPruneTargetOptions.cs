using MackySoft.AgentSkills.Installation.Targeting;
using MackySoft.AgentSkills.Tiers;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal sealed record SkillsPruneTargetOptions (
    string Host,
    SkillScopeKind Scope,
    string? RepositoryRoot,
    string? TargetDir,
    IReadOnlyList<SkillTier> ReportTiers,
    IReadOnlyList<SkillTier> TierFilter,
    IReadOnlyList<string> SkillNames);
