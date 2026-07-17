using MackySoft.AgentSkills.Doctor;
using MackySoft.AgentSkills.Hosting.Commands;
using MackySoft.AgentSkills.OperationReports.Contracts;
using MackySoft.AgentSkills.OperationReports.Literals;
using MackySoft.AgentSkills.Shared;
using MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;
using MackySoft.SkillsPack.Hosting.Cli.Common.Execution;

namespace MackySoft.SkillsPack.Hosting.Cli.Skills;

internal static class SkillsCommandResultFactory
{
    public static CommandResult Create (AgentSkillsCommandResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (!result.IsSuccess)
        {
            return CreateSkillFailure(result.Command, result.Failure!);
        }

        return result.Payload switch
        {
            SkillListReport report => CommandResult.Success(
                result.Command,
                "SkillsPack SKILL package list retrieval completed.",
                CreateListPayload(report)),
            SkillExportReport report => CommandResult.Success(
                result.Command,
                "SkillsPack SKILL packages exported.",
                CreateExportPayload(report)),
            SkillOperationReport report => CommandResult.Success(
                result.Command,
                CreateOperationMessage(result.Command, report),
                CreateOperationPayload(result.Command, report)),
            SkillDoctorReport report => CreateDoctor(result.Command, report),
            _ => CommandFailureProjector.Create(
                result.Command,
                ApplicationFailure.InternalError($"Unsupported Agent Skills command payload: {result.Payload?.GetType().FullName ?? "(null)"}"),
                new { }),
        };
    }

    public static CommandResult CreateSkillFailure (
        string command,
        SkillFailure failure)
    {
        return SkillFailureCommandResultMapper.Map(command, failure);
    }

    private static object CreateListPayload (SkillListReport report)
    {
        return new
        {
            categories = report.Categories,
            skillNames = report.SkillNames,
            availableCategories = report.AvailableCategories
                .Select(static category => new
                {
                    category = category.Category,
                    skillCount = category.SkillCount,
                })
                .ToArray(),
            skills = report.Skills
                .Select(static skill => new
                {
                    skillName = skill.SkillName,
                    skill.DisplayName,
                    skill.Description,
                    dependencies = skill.Dependencies,
                    category = skill.Category,
                    skill.SkillBundleVersion,
                    skill.ContentDigest,
                    skill.HostArtifacts,
                })
                .ToArray(),
            supportedHosts = report.SupportedHosts
                .Select(static host => new
                {
                    host = host.Host,
                    projectTargetDirectory = host.ProjectDefaultTargetPath,
                    userTargetDirectory = host.UserDefaultTargetPath,
                    host.ReloadGuidance,
                })
                .ToArray(),
        };
    }

    private static object CreateExportPayload (SkillExportReport report)
    {
        return new
        {
            host = report.Host,
            categories = report.Categories,
            skillNames = report.SkillNames,
            format = report.Format,
            outputRoot = report.OutputPath,
            skills = report.Skills,
            skillCount = report.SkillCount,
            reloadGuidance = report.ReloadGuidance,
        };
    }

    private static object CreateOperationPayload (
        string command,
        SkillOperationReport report)
    {
        var actions = CreateActionPayloads(report.Actions, report.TargetRoot);

        return command switch
        {
            SkillsPackCommandNames.SkillsInstall => new
            {
                host = report.Host,
                categories = report.Categories,
                skillNames = report.SkillNames,
                scope = report.Scope,
                report.RepositoryRoot,
                report.TargetRoot,
                report.DryRun,
                report.Force,
                printDiff = HasDiffs(report),
                reloadGuidance = report.ReloadGuidance,
                actions,
                createdCount = CountAction(report, "created"),
                updatedCount = CountAction(report, "updated"),
                noOpCount = CountAction(report, "noOp"),
                blockedCount = CountBlocked(report),
            },
            SkillsPackCommandNames.SkillsUpdate => new
            {
                host = report.Host,
                categories = report.Categories,
                skillNames = report.SkillNames,
                scope = report.Scope,
                report.RepositoryRoot,
                report.TargetRoot,
                report.DryRun,
                report.Force,
                printDiff = HasDiffs(report),
                reloadGuidance = report.ReloadGuidance,
                actions,
                createdCount = CountAction(report, "created"),
                updatedCount = CountAction(report, "updated"),
                noOpCount = CountAction(report, "noOp"),
                blockedCount = CountBlocked(report),
            },
            SkillsPackCommandNames.SkillsUninstall => new
            {
                host = report.Host,
                categories = report.Categories,
                skillNames = report.SkillNames,
                scope = report.Scope,
                report.RepositoryRoot,
                report.TargetRoot,
                report.DryRun,
                report.Force,
                reloadGuidance = report.ReloadGuidance,
                actions,
                deletedCount = CountAction(report, "deleted"),
                noOpCount = CountAction(report, "noOp"),
                skippedUnmanagedCount = CountAction(report, "skippedUnmanaged"),
                blockedCount = CountBlocked(report),
            },
            SkillsPackCommandNames.SkillsPrune => new
            {
                host = report.Host,
                categories = report.Categories,
                skillNames = report.SkillNames,
                scope = report.Scope,
                report.RepositoryRoot,
                report.TargetRoot,
                report.DryRun,
                report.Force,
                reloadGuidance = report.ReloadGuidance,
                actions,
                deletedCount = CountAction(report, "deleted"),
                skippedCurrentCount = CountAction(report, "skippedCurrent"),
                skippedForeignCatalogCount = CountAction(report, "skippedForeignCatalog"),
                skippedUnmanagedCount = CountAction(report, "skippedUnmanaged"),
                blockedCount = CountBlocked(report),
            },
            _ => new
            {
                host = report.Host,
                categories = report.Categories,
                skillNames = report.SkillNames,
                scope = report.Scope,
                report.RepositoryRoot,
                report.TargetRoot,
                report.DryRun,
                report.Force,
                reloadGuidance = report.ReloadGuidance,
                actions,
                actionCounts = report.ActionCounts,
                statusCounts = report.StatusCounts,
            },
        };
    }

