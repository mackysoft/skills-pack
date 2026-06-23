using MackySoft.AgentSkills.Installation.Targeting;
using MackySoft.AgentSkills.Tiers;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal sealed record SkillsTargetOptions (
    string Host,
    SkillScopeKind Scope,
    string? RepositoryRoot,
    string? TargetDir,
    IReadOnlyList<SkillTier> Tiers);
