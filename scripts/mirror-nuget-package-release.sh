#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat >&2 <<'EOF'
Usage: scripts/mirror-nuget-package-release.sh --repository <owner/repo> --tag-name <tag> --package-glob <glob> --title <title> [--notes <notes> | --generate-notes]

Creates or updates the GitHub Release for a package tag and uploads matched nupkg artifacts.
Use --generate-notes to generate release notes from GitHub changes since the previous release.
EOF
}

repository=""
tag_name=""
package_glob=""
release_title=""
release_notes=""
notes_provided=false
generate_notes=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --repository)
      [[ $# -ge 2 ]] || { usage; exit 2; }
      repository="$2"
      shift 2
      ;;
    --tag-name)
      [[ $# -ge 2 ]] || { usage; exit 2; }
      tag_name="$2"
      shift 2
      ;;
    --package-glob)
      [[ $# -ge 2 ]] || { usage; exit 2; }
      package_glob="$2"
      shift 2
      ;;
    --title)
      [[ $# -ge 2 ]] || { usage; exit 2; }
      release_title="$2"
      shift 2
      ;;
    --notes)
      [[ $# -ge 2 ]] || { usage; exit 2; }
      release_notes="$2"
      notes_provided=true
      shift 2
      ;;
    --generate-notes)
      generate_notes=true
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      usage
      exit 2
      ;;
  esac
done

if [[ -z "${repository}" || -z "${tag_name}" || -z "${package_glob}" || -z "${release_title}" ]]; then
  usage
  exit 2
fi

if [[ "${notes_provided}" == "true" && "${generate_notes}" == "true" ]]; then
  echo "--notes and --generate-notes cannot be used together." >&2
  exit 2
fi

package_paths=()
while IFS= read -r package_path; do
  package_paths+=("${package_path}")
done < <(compgen -G "${package_glob}" | sort)
if [[ ${#package_paths[@]} -eq 0 ]]; then
  echo "No NuGet package artifacts matched: ${package_glob}" >&2
  exit 1
fi

if [[ "${generate_notes}" == "true" ]]; then
  release_notes="$(
    gh api --method POST "repos/${repository}/releases/generate-notes" \
      --field "tag_name=${tag_name}" \
      --jq '.body'
  )"
fi

if gh release view "${tag_name}" --repo "${repository}" >/dev/null 2>&1; then
  gh release edit "${tag_name}" \
    --repo "${repository}" \
    --title "${release_title}" \
    --notes "${release_notes}"
  gh release upload "${tag_name}" "${package_paths[@]}" --repo "${repository}" --clobber
else
  gh release create "${tag_name}" "${package_paths[@]}" \
    --repo "${repository}" \
    --title "${release_title}" \
    --notes "${release_notes}"
fi
