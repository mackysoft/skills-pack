#!/usr/bin/env bash
set -euo pipefail

if [[ "$#" -ne 2 ]]; then
  echo "Usage: $0 <package-dir> <expected-version>" >&2
  exit 2
fi

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "${script_dir}/.." && pwd)"
package_dir="$1"
expected_version="$2"

if [[ ! -d "${package_dir}" ]]; then
  echo "CLI package directory does not exist: ${package_dir}" >&2
  exit 1
fi

package_dir="$(cd "${package_dir}" && pwd)"
package_path="${package_dir}/MackySoft.SkillsPack.${expected_version}.nupkg"
if [[ ! -f "${package_path}" ]]; then
  echo "CLI package was not created: ${package_path}" >&2
  exit 1
fi

temp_root="${RUNNER_TEMP:-${TMPDIR:-/tmp}}"
tool_path="$(mktemp -d "${temp_root%/}/skills-pack-tool.XXXXXX")"
install_repo=""
trap 'rm -rf "${tool_path}" "${install_repo}"' EXIT

dotnet tool install \
  --tool-path "${tool_path}" \
  --add-source "${package_dir}" \
  MackySoft.SkillsPack \
  --version "${expected_version}"

actual_version="$("${tool_path}/skills-pack" --version)"
if [[ "${actual_version}" != "${expected_version}" ]]; then
  echo "Unexpected skills-pack --version. Expected: ${expected_version}. Actual: ${actual_version}" >&2
  exit 1
fi

package_entries="$(unzip -Z1 "${package_path}")"
for entry in README.md LICENSE tools/net10.0/any/DotnetToolSettings.xml; do
  if ! grep -Fx "${entry}" <<< "${package_entries}" >/dev/null; then
    echo "CLI package is missing required entry: ${entry}" >&2
    exit 1
  fi
done

generated_skills_root="${repo_root}/skills/generated"
while IFS= read -r skill_file; do
  relative_path="${skill_file#"${generated_skills_root}/"}"
  entry="tools/net10.0/any/skills/${relative_path}"
  if ! grep -Fx "${entry}" <<< "${package_entries}" >/dev/null; then
    echo "CLI package is missing required generated SKILL entry: ${entry}" >&2
    exit 1
  fi
done < <(find "${generated_skills_root}" -type f | sort)

skills_list="$("${tool_path}/skills-pack" skills list)"
if ! grep -F '"command":"skills.list"' <<< "${skills_list}" >/dev/null; then
  echo "skills-pack skills list did not report the skills.list command." >&2
  exit 1
fi

if ! grep -F '"skillName":"commit"' <<< "${skills_list}" >/dev/null; then
  echo "skills-pack skills list did not include bundled SKILL packages." >&2
  exit 1
fi

if ! grep -F '"skillName":"changelog"' <<< "${skills_list}" >/dev/null; then
  echo "skills-pack skills list did not include changelog." >&2
  exit 1
fi

SKILLS_LIST_JSON="${skills_list}" python3 - <<'PY'
import json
import os
import sys

root = json.loads(os.environ["SKILLS_LIST_JSON"])
payload = root.get("payload") or {}
skill_names = payload.get("skillNames")
if skill_names != []:
    print(
        f"skills-pack skills list did not report an empty skillNames selection for unfiltered list. Actual: {skill_names}",
        file=sys.stderr,
    )
    sys.exit(1)
actual = [
    (tier.get("tier"), tier.get("skillCount"))
    for tier in payload.get("availableTiers", [])
]
expected = [
    ("general", 1),
    ("development", 16),
    ("personal", 0),
]
if actual != expected:
    print(
        f"skills-pack skills list did not report expected availableTiers. Expected: {expected}. Actual: {actual}",
        file=sys.stderr,
    )
    sys.exit(1)
PY

single_skill_list="$("${tool_path}/skills-pack" skills list --skill changelog)"
SINGLE_SKILL_LIST_JSON="${single_skill_list}" python3 - <<'PY'
import json
import os
import sys

root = json.loads(os.environ["SINGLE_SKILL_LIST_JSON"])
payload = root.get("payload") or {}
tiers = payload.get("tiers")
skill_names = payload.get("skillNames")
skills = payload.get("skills", [])
actual_names = [skill.get("skillName") for skill in skills]
if tiers != ["general", "development", "personal"] or skill_names != ["changelog"] or actual_names != ["changelog"]:
    print(
        f"skills-pack skills list did not support exact skill selection. Actual tiers: {tiers}. Actual skillNames: {skill_names}. Actual skills: {actual_names}",
        file=sys.stderr,
    )
    sys.exit(1)
PY