    private static CommandResult CreateDoctor (
        string command,
        SkillDoctorReport report)
    {
        var payload = new
        {
            host = report.Host,
            categories = report.Categories,
            skillNames = report.SkillNames,
            scope = report.Scope,
            report.RepositoryRoot,
            report.TargetRoot,
            report.ReloadGuidance,
            report.IsHealthy,
            diagnostics = report.Diagnostics
                .Select(static diagnostic => new
                {
                    severity = diagnostic.Severity,
                    diagnostic.Code,
                    diagnostic.Message,
                    diagnostic.SkillName,
                })
                .ToArray(),
        };

        if (report.IsHealthy)
        {
            return CommandResult.Success(
                command,
                "SkillsPack SKILL packages are healthy.",
                payload);
        }

        var failures = report.Diagnostics
            .Where(static diagnostic => diagnostic.Severity == SkillDoctorSeverity.Error)
            .Select(static diagnostic => ApplicationFailure.InternalError(diagnostic.Message, ToCode(diagnostic.Code), diagnostic.SkillName))
            .ToArray();
        if (failures.Length == 0)
        {
            failures =
            [
                ApplicationFailure.InternalError("SkillsPack skills doctor reported an unknown error."),
            ];
        }

        return CommandFailureProjector.Create(
            command,
            "SkillsPack skills doctor reported errors.",
            payload,
            failures);
    }

    private static string CreateOperationMessage (
        string command,
        SkillOperationReport report)
    {
        return command switch
        {
            SkillsPackCommandNames.SkillsInstall => report.DryRun ? "SkillsPack SKILL install plan generated." : "SkillsPack SKILL packages installed.",
            SkillsPackCommandNames.SkillsUpdate => report.DryRun ? "SkillsPack SKILL update plan generated." : "SkillsPack SKILL packages updated.",
            SkillsPackCommandNames.SkillsUninstall => report.DryRun ? "SkillsPack SKILL uninstall plan generated." : "SkillsPack SKILL packages uninstalled.",
            SkillsPackCommandNames.SkillsPrune => report.DryRun ? "SkillsPack SKILL prune plan generated." : "SkillsPack SKILL packages pruned.",
            _ => "SkillsPack SKILL operation completed.",
        };
    }

    private static object[] CreateActionPayloads (
        IReadOnlyList<SkillOperationActionReport> actions,
        string targetRoot)
    {
        return actions
            .Select(action => new
            {
                skillName = action.SkillName,
                action = action.Action,
                targetRoot,
                blockedReason = action.BlockedReason,
                diffs = CreateDiffPayloads(action.FileDiffs),
            })
            .ToArray();
    }

    private static object[] CreateDiffPayloads (IReadOnlyList<SkillOperationFileDiffReport> fileDiffs)
    {
        if (fileDiffs.Count == 0)
        {
            return [];
        }

        return
        [
            new
            {
                files = fileDiffs
                    .Select(static file => new
                    {
                        relativePath = file.RelativePath,
                        changeKind = file.ChangeKind,
                        beforeContent = file.BeforeContent,
                        afterContent = file.AfterContent,
                    })
                    .ToArray(),
            },
        ];
    }

    private static int CountAction (
        SkillOperationReport report,
        string action)
    {
        return report.Actions.Count(candidate => string.Equals(candidate.Action, action, StringComparison.Ordinal));
    }

    private static int CountBlocked (SkillOperationReport report)
    {
        return report.Actions.Count(static action =>
            action.Status == SkillOperationActionStatus.Blocked
            || action.BlockedReason is not null);
    }

    private static bool HasDiffs (SkillOperationReport report)
    {
        return report.Actions.Any(static action => action.FileDiffs.Count > 0);
    }

    private static SkillsPackCode ToCode (string code)
    {
        return SkillsPackCode.TryCreate(code, out var parsedCode)
            ? parsedCode
            : SkillsPackCoreErrorCodes.InternalError;
    }
}
