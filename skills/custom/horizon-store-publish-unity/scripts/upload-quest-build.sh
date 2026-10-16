#!/usr/bin/env bash
# upload-quest-build.sh: guarded wrapper around `ovr-platform-util upload-quest-build`.
# Usage: upload-quest-build.sh <apk> <channel> [extra ovr-platform-util flags...]
#   channel: alpha | beta | rc | <custom> | store   (store = production; needs CONFIRM_STORE_UPLOAD=yes)
# Env (required): META_APP_ID, META_AGE_GROUP (TEENS_AND_ADULTS | MIXED_AGES | CHILDREN),
#                 and META_UPLOAD_TOKEN or META_APP_SECRET
# Env (optional): RELEASE_NOTES, OVR_PLATFORM_UTIL (path to binary), QUEST_OBB, QUEST_ASSETS_DIR,
#                 QUEST_ASSET_FILES_CONFIG, QUEST_SYMBOLS_DIR, UPLOAD_AS_DRAFT=1,
#                 QUEST_DEVICE_TARGETS (e.g. "quest3+" or "quest2only|questproonly"; sent as --channel "<channel>:<targets>")
# Flags per developers.meta.com/vr/resources/publish-reference-platform-command-line-utility/ (2026-07-20)
# and /vr/resources/publish-release-channels-device-targeting/ (2026-09-04), verified 2026-10-17.
# Secrets are passed as arguments because the tool requires it, so mask them in CI logs.
set -euo pipefail

APK="${1:?usage: $0 <apk> <channel> [extra flags]}"
CHANNEL="${2:?usage: $0 <apk> <channel> [extra flags]}"
shift 2

TOOL="${OVR_PLATFORM_UTIL:-ovr-platform-util}"
command -v "$TOOL" >/dev/null 2>&1 || { echo "ovr-platform-util not found (metavr tools install platform-utils)" >&2; exit 2; }
[ -f "$APK" ] || { echo "APK not found: $APK" >&2; exit 2; }
: "${META_APP_ID:?set META_APP_ID}"
: "${META_AGE_GROUP:?set META_AGE_GROUP (TEENS_AND_ADULTS | MIXED_AGES | CHILDREN)}"
case "$META_AGE_GROUP" in TEENS_AND_ADULTS|MIXED_AGES|CHILDREN) ;; *) echo "bad META_AGE_GROUP" >&2; exit 2;; esac

channel_lc=$(echo "$CHANNEL" | tr '[:upper:]' '[:lower:]')
if [ "$channel_lc" = "store" ] || [ "$channel_lc" = "production" ] || [ "$channel_lc" = "live" ]; then
  if [ "${CONFIRM_STORE_UPLOAD:-}" != "yes" ]; then
    echo "Refusing to upload to the production channel without CONFIRM_STORE_UPLOAD=yes" >&2
    exit 3
  fi
  CHANNEL="store"
fi

if [ -n "${QUEST_DEVICE_TARGETS:-}" ]; then
  case "$QUEST_DEVICE_TARGETS" in
    *[!a-z0-9+\|-]*) echo "bad QUEST_DEVICE_TARGETS (use specifiers like quest3+ or quest2only|questproonly)" >&2; exit 2;;
  esac
  CHANNEL="$CHANNEL:$QUEST_DEVICE_TARGETS"
fi

AUTH=()
if [ -n "${META_UPLOAD_TOKEN:-}" ]; then AUTH=(--token "$META_UPLOAD_TOKEN")
elif [ -n "${META_APP_SECRET:-}" ]; then AUTH=(--app-secret "$META_APP_SECRET")
else echo "set META_UPLOAD_TOKEN or META_APP_SECRET" >&2; exit 2; fi

ARGS=(upload-quest-build --app-id "$META_APP_ID" "${AUTH[@]}" --apk "$APK"
      --channel "$CHANNEL" --age-group "$META_AGE_GROUP" --disable-progress-bar)
[ -n "${RELEASE_NOTES:-}" ] && ARGS+=(--notes "$RELEASE_NOTES")
[ -n "${QUEST_OBB:-}" ] && ARGS+=(--obb "$QUEST_OBB")
[ -n "${QUEST_ASSETS_DIR:-}" ] && ARGS+=(--assets-dir "$QUEST_ASSETS_DIR")
[ -n "${QUEST_ASSET_FILES_CONFIG:-}" ] && ARGS+=(--asset-files-config "$QUEST_ASSET_FILES_CONFIG")
if [ -n "${QUEST_SYMBOLS_DIR:-}" ]; then ARGS+=(--debug-symbols-dir "$QUEST_SYMBOLS_DIR" --debug-symbols-pattern "*.sym.so"); fi
[ "${UPLOAD_AS_DRAFT:-}" = "1" ] && ARGS+=(--draft)

echo "Uploading $(basename "$APK") to channel '$CHANNEL' for app $META_APP_ID (age group $META_AGE_GROUP)..."
"$TOOL" "${ARGS[@]}" "$@"
