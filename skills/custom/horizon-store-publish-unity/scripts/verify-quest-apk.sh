#!/usr/bin/env bash
# verify-quest-apk.sh: pre-upload checks of a Quest release APK against Meta Horizon Store
# packaging rules (publish-mobile-manifest, VRC.Quest.Packaging.1-6, Security.2).
# Usage: verify-quest-apk.sh path/to/app.apk [--2d]
# Needs Android build-tools (aapt2, apksigner). Set ANDROID_BUILD_TOOLS=/path/to/build-tools/<ver>
# if they are not on PATH; the script also searches ANDROID_HOME and Unity Hub installs.
# Exit code: 0 = no FAIL, 1 = at least one FAIL, 2 = usage/tooling error.
# Rules verified 2026-10-17 (publish-mobile-manifest 2026-10-15, essentials/app-compatibility 2026-10-13).
# Re-verify with: metavr docs search "release build manifest requirements"
set -u

APK="${1:-}"
MODE="immersive"
[ "${2:-}" = "--2d" ] && MODE="2d"
if [ -z "$APK" ] || [ ! -f "$APK" ]; then
  echo "usage: $0 app.apk [--2d]" >&2
  exit 2
fi

find_tool() {
  local name="$1" dir
  if [ -n "${ANDROID_BUILD_TOOLS:-}" ] && [ -x "$ANDROID_BUILD_TOOLS/$name" ]; then
    echo "$ANDROID_BUILD_TOOLS/$name"; return 0
  fi
  if command -v "$name" >/dev/null 2>&1; then command -v "$name"; return 0; fi
  for dir in "${ANDROID_HOME:-}/build-tools" "${ANDROID_SDK_ROOT:-}/build-tools" \
             /Applications/Unity/Hub/Editor/*/PlaybackEngines/AndroidPlayer/SDK/build-tools \
             "$HOME/Library/Android/sdk/build-tools"; do
    [ -d "$dir" ] || continue
    local hit
    hit=$(ls -d "$dir"/*/ 2>/dev/null | sort -V | tail -n 1)
    if [ -n "$hit" ] && [ -x "${hit}${name}" ]; then echo "${hit}${name}"; return 0; fi
  done
  return 1
}

AAPT2=$(find_tool aapt2) || { echo "aapt2 not found; set ANDROID_BUILD_TOOLS" >&2; exit 2; }
APKSIGNER=$(find_tool apksigner) || { echo "apksigner not found; set ANDROID_BUILD_TOOLS" >&2; exit 2; }

# apksigner is a Java wrapper. If no working java is on PATH, fall back to the OpenJDK that ships
# beside Unity's Android SDK (.../AndroidPlayer/SDK/build-tools/<ver>/ -> .../AndroidPlayer/OpenJDK).
if ! java -version >/dev/null 2>&1; then
  UNITY_JDK="$(cd "$(dirname "$APKSIGNER")/../../.." 2>/dev/null && pwd)/OpenJDK"
  if [ -x "$UNITY_JDK/bin/java" ]; then
    export JAVA_HOME="$UNITY_JDK" PATH="$UNITY_JDK/bin:$PATH"
  else
    echo "no java found; install a JDK or set JAVA_HOME (Unity's AndroidPlayer/OpenJDK works)" >&2; exit 2
  fi
fi

FAILS=0
pass() { echo "PASS  $*"; }
warn() { echo "WARN  $*"; }
fail() { echo "FAIL  $*"; FAILS=$((FAILS + 1)); }

BADGING=$("$AAPT2" dump badging "$APK" 2>/dev/null) || { echo "aapt2 dump badging failed" >&2; exit 2; }
XMLTREE=$("$AAPT2" dump xmltree --file AndroidManifest.xml "$APK" 2>/dev/null) || XMLTREE=""

pkg=$(echo "$BADGING" | sed -n "s/^package: name='\([^']*\)'.*/\1/p")
vcode=$(echo "$BADGING" | sed -n "s/^package: .* versionCode='\([^']*\)'.*/\1/p")
vname=$(echo "$BADGING" | sed -n "s/^package: .* versionName='\([^']*\)'.*/\1/p")
csdk=$(echo "$BADGING" | sed -n "s/^package: .* compileSdkVersion='\([^']*\)'.*/\1/p")
# aapt2 prints minSdkVersion:'32'; the old aapt printed sdkVersion:'32'.
minsdk=$(echo "$BADGING" | sed -n -e "s/^minSdkVersion:'\([0-9]*\)'.*/\1/p" -e "s/^sdkVersion:'\([0-9]*\)'.*/\1/p" | head -n 1)
tsdk=$(echo "$BADGING" | sed -n "s/^targetSdkVersion:'\([0-9]*\)'.*/\1/p")
echo "APK: $APK"
echo "package=$pkg versionCode=$vcode versionName=$vname minSdk=$minsdk targetSdk=$tsdk compileSdk=${csdk:-unset} mode=$MODE"
echo "----"

