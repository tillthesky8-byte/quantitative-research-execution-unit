#!/usr/bin/env bash
set -euo pipefail

repo_root="$(git rev-parse --show-toplevel)"
cd "$repo_root"

paths=("$@")

printf '%s\n' '--- STATUS ---'
git status --short

printf '\n%s\n' '--- DIFFSTAT ---'
if (( ${#paths[@]} > 0 )); then
  git diff --stat -- "${paths[@]}"
else
  git diff --stat
fi

printf '\n%s\n' '--- DIFF ---'
if (( ${#paths[@]} > 0 )); then
  git diff -- "${paths[@]}"
else
  git diff
fi

printf '\n%s\n' '--- UNTRACKED FILES ---'
untracked=()
while IFS= read -r file; do
  if [[ -n "$file" ]]; then
    untracked+=("$file")
  fi
done < <(git ls-files --others --exclude-standard)

if (( ${#untracked[@]} == 0 )); then
  printf '%s\n' 'None'
else
  printf '%s\n' "${untracked[@]}"
fi

if (( ${#untracked[@]} > 0 )); then
  printf '\n%s\n' '--- UNTRACKED DIFFS ---'
  for file in "${untracked[@]}"; do
    printf '\n%s\n' "diff --no-index /dev/null $file"
    git diff --no-index -- /dev/null "$file" || true
  done
fi

printf '\n%s\n' '--- COMMIT MESSAGE HINT ---'
printf '%s\n' 'Use the diff above to write a conventional commit like: feat(scope): short summary'
