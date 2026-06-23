namespace MackySoft.SkillsPack.Hosting.Cli.Common.Contracts;

internal sealed record CommandError (
    SkillsPackCode Code,
    string Message,
    string? OpId);
