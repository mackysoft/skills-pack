# SkillsPack - Reusable Agent Skills for repositories and environments

[![verify](https://github.com/mackysoft/skills-pack/actions/workflows/verify.yaml/badge.svg)](https://github.com/mackysoft/skills-pack/actions/workflows/verify.yaml) [![NuGet](https://img.shields.io/nuget/v/MackySoft.SkillsPack?label=MackySoft.SkillsPack)](https://www.nuget.org/packages/MackySoft.SkillsPack) [![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Created by Hiroya Aramaki ([Makihiro](https://twitter.com/makihiro_dev))**

SkillsPack distributes reusable Agent Skills as a .NET global tool.

Use it for skills that should be shared across repositories and development environments instead of copied by hand.
The tool packages SKILL definitions and provides list, export, install, update, doctor, and uninstall commands for supported agent hosts.

## Included Skills

SkillsPack includes these reusable development workflow skills:

| Skill | Tier | Purpose |
| --- | --- | --- |
| `branch-create` | `development` | Create or reuse task branches with repository naming rules. |
| `changelog` | `development` | Write reader-facing changelogs, release notes, and PR change summaries. |
| `code-quality-review` | `development` | Review code for duplication, low cohesion, excess state, and unnecessary abstraction. |
| `commit` | `development` | Create responsibility-scoped Conventional Commit messages. |
| `pr-create` | `development` | Verify, push, and create or update pull requests. |
| `pr-merge` | `development` | Merge pull requests through CI and branch cleanup. |
| `push` | `development` | Commit pending work when needed and push the current branch safely. |
| `review-triage` | `development` | Triage review comments against code, specifications, and evidence. |
| `test-writer` | `development` | Design and create contract-based tests. |
| `ultra-review` | `development` | Run deeper multi-pass review and apply safe fixes. |
| `verification-gate` | `development` | Run the verification needed before PRs or final checks. |
| `xml-doc-writer` | `development` | Write contract-focused XML documentation comments. |

SkillsPack defines these tiers:

| Tier | Purpose |
| --- | --- |
| `general` | General-purpose reusable workflow skills. |
| `development` | Code, review, test, Git, and pull request workflow skills. |
| `personal` | Skills for the author's personal environment and workflow setup. |

Empty tiers are valid.
They are still reported by `skills-pack skills list` so automation can discover the full supported tier set.

## Install

```bash
dotnet tool install --global MackySoft.SkillsPack
```

Update the tool to receive the latest bundled skills:

```bash
dotnet tool update --global MackySoft.SkillsPack
```

## CLI Usage

List bundled skills, supported hosts, tiers, and package counts:

```bash
skills-pack skills list
skills-pack skills list --tier development
skills-pack skills list --tier general,development
```

`skills list` treats `--tier` as optional.
When omitted, it reports every defined tier.

Export host-specific skill files:

```bash
skills-pack skills export --host openai --tier development --output ./exported-skills
```

Install skills into a repository:

```bash
skills-pack skills install --host openai --scope project --repo-root . --tier development
```

Update installed skills:

```bash
skills-pack skills update --host openai --scope project --repo-root . --tier development
```

Diagnose installed skills:

```bash
skills-pack skills doctor --host openai --scope project --repo-root . --tier development
```

Uninstall managed skills:

```bash
skills-pack skills uninstall --host openai --scope project --repo-root . --tier development
```

Install skills into the current user's host skill directory:

```bash
skills-pack skills install --host openai --scope user --tier development
```

For `export`, `install`, `update`, `doctor`, and `uninstall`, `--tier` is required.
Multiple tiers can be selected with a comma-separated value:

```bash
skills-pack skills install --host openai --scope project --repo-root . --tier general,development
```

Use `--dry-run` before changing installed files:

```bash
skills-pack skills install --host openai --scope project --repo-root . --tier development --dry-run --print-diff
skills-pack skills doctor --host openai --scope project --repo-root . --tier development
```

Supported hosts are reported by `skills-pack skills list`.

## Author

Hiroya Aramaki is an indie game developer in Japan.

- Website: <https://mackysoft.net/>
- GitHub: <https://github.com/mackysoft>

## License

SkillsPack is under the [MIT License](LICENSE).
