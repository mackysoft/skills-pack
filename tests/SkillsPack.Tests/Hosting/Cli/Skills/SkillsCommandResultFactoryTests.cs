using System.Text.Json;
using MackySoft.AgentSkills.Hosting.Commands;
using MackySoft.AgentSkills.Hosts.Claude;
using MackySoft.AgentSkills.Hosts.Copilot;
using MackySoft.AgentSkills.Hosts.OpenAi;
using MackySoft.AgentSkills.Hosts.Registration;
using MackySoft.AgentSkills.OperationReports.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Skills;

namespace SkillsPack.Tests.Hosting.Cli.Skills;

public sealed class SkillsCommandResultFactoryTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    [Trait("Size", "Small")]
    public void Create_WithListReport_ProjectsSkillsPackListPayload ()
    {
        CommandResult result = SkillsCommandResultFactory.Create(
            AgentSkillsCommandResult.Success(SkillsPackCommandNames.SkillsList, ListReport()),
            HostAdapters());

        Assert.Equal(SkillsPackCommandNames.SkillsList, result.Command);
        Assert.Equal(SkillsPackProtocol.StatusOk, result.Status);
        Assert.Equal((int)CliExitCode.Success, result.ExitCode);

        JsonElement payload = SerializePayload(result);
        JsonElement skill = Assert.Single(payload.GetProperty("skills").EnumerateArray());
        Assert.Equal("commit", skill.GetProperty("skillName").GetString());
        Assert.False(skill.TryGetProperty("catalogId", out _));
    }

    [Fact]
    [Trait("Size", "Small")]
    public void Create_WithOperationReport_ProjectsOperationCounts ()
    {
        CommandResult result = SkillsCommandResultFactory.Create(
            AgentSkillsCommandResult.Success(
                SkillsPackCommandNames.SkillsInstall,
                OperationReport(
                    [
                        Action("commit", "created", "changed"),
                        Action("writing", "noOp", "unchanged"),
                        Action("local-only", "blockedUnmanaged", "blocked", "unmanagedTarget"),
                    ])),
            HostAdapters());

        Assert.Equal(SkillsPackCommandNames.SkillsInstall, result.Command);
        Assert.Equal(SkillsPackProtocol.StatusOk, result.Status);

        JsonElement payload = SerializePayload(result);
        Assert.Equal(1, payload.GetProperty("createdCount").GetInt32());
        Assert.Equal(1, payload.GetProperty("noOpCount").GetInt32());
        Assert.Equal(1, payload.GetProperty("blockedCount").GetInt32());
    }

    [Fact]
    [Trait("Size", "Small")]
    public void Create_WithDoctorErrorReport_ReturnsToolError ()
    {
        CommandResult result = SkillsCommandResultFactory.Create(
            AgentSkillsCommandResult.Success(
                SkillsPackCommandNames.SkillsDoctor,
                new SkillDoctorReport(
                    "openai",
                    ["general"],
                    [],
                    "project",
                    "/repo/.codex/skills",
                    IsHealthy: false,
                    [
                        new SkillDoctorDiagnosticReport(
                            "error",
                            "SKILL_TARGET_UNMANAGED",
                            "Target is unmanaged.",
                            "commit",
                            "unmanaged"),
                    ])),
            HostAdapters());

        Assert.Equal(SkillsPackProtocol.StatusError, result.Status);
        Assert.Equal((int)CliExitCode.ToolError, result.ExitCode);
        CommandError error = Assert.Single(result.Errors);
        Assert.Equal("SKILL_TARGET_UNMANAGED", error.Code.Value);
        Assert.Equal("commit", error.OpId);
    }

    private static SkillListReport ListReport ()
    {
        return new SkillListReport(
            Tiers: ["general"],
            SkillNames: [],
            AvailableTiers: [new SkillListTierReport("general", 1)],
            Skills:
            [
                new SkillListSkillReport(
                    SchemaVersion: 1,
                    SkillBundleVersion: 1,
                    SkillName: "commit",
                    DisplayName: "Commit",
                    Description: "Create commits.",
                    Dependencies: [],
                    Tier: "general",
                    CatalogId: SkillsPackSkillCatalogLiterals.Official,
                    ContentDigest: "sha256:content",
                    ManifestDigest: "sha256:manifest",
                    HostArtifacts: []),
            ],
            SupportedHosts: []);
    }

    private static SkillOperationReport OperationReport (IReadOnlyList<SkillOperationActionReport> actions)
    {
        return new SkillOperationReport(
            Host: "openai",
            Tiers: ["general"],
            SkillNames: [],
            Scope: "project",
            TargetRoot: "/repo/.codex/skills",
            DryRun: false,
            Force: false,
            ReloadGuidance: "Reload the host.",
            Actions: actions,
            ActionCounts: [],
            StatusCounts: []);
    }

    private static SkillOperationActionReport Action (
        string skillName,
        string action,
        string status,
        string? blockedReason = null)
    {
        return new SkillOperationActionReport(
            SkillName: skillName,
            Action: action,
            Status: status,
            BlockedReason: blockedReason,
            TargetState: null,
            FileChanges: null,
            FileDiffs: []);
    }

    private static SkillHostAdapterSet HostAdapters ()
    {
        return new SkillHostAdapterSet(
        [
            new ClaudeSkillHostAdapter(),
            new CopilotSkillHostAdapter(),
            new OpenAiSkillHostAdapter(),
        ]);
    }

    private static JsonElement SerializePayload (CommandResult result)
    {
        return JsonSerializer.SerializeToElement(
            result.Payload,
            result.Payload.GetType(),
            JsonOptions);
    }
}