dependency_skill_list="$("${tool_path}/skills-pack" skills list --skill pr-merge)"
DEPENDENCY_SKILL_LIST_JSON="${dependency_skill_list}" python3 - <<'PY'
import json
import os
import sys

root = json.loads(os.environ["DEPENDENCY_SKILL_LIST_JSON"])
payload = root.get("payload") or {}
skill_names = payload.get("skillNames")
skills = payload.get("skills", [])
actual_names = [skill.get("skillName") for skill in skills]
expected_names = [
    "branch-create",
    "commit",
    "pr-create",
    "pr-merge",
    "push",
    "verification-gate",
]
dependencies_by_skill = {
    skill.get("skillName"): skill.get("dependencies")
    for skill in skills
}
if skill_names != ["pr-merge"] or actual_names != expected_names:
    print(
        f"skills-pack skills list did not include transitive dependencies for exact skill selection. Expected root ['pr-merge'] and skills {expected_names}. Actual root: {skill_names}. Actual skills: {actual_names}",
        file=sys.stderr,
    )
    sys.exit(1)
if dependencies_by_skill.get("pr-merge") != ["branch-create", "pr-create", "push"]:
    print(
        f"skills-pack skills list did not report pr-merge dependencies. Actual: {dependencies_by_skill.get('pr-merge')}",
        file=sys.stderr,
    )
    sys.exit(1)
if any(skill.get("skillBundleVersion") != 1 for skill in skills):
    print(
        f"skills-pack skills list did not report skillBundleVersion 1 for dependency selection. Actual: {[skill.get('skillBundleVersion') for skill in skills]}",
        file=sys.stderr,
    )
    sys.exit(1)
PY

multi_tier_list="$("${tool_path}/skills-pack" skills list --tier general,development)"
MULTI_TIER_LIST_JSON="${multi_tier_list}" python3 - <<'PY'
import json
import os
import sys

root = json.loads(os.environ["MULTI_TIER_LIST_JSON"])
payload = root.get("payload") or {}
tiers = payload.get("tiers")
skill_count = len(payload.get("skills", []))
if tiers != ["general", "development"] or skill_count != 17:
    print(
        f"skills-pack skills list did not support comma-separated tier selection. Expected tiers ['general', 'development'] with 17 skills. Actual tiers: {tiers}. Actual skill count: {skill_count}",
        file=sys.stderr,
    )
    sys.exit(1)
PY

export_path="${tool_path}/exported-skills"
"${tool_path}/skills-pack" skills export --host openai --tier development --output "${export_path}" >/dev/null
if [[ ! -f "${export_path}/commit/SKILL.md" ]]; then
  echo "skills-pack skills export did not materialize commit/SKILL.md." >&2
  exit 1
fi

if [[ ! -f "${export_path}/changelog/SKILL.md" ]]; then
  echo "skills-pack skills export did not materialize changelog/SKILL.md." >&2
  exit 1
fi

single_skill_export_path="${tool_path}/exported-single-skill"
"${tool_path}/skills-pack" skills export --host openai --skill changelog --output "${single_skill_export_path}" >/dev/null
if [[ ! -f "${single_skill_export_path}/changelog/SKILL.md" ]]; then
  echo "skills-pack skills export did not materialize changelog/SKILL.md for exact skill selection." >&2
  exit 1
fi

if [[ -e "${single_skill_export_path}/commit" ]]; then
  echo "skills-pack skills export materialized an unselected skill for exact skill selection." >&2
  exit 1
fi

dependency_skill_export_path="${tool_path}/exported-dependency-skill"
"${tool_path}/skills-pack" skills export --host openai --skill pr-merge --output "${dependency_skill_export_path}" >/dev/null
for expected_skill in branch-create commit pr-create pr-merge push verification-gate; do
  if [[ ! -f "${dependency_skill_export_path}/${expected_skill}/SKILL.md" ]]; then
    echo "skills-pack skills export did not materialize dependency skill ${expected_skill}/SKILL.md for exact skill selection." >&2
    exit 1
  fi
done

if [[ -e "${dependency_skill_export_path}/review-triage" ]]; then
  echo "skills-pack skills export materialized an unrelated skill for dependency exact skill selection." >&2
  exit 1
fi

install_repo="$(mktemp -d "${temp_root%/}/skills-pack-install.XXXXXX")"
"${tool_path}/skills-pack" skills install --host openai --tier development --scope project --repo-root "${install_repo}" >/dev/null
"${tool_path}/skills-pack" skills doctor --host openai --tier development --scope project --repo-root "${install_repo}" >/dev/null

if ! diff -ruN "${export_path}" "${install_repo}/.agents/skills"; then
  echo "skills-pack install output differs from export output for the same host." >&2
  exit 1
fi