# Package name
case "$pkg" in
  com.oculus.*|com.meta.*|com.example.*|com.DefaultCompany.*) warn "package name '$pkg' looks reserved or default; it must be unique and permanent";;
  "") fail "could not read package name";;
  *) pass "package name $pkg";;
esac

# SDK levels (publish-mobile-manifest, updated 2026-10-15)
if [ -n "$minsdk" ] && [ "$minsdk" -ge 29 ] && [ "$minsdk" -le 34 ]; then
  pass "minSdkVersion $minsdk within 29-34 (32 recommended)"
else
  fail "minSdkVersion '$minsdk' outside 29-34"
fi
if [ "$MODE" = "immersive" ]; then tmax=34; else tmax=37; fi
if [ -n "$tsdk" ] && [ "$tsdk" -ge 32 ] && [ "$tsdk" -le "$tmax" ]; then
  pass "targetSdkVersion $tsdk within 32-$tmax"
  [ "$tsdk" -ne 34 ] && warn "apps created since 2026-03-01 must target 34"
else
  fail "targetSdkVersion '$tsdk' outside 32-$tmax (set Target API Level explicitly; not Automatic)"
fi
if [ -n "$csdk" ] && [ -n "$tsdk" ] && [ "$csdk" -lt "$tsdk" ]; then
  fail "compileSdkVersion $csdk < targetSdkVersion $tsdk"
fi

# 64-bit only (VRC.Quest.Packaging.6)
native=$(echo "$BADGING" | sed -n "s/^native-code: //p")
if [ -z "$native" ]; then
  warn "no native-code line (unexpected for IL2CPP Unity builds)"
elif echo "$native" | grep -q "armeabi-v7a\|x86"; then
  fail "native code includes non-ARM64 ABIs: $native"
elif echo "$native" | grep -q "arm64-v8a"; then
  pass "native code arm64-v8a only"
else
  fail "unexpected native-code: $native"
fi

# Debuggable
if echo "$BADGING" | grep -q "^application-debuggable"; then
  fail "android:debuggable=true (turn off Development Build)"
else
  pass "not debuggable"
fi

# Signing (VRC.Quest.Packaging.2)
SIGN=$("$APKSIGNER" verify --verbose --print-certs "$APK" 2>&1)
if echo "$SIGN" | grep -q "Verified using v2 scheme (APK Signature Scheme v2): true"; then
  pass "APK Signature Scheme v2"
else
  fail "not signed with APK Signature Scheme v2"
fi
if echo "$SIGN" | grep -qi "CN=Android Debug"; then
  fail "signed with the Android debug certificate; use the release keystore"
else
  pass "release certificate: $(echo "$SIGN" | sed -n 's/^Signer #1 certificate DN: //p' | head -n 1)"
fi

# Manifest entries
if [ -z "$XMLTREE" ]; then
  warn "could not dump AndroidManifest.xml tree; skipping manifest element checks"
