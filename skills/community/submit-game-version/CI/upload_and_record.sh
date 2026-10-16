#!/usr/bin/env bash
#
# upload_and_record.sh — uploads an already-built signed release APK to the Meta
# Horizon Store BETA channel and records the submitted version in
# CI/LastSubmittedVersion.md.
#
# Called by the "submit-game-version" skill after it drives the OPEN Unity editor to
# build via ReleaseBuilder.BuildAndroidReleaseInEditor (Unity stays open the whole time).
#
# By default it reads the build result written by ReleaseBuilder at
# Builds/last_release_result.txt (version=, code=, apk=). Override with flags:
#   --apk PATH --version VER --code CODE
#
# Requires CI/release.local.env (gitignored) with META_APP_ID + META_APP_SECRET.
#
set -euo pipefail

# --- Paths ---------------------------------------------------------------------
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
ENV_FILE="${SCRIPT_DIR}/release.local.env"
BUILD_DIR="${ROOT_DIR}/Builds"
RESULT_FILE="${BUILD_DIR}/last_release_result.txt"
VERSION_FILE="${SCRIPT_DIR}/LastSubmittedVersion.md"
NOTES_FILE="${SCRIPT_DIR}/release_notes.md"   # pending release notes for the NEXT build (cleared after submit)
CHANNEL="BETA"

# ovr-platform-util ships with Meta Quest Developer Hub; fall back to PATH.
OVR_UTIL="${HOME}/Library/Application Support/odh/ovr-platform-util"
if [[ ! -x "${OVR_UTIL}" ]]; then
  OVR_UTIL="$(command -v ovr-platform-util || true)"
fi

err() { echo "ERROR: $*" >&2; exit 1; }

# --- Args (optional overrides) -------------------------------------------------
APK=""; VERSION=""; CODE=""; NOTES_OVERRIDE=""
while [[ $# -gt 0 ]]; do
  case "$1" in
    --apk)     APK="$2";     shift 2 ;;
    --version) VERSION="$2"; shift 2 ;;
    --code)    CODE="$2";    shift 2 ;;
    --notes)   NOTES_OVERRIDE="$2"; shift 2 ;;
    *) err "Unknown argument: $1" ;;
  esac
done

# --- Fill any missing fields from the result file written by ReleaseBuilder -----
if [[ -z "${APK}" || -z "${VERSION}" || -z "${CODE}" ]]; then
  [[ -f "${RESULT_FILE}" ]] || err "No build result. Pass --apk/--version/--code, or build first (expected ${RESULT_FILE})."
  [[ -z "${VERSION}" ]] && VERSION="$(sed -n 's/^version=//p' "${RESULT_FILE}")"
  [[ -z "${CODE}" ]]    && CODE="$(sed -n 's/^code=//p' "${RESULT_FILE}")"
  [[ -z "${APK}" ]]     && APK="$(sed -n 's/^apk=//p' "${RESULT_FILE}")"
fi

[[ -n "${VERSION}" && -n "${CODE}" && -n "${APK}" ]] || err "Incomplete build result (version='${VERSION}' code='${CODE}' apk='${APK}')."
[[ -f "${APK}" ]] || err "APK not found at ${APK}"

# --- Preconditions -------------------------------------------------------------
[[ -f "${ENV_FILE}" ]] || err "Missing ${ENV_FILE}.
Copy CI/release.local.env.example to CI/release.local.env and fill in your Meta
App ID/secret. (The file is gitignored.)"

# shellcheck disable=SC1090
set -a; source "${ENV_FILE}"; set +a

: "${META_APP_ID:?META_APP_ID not set in release.local.env}"
: "${META_APP_SECRET:?META_APP_SECRET not set in release.local.env}"
[[ -n "${OVR_UTIL}" && -x "${OVR_UTIL}" ]] || err "ovr-platform-util not found (install Meta Quest Developer Hub)."

# --- Resolve release notes (shown to users on the build) -----------------------
# Priority: --notes override > CI/release_notes.md (the running "what's new since the
# last build") > a generic fallback. The notes file is cleared after a successful submit
# so the next build's notes only cover NEW changes.
USED_NOTES_FILE=false
if [[ -n "${NOTES_OVERRIDE}" ]]; then
  NOTES="${NOTES_OVERRIDE}"
elif [[ -f "${NOTES_FILE}" ]] && [[ -n "$(tr -d '[:space:]' < "${NOTES_FILE}")" ]]; then
  NOTES="$(cat "${NOTES_FILE}")"
  USED_NOTES_FILE=true
else
  NOTES="ArcadeHoopsMR ${VERSION} (code ${CODE})"
fi

# --- 1. Upload to Meta Horizon Store ------------------------------------------
echo ">> Uploading ${VERSION} (code ${CODE}) to the Meta Horizon Store (${CHANNEL} channel)..."
echo "   ${APK}"
echo "   Release notes:"
echo "${NOTES}" | sed 's/^/     /'
  # Use the --notes=VALUE (attached '=') form: the notes are a dash-bulleted list, and the
  # CLI's arg parser would otherwise treat a value starting with '-' as another flag
  # ("Not enough arguments following: notes").
"${OVR_UTIL}" upload-quest-build \
  --app-id "${META_APP_ID}" \
  --app-secret "${META_APP_SECRET}" \
  --apk "${APK}" \
  --channel "${CHANNEL}" \
  --notes="${NOTES}"

# --- 2. Record the submitted version ------------------------------------------
SUBMITTED_DATE="$(date +%Y-%m-%d)"
{
  echo "# Last Submitted Version"
  echo
  echo "- **Version:** ${VERSION}"
  echo "- **Bundle Version Code:** ${CODE}"
  echo "- **Channel:** ${CHANNEL}"
  echo "- **Submitted:** ${SUBMITTED_DATE}"
  echo
  echo "## Release Notes"
  echo
  echo "${NOTES}"
  echo
  echo "## Recent Commits"
  git -C "${ROOT_DIR}" log -5 --pretty=format:'%s' | nl -w1 -s'. '
  echo
} >"${VERSION_FILE}"

# Consumed the pending notes — clear them so the next build's notes only cover NEW changes.
if [[ "${USED_NOTES_FILE}" == true ]]; then
  : >"${NOTES_FILE}"
fi

echo
echo "✅ Submitted ${VERSION} (code ${CODE}) to the ${CHANNEL} channel."
echo "   Recorded in ${VERSION_FILE}."
if [[ "${USED_NOTES_FILE}" == true ]]; then
  echo "   Cleared ${NOTES_FILE} (next build starts a fresh notes list)."
fi
echo "   Version bump + LastSubmittedVersion.md are left uncommitted for your review."
