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
trap 'rm -rf "${tool_path}"' EXIT

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

bundle_entry="tools/net10.0/any/skills/bundle.json"
expected_bundle_path="${generated_skills_root}/bundle.json"
PACKAGE_PATH="${package_path}" BUNDLE_ENTRY="${bundle_entry}" EXPECTED_BUNDLE_PATH="${expected_bundle_path}" python3 - <<'PY'
import json
import os
import sys
import zipfile

with zipfile.ZipFile(os.environ["PACKAGE_PATH"]) as package:
    bundle = json.loads(package.read(os.environ["BUNDLE_ENTRY"]))

with open(os.environ["EXPECTED_BUNDLE_PATH"], encoding="utf-8") as descriptor:
    expected = json.load(descriptor)

if bundle != expected:
    print(
        f"CLI package bundle descriptor does not match the repository generated descriptor. Expected: {expected}. Actual: {bundle}",
        file=sys.stderr,
    )
    sys.exit(1)
PY

skills_list="$("${tool_path}/skills-pack" skills list)"
if ! grep -F '"command":"skills.list"' <<< "${skills_list}" >/dev/null; then
  echo "skills-pack skills list did not report the skills.list command." >&2
  exit 1
fi