else
  if echo "$XMLTREE" | grep -q '"com.oculus.intent.category.VR"'; then
    pass "launch intent has com.oculus.intent.category.VR"
  else
    [ "$MODE" = "immersive" ] && fail "missing com.oculus.intent.category.VR (required for OpenXR apps)" || warn "no VR intent category (OK for 2D panel apps)"
  fi
  if echo "$XMLTREE" | grep -q '"android.hardware.vr.headtracking"'; then
    pass "uses-feature android.hardware.vr.headtracking present (immersive apps need required=true)"
  else
    [ "$MODE" = "immersive" ] && fail "missing uses-feature android.hardware.vr.headtracking" || pass "no headtracking feature (2D)"
  fi
  if echo "$XMLTREE" | grep -q '"com.oculus.supportedDevices"'; then
    devices=$(echo "$XMLTREE" | grep -A2 '"com.oculus.supportedDevices"' | sed -n 's/.*android:value([^)]*)="\([^"]*\)".*/\1/p' | head -n 1)
    pass "com.oculus.supportedDevices=${devices:-<unparsed>}"
    echo "INFO  Meta VR Glasses delivery is set by Dashboard targeting (Supported Devices > Future devices), not by this list; Meta's pages disagree on a 'vrglasses' value (Core SDK 207 writes 'stanley')"
  else
    fail "missing com.oculus.supportedDevices meta-data (set Target Devices in Meta XR settings)"
  fi
  # Declared input (essentials/app-compatibility, 2026-10-13): none = controllers only,
  # handtracking required=false + HAND_TRACKING = controllers and hands, required=true = hands only
  if echo "$XMLTREE" | grep -q '"oculus.software.handtracking"'; then
    ht=$(echo "$XMLTREE" | grep -A4 '"oculus.software.handtracking"' | grep -m1 ':required(' | sed -n 's/.*)=\(.*\)$/\1/p' | sed 's/ (Raw.*//; s/"//g')
    case "$ht" in
      true|0xffffffff) echo "INFO  declares hands only (handtracking required=true): every flow must work without controllers";;
      *) echo "INFO  declares controllers and hands (handtracking required=${ht:-false}): every declared mode must work alone (Tracking.2)";;
    esac
    echo "$XMLTREE" | grep -q '"com.oculus.permission.HAND_TRACKING"' || warn "oculus.software.handtracking without com.oculus.permission.HAND_TRACKING"
  else
    warn "no hand-tracking declaration: controllers only. Meta VR Glasses ship without controllers and show a 'Switch to Controllers' dialog"
  fi
  if echo "$XMLTREE" | grep -q '"com.meta.store.defaultDeviceTargets"'; then
    dt=$(echo "$XMLTREE" | grep -A2 '"com.meta.store.defaultDeviceTargets"' | sed -n 's/.*android:value([^)]*)="\([^"]*\)".*/\1/p' | head -n 1)
    echo "INFO  com.meta.store.defaultDeviceTargets=${dt:-<unparsed>} presets Supported Devices at upload (overridden by an ovr-platform-util --channel suffix)"
  fi
  il=$(echo "$XMLTREE" | grep "installLocation" | sed -n 's/.*installLocation([^)]*)=\([^ ]*\).*/\1/p' | head -n 1)
  case "$il" in
    0|auto|*\"auto\"*) pass "installLocation auto";;
    "") warn "installLocation not found; Meta requires auto (Player > Install Location: Automatic)";;
    *) fail "installLocation=$il; must be auto";;
  esac
  if echo "$XMLTREE" | grep "excludeFromRecents" | grep -qi "true\|0xffffffff"; then
    pass "excludeFromRecents=true"
  else
    warn "excludeFromRecents=true not found on launch activity (Meta requires it; Meta XR manifest tool sets it)"
  fi
fi

# Size (VRC.Quest.Packaging.5: APK < 1 GB)
bytes=$(wc -c < "$APK" | tr -d ' ')
if [ "$bytes" -lt 1073741824 ]; then pass "APK size $bytes bytes (< 1 GB)"; else fail "APK size $bytes bytes >= 1 GB; split to OBB or asset files"; fi

