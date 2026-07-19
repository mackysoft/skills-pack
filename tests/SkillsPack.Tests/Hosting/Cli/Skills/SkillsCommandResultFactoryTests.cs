using System.Text.Json;
using MackySoft.AgentSkills.Hosting.Commands;
using MackySoft.AgentSkills.Shared.Text;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Skills;
using MackySoft.SkillsPack.Hosting.Composition.Features;
using Microsoft.Extensions.DependencyInjection;

namespace SkillsPack.Tests.Hosting.Cli.Skills;

public sealed class SkillsCommandResultFactoryTests
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [Fact]
    [Trait("Size", "Small")]
    public async Task Create_WithListReport_ProjectsSkillsPackListPayload ()
    {
        using var serviceProvider = CreateServiceProvider();
        var runner = serviceProvider.GetRequiredService<AgentSkillsCommandRunner>();
        var agentSkillsResult = await runner.ListAsync(new AgentSkillsListCommandRequest(skill: ["commit"]));

        CommandResult result = SkillsCommandResultFactory.Create(agentSkillsResult);

        Assert.Equal(SkillsPackCommandNames.SkillsList, result.Command);
        Assert.Equal(SkillsPackProtocol.StatusOk, result.Status);
        Assert.Equal((int)CliExitCode.Success, result.ExitCode);

        JsonElement payload = SerializePayload(result);
        Assert.Equal(["basic", "development"], GetStrings(payload.GetProperty("categories")));
        Assert.Equal(["commit"], GetStrings(payload.GetProperty("skillNames")));
        Assert.Equal(
            new[] { ("basic", 2), ("development", 19) },
            payload.GetProperty("availableCategories")
                .EnumerateArray()
                .Select(static category => (
                    category.GetProperty("category").GetString()!,
                    category.GetProperty("skillCount").GetInt32()))
                .ToArray());

        JsonElement[] skills = payload.GetProperty("skills").EnumerateArray().ToArray();
        Assert.Equal(["commit", "referent-modeling", "writing"], skills.Select(static skill => skill.GetProperty("skillName").GetString()!).ToArray());
        JsonElement skill = skills[0];
        Assert.Equal("commit", skill.GetProperty("skillName").GetString());
        Assert.Equal("development", skill.GetProperty("category").GetString());
        Assert.False(skill.TryGetProperty("catalogId", out _));
    }

    [Fact]
    [Trait("Size", "Small")]
    public async Task Create_WithProjectInstallReport_ProjectsCanonicalPathsAndCounts ()
    {
        var repositoryRoot = CreateTemporaryRepository("install-report");
        try
        {
            using var serviceProvider = CreateServiceProvider();
            var runner = serviceProvider.GetRequiredService<AgentSkillsCommandRunner>();
            var agentSkillsResult = await runner.InstallAsync(new AgentSkillsInstallCommandRequest(
                host: "openai",
                skill: ["writing"],
                scope: "project",
                repositoryRoot: repositoryRoot,
                dryRun: true));

            CommandResult result = SkillsCommandResultFactory.Create(agentSkillsResult);

            Assert.Equal(SkillsPackCommandNames.SkillsInstall, result.Command);
            Assert.Equal(SkillsPackProtocol.StatusOk, result.Status);

            JsonElement payload = SerializePayload(result);
            Assert.Equal("openai", payload.GetProperty("host").GetString());
            Assert.Equal("project", payload.GetProperty("scope").GetString());
            Assert.Equal(Path.GetFullPath(repositoryRoot), payload.GetProperty("repositoryRoot").GetString());
            Assert.Equal(
                Path.Combine(Path.GetFullPath(repositoryRoot), ".agents", "skills", "com.mackysoft.skills-pack"),
                payload.GetProperty("targetRoot").GetString());
            Assert.Equal(2, payload.GetProperty("createdCount").GetInt32());
            Assert.Equal(0, payload.GetProperty("blockedCount").GetInt32());
        }
        finally
        {
            Directory.Delete(repositoryRoot, recursive: true);
        }
    }

    [Fact]
    [Trait("Size", "Small")]
    public async Task Create_WithDoctorErrorReport_ReturnsToolError ()
    {
        var repositoryRoot = CreateTemporaryRepository("doctor-report");
        try
        {
            var unmanagedSkillRoot = Path.Combine(repositoryRoot, ".agents", "skills", "referent-modeling");
            Directory.CreateDirectory(unmanagedSkillRoot);
            File.WriteAllText(Path.Combine(unmanagedSkillRoot, "SKILL.md"), "# Unmanaged\n");

            using var serviceProvider = CreateServiceProvider();
            var runner = serviceProvider.GetRequiredService<AgentSkillsCommandRunner>();
            var agentSkillsResult = await runner.DoctorAsync(new AgentSkillsDoctorCommandRequest(
                host: "openai",
                skill: ["referent-modeling"],
                scope: "project",
                repositoryRoot: repositoryRoot));

            CommandResult result = SkillsCommandResultFactory.Create(agentSkillsResult);

            Assert.Equal(SkillsPackProtocol.StatusError, result.Status);
            Assert.Equal((int)CliExitCode.ToolError, result.ExitCode);
            CommandError error = Assert.Single(result.Errors);
            Assert.Equal("SKILL_INSTALL_TARGET_UNMANAGED", error.Code.Value);
            Assert.Equal("referent-modeling", error.OpId);

            JsonElement payload = SerializePayload(result);
            Assert.Equal(Path.GetFullPath(repositoryRoot), payload.GetProperty("repositoryRoot").GetString());
            Assert.Equal("error", Assert.Single(payload.GetProperty("diagnostics").EnumerateArray()).GetProperty("severity").GetString());
            Assert.False(string.IsNullOrWhiteSpace(payload.GetProperty("reloadGuidance").GetString()));
        }
        finally
        {
            Directory.Delete(repositoryRoot, recursive: true);
        }
    }

    private static ServiceProvider CreateServiceProvider ()
    {
        return new ServiceCollection()
            .AddSkillsPackSkillServices()
            .BuildServiceProvider();
    }

    private static string CreateTemporaryRepository (string name)
    {
        var repositoryRoot = Path.Combine(
            AppContext.BaseDirectory,
            $"skills-pack-{name}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(repositoryRoot);
        return repositoryRoot;
    }

    private static JsonElement SerializePayload (CommandResult result)
    {
        return JsonSerializer.SerializeToElement(
            result.Payload,
            result.Payload.GetType(),
            JsonOptions);
    }

    private static string[] GetStrings (JsonElement element)
    {
        return element.EnumerateArray()
            .Select(static item => item.GetString()!)
            .ToArray();
    }

    private static JsonSerializerOptions CreateJsonOptions ()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        options.Converters.Add(new ContractLiteralJsonConverterFactory());
        return options;
    }
}
