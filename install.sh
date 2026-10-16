#!/usr/bin/env bash
# install.sh: copy skills from this library into a Claude Code skills folder.
# Part of unity-meta-quest-skills (MIT): https://github.com/mirzaaghazadeh/unity-meta-quest-skills
#
# Bundles come from .claude-plugin/marketplace.json, so this script and the
# Claude Code plugins always install the same sets.
set -euo pipefail

REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MARKETPLACE="$REPO_DIR/.claude-plugin/marketplace.json"

usage() {
  cat <<'EOF'
Usage: ./install.sh [target] [options] <bundle|skill>...

Targets (pick one):
  --project DIR   install into DIR/.claude/skills   (one game project; recommended)
  --user          install into ~/.claude/skills     (every project on this machine)
  --dest DIR      install into DIR as-is            (other agents, e.g. DIR/.agents/skills)

Options:
  --link          symlink instead of copy, so `git pull` here updates the skills
  --force         replace skills that already exist in the target
  --dry-run       print what would happen, change nothing
  --uninstall     remove the named bundles/skills from the target
  --list          list bundles and their skills, then exit
  -h, --help      show this help

Bundles: quest-core, meta-quest-extras, unity-extras, community-unity-xr,
         community-design-art, or "all". Any single skill name works too.

Examples:
  ./install.sh --project ~/Games/MyQuestGame quest-core
  ./install.sh --user quest-core unity-extras
  ./install.sh --project . --link quest-core scenario-unity-shaders
  ./install.sh --project ~/Games/MyQuestGame --uninstall community-design-art
EOF
}

die() { echo "error: $*" >&2; exit 1; }

command -v python3 >/dev/null 2>&1 || die "python3 is required to read the bundle list (install Xcode Command Line Tools or Python 3)."
[ -f "$MARKETPLACE" ] || die "can't find $MARKETPLACE. Run this script from a full clone of the repository."

# Prints "<bundle>\t<skill path relative to repo>" for every bundled skill.
bundle_table() {
  python3 -I - "$MARKETPLACE" <<'PY'
import json, sys
with open(sys.argv[1]) as f:
    data = json.load(f)
for plugin in data["plugins"]:
    for path in plugin.get("skills", []):
        print(plugin["name"] + "\t" + path[2:] if path.startswith("./") else plugin["name"] + "\t" + path)
PY
}

TARGET="" ; MODE="copy" ; FORCE=0 ; DRY=0 ; UNINSTALL=0 ; LIST=0
NAMES=()
while [ $# -gt 0 ]; do
  case "$1" in
    --project) [ $# -ge 2 ] || die "--project needs a directory"; TARGET="$2/.claude/skills"; shift 2 ;;
    --user)    TARGET="$HOME/.claude/skills"; shift ;;
    --dest)    [ $# -ge 2 ] || die "--dest needs a directory"; TARGET="$2"; shift 2 ;;
    --link)    MODE="link"; shift ;;
    --force)   FORCE=1; shift ;;
    --dry-run) DRY=1; shift ;;
    --uninstall) UNINSTALL=1; shift ;;
    --list)    LIST=1; shift ;;
    -h|--help) usage; exit 0 ;;
    -*)        die "unknown option: $1 (see --help)" ;;
    *)         NAMES+=("$1"); shift ;;
  esac
done

TABLE="$(bundle_table)"

if [ "$LIST" -eq 1 ]; then
  current=""
  while IFS=$'\t' read -r bundle path; do
    if [ "$bundle" != "$current" ]; then
      count=$(printf '%s\n' "$TABLE" | awk -F'\t' -v b="$bundle" '$1==b' | wc -l | tr -d ' ')
      printf '\n%s (%s skills)\n' "$bundle" "$count"
      current="$bundle"
    fi
    printf '  %-48s %s\n' "$(basename "$path")" "$(dirname "$path")"
  done <<< "$TABLE"
  echo
  exit 0
fi

[ ${#NAMES[@]} -gt 0 ] || { usage; exit 1; }
[ -n "$TARGET" ] || die "choose where to install: --project DIR, --user or --dest DIR (see --help)."

# Resolve bundle and skill names to skill paths (deduplicated, order kept).
PATHS=()
add_path() {
  local p="$1" existing
  for existing in ${PATHS[@]+"${PATHS[@]}"}; do [ "$existing" = "$p" ] && return 0; done
  PATHS+=("$p")
}
for name in "${NAMES[@]}"; do
  matched=0
  while IFS=$'\t' read -r bundle path; do
    if [ "$name" = "all" ] || [ "$name" = "$bundle" ] || [ "$name" = "$(basename "$path")" ]; then
      add_path "$path"; matched=1
    fi
  done <<< "$TABLE"
  [ "$matched" -eq 1 ] || die "no bundle or skill named '$name'. Run ./install.sh --list to see them."
done

[ "$DRY" -eq 1 ] && echo "(dry run: nothing will change)"
[ "$DRY" -eq 1 ] || mkdir -p "$TARGET"
TARGET="$(cd "$TARGET" 2>/dev/null && pwd || echo "$TARGET")"

installed=0 ; skipped=0 ; removed=0
for path in "${PATHS[@]}"; do
  skill="$(basename "$path")"
  src="$REPO_DIR/$path"
  dst="$TARGET/$skill"
  case "$skill" in ""|.|..|*/*) die "refusing unsafe skill name '$skill'" ;; esac
  [ -f "$src/SKILL.md" ] || die "missing $src/SKILL.md"

  if [ "$UNINSTALL" -eq 1 ]; then
    if [ -e "$dst" ] || [ -L "$dst" ]; then
      echo "remove   $skill"
      [ "$DRY" -eq 1 ] || rm -rf -- "$dst"
      removed=$((removed + 1))
    fi
    continue
  fi

  if [ -e "$dst" ] || [ -L "$dst" ]; then
    if [ "$FORCE" -ne 1 ]; then
      echo "skip     $skill (already there; use --force to replace)"
      skipped=$((skipped + 1))
      continue
    fi
    [ "$DRY" -eq 1 ] || rm -rf -- "$dst"
  fi

  if [ "$MODE" = "link" ]; then
    echo "link     $skill -> $path"
    [ "$DRY" -eq 1 ] || ln -s "$src" "$dst"
  else
    echo "copy     $skill"
    [ "$DRY" -eq 1 ] || cp -R "$src" "$dst"
  fi
  installed=$((installed + 1))
done

echo
verb_rm="Removed" ; verb_in="Installed"
[ "$DRY" -eq 1 ] && { verb_rm="Would remove" ; verb_in="Would install" ; }
if [ "$UNINSTALL" -eq 1 ]; then
  echo "$verb_rm $removed skill(s) from $TARGET"
else
  echo "$verb_in $installed skill(s) into $TARGET ($skipped skipped)"
  if [ "$DRY" -ne 1 ] && [ "$installed" -gt 0 ]; then
    cat <<EOF

Next:
  1. Start a new Claude Code session (or run /reload-plugins) so the skills load.
  2. Ask: "Use quest-dev-workflow to plan my Quest game" and follow the steps it gives.
  3. Don't also install the meta-vr or unity@unity-agent-plugin plugins, or the same
     bundles as Claude Code plugins: Claude would see duplicate skills.
EOF
  fi
fi