# Permissions (permissions-prohibited 2025-04-30, permissions-review-required 2025-11-26)
PROHIBITED="ACCEPT_HANDOVER ACCESS_BACKGROUND_LOCATION ACCESS_CHECKIN_PROPERTIES ACCESS_LOCATION_EXTRA_COMMANDS
ACCESS_NOTIFICATION_POLICY ACCOUNT_MANAGER ACTIVITY_RECOGNITION ADD_VOICEMAIL ANSWER_PHONE_CALLS
BIND_ACCESSIBILITY_SERVICE BIND_APPWIDGET BIND_AUTOFILL_SERVICE BIND_CALL_REDIRECTION_SERVICE
BIND_CARRIER_MESSAGING_CLIENT_SERVICE BIND_CARRIER_MESSAGING_SERVICE BIND_CARRIER_SERVICES
BIND_CHOOSER_TARGET_SERVICE BIND_CONDITION_PROVIDER_SERVICE BIND_CONTROLS BIND_DEVICE_ADMIN
BIND_DREAM_SERVICE BIND_INCALL_SERVICE BIND_INPUT_METHOD BIND_MIDI_DEVICE_SERVICE BIND_NFC_SERVICE
BIND_NOTIFICATION_LISTENER_SERVICE BIND_PRINT_SERVICE BIND_QUICK_ACCESS_WALLET_SERVICE
BIND_QUICK_SETTINGS_TILE BIND_REMOTEVIEWS BIND_SCREENING_SERVICE BIND_TELECOM_CONNECTION_SERVICE
BIND_TEXT_SERVICE BIND_TV_INPUT BIND_VISUAL_VOICEMAIL_SERVICE BIND_VOICE_INTERACTION
BIND_VR_LISTENER_SERVICE BIND_WALLPAPER BLUETOOTH_PRIVILEGED BODY_SENSORS
BROADCAST_PACKAGE_REMOVED BROADCAST_SMS BROADCAST_WAP_PUSH CALL_PHONE CALL_PRIVILEGED
CAPTURE_AUDIO_OUTPUT CHANGE_COMPONENT_ENABLED_STATE CHANGE_CONFIGURATION CLEAR_APP_CACHE
CONTROL_LOCATION_UPDATES DELETE_CACHE_FILES DELETE_PACKAGES DIAGNOSTIC DUMP FACTORY_TEST
GET_ACCOUNTS GET_ACCOUNTS_PRIVILEGED INSTALL_LOCATION_PROVIDER INSTALL_PACKAGES
INSTANT_APP_FOREGROUND_SERVICE LOADER_USAGE_STATS LOCATION_HARDWARE MANAGE_DOCUMENTS MANAGE_MEDIA
MANAGE_ONGOING_CALLS MASTER_CLEAR MEDIA_CONTENT_CONTROL MODIFY_PHONE_STATE
MOUNT_FORMAT_FILESYSTEMS MOUNT_UNMOUNT_FILESYSTEMS PACKAGE_USAGE_STATS PROCESS_OUTGOING_CALLS
QUERY_ALL_PACKAGES READ_CALENDAR READ_CALL_LOG READ_CONTACTS READ_INPUT_STATE READ_LOGS
READ_PHONE_NUMBERS READ_PHONE_STATE READ_PRECISE_PHONE_STATE READ_SMS READ_VOICEMAIL REBOOT
RECEIVE_MMS RECEIVE_SMS RECEIVE_WAP_PUSH REQUEST_DELETE_PACKAGES REQUEST_INSTALL_PACKAGES
SEND_RESPOND_VIA_MESSAGE SEND_SMS SET_ALWAYS_FINISH SET_ANIMATION_SCALE SET_DEBUG_APP
SET_PROCESS_LIMIT SET_TIME SET_TIME_ZONE SIGNAL_PERSISTENT_PROCESSES SMS_FINANCIAL_TRANSACTIONS
START_FOREGROUND_SERVICES_FROM_BACKGROUND START_VIEW_PERMISSION_USAGE STATUS_BAR
SYSTEM_ALERT_WINDOW UNINSTALL_SHORTCUT UPDATE_DEVICE_STATS USB_CAMERA
USE_ICC_AUTH_WITH_DEVICE_IDENTIFIER USE_SIP UWB_RANGING WRITE_APN_SETTINGS WRITE_CALENDAR
WRITE_CALL_LOG WRITE_CONTACTS WRITE_GSERVICES WRITE_SECURE_SETTINGS WRITE_SETTINGS WRITE_VOICEMAIL"
REVIEW="ACCESS_MEDIA_LOCATION BLUETOOTH_CONNECT BLUETOOTH_SCAN BLUETOOTH_ADVERTISE CAMERA CHANGE_WIFI_STATE
HEADSET_CAMERA MANAGE_EXTERNAL_STORAGE MODIFY_AUDIO_SETTINGS POST_NOTIFICATIONS READ_EXTERNAL_STORAGE
READ_MEDIA_AUDIO READ_MEDIA_IMAGES READ_MEDIA_VIDEO READ_PHONE_STATE RECEIVE_BOOT_COMPLETED RECORD_AUDIO
AVATAR_CAMERA WAKE_LOCK WRITE_EXTERNAL_STORAGE"

perms=$(echo "$BADGING" | sed -n "s/^uses-permission: name='\([^']*\)'.*/\1/p")
echo "permissions declared:"; echo "$perms" | sed 's/^/        /'
bad=0
for p in $perms; do
  short="${p##*.}"
  for x in $PROHIBITED; do
    if [ "$short" = "$x" ] && { [ "$p" = "android.permission.$x" ] || [ "$p" = "$x" ]; }; then
      fail "prohibited permission $p (remove with tools:node=\"remove\"; upload will fail)"; bad=1
    fi
  done
  for x in $REVIEW; do
    if [ "$short" = "$x" ]; then
      warn "review-required permission $p: needs a justification at submission, remove if unused"
    fi
  done
done
[ "$bad" -eq 0 ] && pass "no prohibited permissions"

echo "----"
if [ "$FAILS" -gt 0 ]; then echo "RESULT: $FAILS FAIL(s)"; exit 1; fi
echo "RESULT: no FAILs (review WARN lines)"; exit 0
