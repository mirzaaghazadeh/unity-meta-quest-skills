---
name: horizon-store-publish-unity
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.1.0"
description: >-
  Ship a Unity Meta Quest game build to the Meta Horizon Store: one-time
  Developer Dashboard setup (org verification, app record, App ID and
  credentials, Developer Distribution Agreement), Unity release-APK settings
  (IL2CPP, ARM64, target API 34, keystore signing, Bundle Version Code bump),
  a headless build with the Unity CLI, local APK verification, and upload with
  ovr-platform-util upload-quest-build (OBB, asset files/DLC, debug symbols) to
  ALPHA/BETA/RC/production, device targeting (Future devices for Meta VR
  Glasses, quest3+ channel suffix), promotion between channels, and CI. Use
  when the user says "upload my Quest build", "push to alpha/beta/RC", "release
  build for the Horizon Store", "bump the version code", "sign the APK",
  "ovr-platform-util", "upload-quest-build", "ship to Meta VR Glasses", "target
  Quest 3 only", or "set up CI for Quest uploads". Complements hz-store-submit
  (submission, review) and unity-quest-android-build (build routes, build
  failures).
---

# Unity build to Meta Horizon Store release channel

> Made by [Navid Mirzaaghazadeh](https://github.com/mirzaaghazadeh) · part of [unity-meta-quest-skills](https://github.com/mirzaaghazadeh/unity-meta-quest-skills) · MIT

This skill covers the path from a Unity project to a build sitting in a Horizon Store
release channel. It stops at **Submit for Review**, which happens only in the Developer
Dashboard. Hand off to `hz-store-submit` for that step, and audit with
`horizon-vrc-checklist` and `horizon-store-listing-assets` first.

Related skills:
- `hz-quest-verify-first` (Meta). Re-verify any Quest claim with `metavr docs search` before acting.
- `unity-cli` (Unity). Installs Editors, activates licenses, runs `unity build`.
- `unity-quest-android-build` (this library). Quest APK build routes, toolchain checks, build troubleshooting.
- `unity-quest-dev-setup` (this library). Editor and Android module setup on a new machine.
- `quest-unity-project-settings`. Covers the non-release Player, XR and URP settings.
- `hz-unity-platform-sdk` (Meta). Entitlement check code.
- `metavr-cli` (Meta). Device install, launch and logs.

## Ground rules

1. Never invent App IDs, org IDs, secrets or tokens. Ask the user for them. Keep them in
   environment variables or a CI secret store, and never commit or echo them.
2. **Keep one keystore forever.** Every update must be signed with the same key. Back it up
   outside the repo. Meta's `hz-store-submit` says a lost key means creating a new app entry.
3. **Version codes only go up.** Each upload needs a `versionCode` higher than any build
   ever uploaded for the app, drafts and alpha builds included.
4. Uploading to `store`/production, passing `--publish` to `metavr store dist upload`, and
   moving a build into production are outward-facing. Confirm with the user every time.
5. Horizon Store builds are **APKs**, optionally with an OBB and asset files. Do not build an AAB.

## Step 0: One-time Dashboard setup (user does this)

| Item | Where | Notes |
|---|---|---|
| Org verification | Dashboard > Org Verification | Required before publishing or updating apps and before DUC recertification. Admin verification uses a government ID and "typically" takes minutes. |
| App record | https://developers.meta.com/horizon/manage/ > create app | Note the **App ID**. |
| App credentials | App > **Development > API** | App ID and **App Credentials** (app secret). Viewing the secret needs an admin. |
| User token (alternative) | **Account > Account settings > Get Oculus platform utility token > Generate token** | Treat it like a password. |
| Developer Distribution Agreement | `https://developers.meta.com/horizon/manage/organizations/<ORG_ID>/legal-documents/` | Typical first-upload failure: "must first agree to our Developer Distribution Agreement". Only an org admin can sign it. Pause and ask the user. |
| Release-channel testers | App > Distribution > Release Channels | ALPHA/BETA/RC are invite-only and start empty. The default cap is 200 users; you can request up to 2,500. |
| Age group self-certification | My Apps > app > Production build > App Product Detail Page > Age Group Self-Certification | Values: Teens and Adults (13+), Mixed Ages, Children (10-12). Same values as `--age-group`. |

Sources: publish-organization-verification, publish-reference-platform-command-line-utility,
publish-release-channels, age-groups (URLs at the end). The DDA URL pattern comes from Meta's
`hz-store-pwa` skill and is unverified on the docs site. If the link fails, look under
Organization > Legal Documents in the Dashboard. The agreement text itself is public:
https://developers.meta.com/vr/policy/developer-distribution-agreement/ (updated 2025-04-09).

## Step 1: Release settings in Unity

Set these in the project, or let `scripts/QuestReleaseBuild.cs` enforce them at build time.

| Setting | Value | Why / source |
|---|---|---|
| Package name | `com.company.game`, unique, never changes | Must be unique in the Meta Quest ecosystem (unity-prepare-for-publish, publish-mobile-manifest) |
| Version (versionName) | Human-readable, for example `1.4.0` | Displayed version |
| **Bundle Version Code** | Integer, strictly increasing | The system uses it to decide which build is newer. Unity's top "Version" field is not the version code (unity-prepare-for-publish) |
| Minimum API Level | **32** recommended (29-34 allowed) | publish-mobile-manifest |
| **Target API Level** | **34, set explicitly** | Apps created since 2026-03-01 must target 34, and immersive apps allow only 32-34. Unity's "Automatic (highest installed)" can resolve above 34, so don't use it |
| Scripting Backend | IL2CPP | unity-prepare-for-publish |
| Target Architectures | ARM64 only (clear ARMv7) | The Store accepts only 64-bit apps (VRC.Quest.Packaging.6). Meta VR Glasses can't run a binary with only 32-bit native libraries at all (app-compatibility) |
| Target Devices (Meta XR settings) | Every Quest you support | Writes `com.oculus.supportedDevices`. Store delivery to Meta VR Glasses is set by **Future devices** in the Dashboard, not by this list (Step 5a) |
| Install Location | Automatic | `installLocation` must be `auto` (publish-mobile-manifest) |
| Development Build | Off | `android:debuggable` must be false or unset |
| Build App Bundle (AAB) | Off | Store takes an APK under 1 GB plus OBB files under 4 GB (VRC.Quest.Packaging.5) |
| Split Application Binary | On only if the APK would exceed 1 GB | Produces `main.obb`. Upload it with `--obb` |
| Keystore | Custom release keystore | VRC.Quest.Packaging.2 requires APK Signature Scheme v2. Debug-signed builds are not release builds |

The Meta XR Core SDK writes the Quest manifest entries: `com.oculus.intent.category.VR`
(required for OpenXR apps), `android.hardware.vr.headtracking` (`required="true"` for
immersive apps), `com.oculus.supportedDevices`, and `excludeFromRecents=true`. Regenerate the
manifest from Meta XR settings rather than hand-editing it (see `hz-unity-meta-core-sdk`),
then check the **merged** manifest in the built APK in Step 4. Uploads are rejected
automatically for prohibited permissions. To strip permissions that packages add, use
`tools:node="remove"` in `Assets/Plugins/Android/AndroidManifest.xml` (permissions-remove).

The merged manifest also tells the system which input the app supports (app-compatibility,
2026-10-13): no hand-tracking entries means controllers only;
`oculus.software.handtracking required="false"` plus `com.oculus.permission.HAND_TRACKING` means
controllers and hands; `required="true"` means hands only. Meta VR Glasses ship without
controllers, and the Store shows hand-capable titles first to people without controllers. Declare
hands only once every required flow works with hands (VRC.Quest.Tracking.2).

## Step 2: Choose the version code

- Pick one scheme and keep it: the CI build number (simplest, always increases) or
  `major*10000 + minor*100 + patch` (readable, but hotfix branches can collide).
- Before you build, read the latest uploaded codes:
  `ovr-platform-util get-release-channel-data --app-id "$META_APP_ID" --token "$META_UPLOAD_TOKEN"`
  (or `metavr store dist channels --app-id <id>`). Set the new code above all of them.
- Pass it to the build script as `QUEST_VERSION_CODE`, or as `--android-version-code` with `unity build`.

## Step 3: Build a signed release APK headlessly

**If `unity-quest-android-build` is installed, use it for this step.** It owns the build mechanics:
the Meta Quest profile route with `--android-*` flags, the `--execute-method` route, building
inside the open Editor (`unity command build` / `build_status`), dev builds, toolchain and module
checks, and build-failure troubleshooting. What follows is the minimal release path. Keep **one** build script per project: either its `templates/QuestBuild.cs` or this
skill's `scripts/QuestReleaseBuild.cs`. Both read the same `QUEST_*` environment variables.
Watch out for one default: per that skill, Unity's Meta Quest build profile defaults to **target
API 32**. Override it to 34.

Copy `scripts/QuestReleaseBuild.cs` into the project under `Assets/Editor/`. It sets the
store-critical options from Step 1, signs with the keystore named by environment variables,
builds the enabled scenes in Build Settings, and exits non-zero on failure. It was compile-checked
against Unity 6000.6.6f1's managed assemblies. It doesn't apply Build Profile overrides, so if your
Quest settings live in a profile, use Option B.

Secrets go in the environment (CI secret store, or a local untracked `.env`):
`QUEST_KEYSTORE_PATH`, `QUEST_KEYSTORE_PASS`, `QUEST_KEY_ALIAS`, `QUEST_KEY_ALIAS_PASS`.
Non-secret inputs: `QUEST_VERSION_CODE`, `QUEST_VERSION_NAME` (optional), and
`QUEST_SPLIT_OBB=1` (optional).

**Option A: Unity CLI with the execute method (recommended).** The flags come from the
`unity-cli` skill, `references/build-run-test.md`.

```bash
unity build /path/to/Project \
  --target Android \
  --execute-method QuestReleaseBuild.Build \
  --output-path ./Builds/Quest/mygame-release.apk \
  --timeout 3600
```

`--output-path` reaches the method as `-buildOutput`. For an `--execute-method` build, the CLI
takes the result from the Editor's exit code. `QuestReleaseBuild.Build` calls
`EditorApplication.Exit(1)` on any failure, so a non-zero exit means a real failure. Per Unity CLI
release notes, Meta Quest builds without a custom method work **only through `--profile`**:
`--target Android` with no `--execute-method` fails for non-desktop targets.

**Option B: Unity 6 Build Profile, no custom code.** Use this when the project keeps its Quest
settings in a Meta Quest build profile. `unity build --list-profiles` shows profile names.

```bash
unity build /path/to/Project --profile "Meta Quest Release" \
  --output-path ./Builds/Quest/mygame-release.apk \
  --android-export-type apk \
  --android-keystore-base64 "$KEYSTORE_B64" --android-keystore-password "$QUEST_KEYSTORE_PASS" \
  --android-key-alias "$QUEST_KEY_ALIAS" --android-key-alias-password "$QUEST_KEY_ALIAS_PASS" \
  --android-version-code "$QUEST_VERSION_CODE" --android-target-sdk-version 34
```

The `unity-cli` skill warns that flag values appear in the process list, so mask them in CI logs.

**Option C: Raw Editor binary (no Unity CLI).**

```bash
"$UNITY_EDITOR" -batchmode -nographics -quit -projectPath /path/to/Project \
  -buildTarget Android -executeMethod QuestReleaseBuild.Build \
  -buildOutput ./Builds/Quest/mygame-release.apk -logFile -
```

## Step 4: Verify the APK before uploading

Run `scripts/verify-quest-apk.sh <apk>`. It uses `aapt2` and `apksigner` from Android
build-tools; set `ANDROID_BUILD_TOOLS` if the script can't find them. It checks:
package/versionCode/versionName, minSdk and targetSdk ranges, compileSdk >= targetSdk,
arm64-v8a-only native code, debuggable flag, v2 signature and a non-debug certificate,
VR category, headtracking, supportedDevices, installLocation, excludeFromRecents, the
declared input (controllers, hands or both), `com.meta.store.defaultDeviceTargets` if set, the
1 GB APK limit, prohibited permissions (FAIL) and review-required permissions (WARN).

Then smoke-test on a headset with the Meta CLI (see `metavr-cli`):

```bash
metavr device list
metavr app install --replace ./Builds/Quest/mygame-release.apk
metavr app launch com.company.game --cold-start --wait-for-idle --measure-launch-time --verify
metavr log --tag Unity --level W
```

`--verify` scans logcat for FATAL EXCEPTION, ANR and native crashes. For the full
pre-submission audit, run `horizon-vrc-checklist`.

## Step 5: Upload to a release channel

Install the uploader with `metavr tools install platform-utils`, or download it from the
Platform Utils CLI page (see references). Upload with `scripts/upload-quest-build.sh`, which
refuses the production channel unless `CONFIRM_STORE_UPLOAD=yes`, or call it directly:

```bash
ovr-platform-util upload-quest-build \
  --app-id "$META_APP_ID" --token "$META_UPLOAD_TOKEN" \
  --apk ./Builds/Quest/mygame-release.apk \
  --channel alpha \
  --age-group TEENS_AND_ADULTS \
  --notes "Build $QUEST_VERSION_CODE: fixes X\nAdds Y" \
  --debug-symbols-dir ./Builds/Quest/symbols --debug-symbols-pattern "*.sym.so" \
  --disable-progress-bar
```

- Required flags: `--app-id`, one of `--app-secret`/`--token`, `--apk`, `--channel`, `--age-group`.
- Channels are case-insensitive. Use `store` for production.
- Device targets ride on the channel: `--channel "alpha:quest3+"`. The wrapper script adds the
  suffix from `QUEST_DEVICE_TARGETS` (see Step 5a).
- Optional: `--obb <file>`, `--assets-dir <dir>`, `--asset-files-config <json>`, `--draft`,
  `--language-packs-dir`, `--inherit-asset-files`, `--exclude-addons`, `--config <json>`.
- A successful upload prints a **Build ID**. Record it, since `set-release-channel-build`
  and symbol uploads use it.
- The doc says an upload with no age group goes into draft status, yet also lists
  `--age-group` as required. Always pass it.

Full flag reference, the asset-file JSON format, DLC upload and gotchas are in
[references/ovr-platform-util.md](references/ovr-platform-util.md).

Alternatives:
- `metavr store dist upload --app-id <id> --channel ALPHA --apk <apk> [--obb <obb>] [--notes ..]`
  uploads a draft unless `--publish` is passed. It has no age-group, device-target, asset-file or
  symbol options (metavr 1.8.0.17.10), so prefer ovr-platform-util for release builds. Meta's
  `hz-store-submit` says to fall back to the Dashboard if it errors.
- Unity Editor: **Window > Meta > Tools > Oculus Platform Tool** uploads an existing APK and does
  not build it.
- Meta Quest Developer Hub, or the Dashboard upload page.

## Step 5a: Device targeting and Meta VR Glasses

Meta VR Glasses (spring 2027) use the same Store, entitlements and add-ons as Quest. Which builds
reach which device is set per build (target-builds-by-device, 2026-09-04; app-compatibility,
2026-10-13):

| Where | How |
|---|---|
| Dashboard | Build > **Supported Devices**: Quest 2, Pro, 3, 3S and **Future devices**. Select Future devices plus every Quest that should get the build. That marks it forward-compatible, and after VR Glasses ship an approved Production build reaches them with no new APK |
| Upload | `QUEST_DEVICE_TARGETS="quest3+" scripts/upload-quest-build.sh app.apk alpha` sends `--channel "alpha:quest3+"`. Specifiers: `quest2only`, `questproonly`, `quest3only`, `quest2+`, `questpro+`, `quest3+`, `questpro-`, joined with `\|`. `quest3+` covers the Quest 3 family, VR Glasses and future devices. CLI values override the manifest |
| Manifest | Optional `<meta-data android:name="com.meta.store.defaultDeviceTargets" android:value="quest3+"/>` presets Supported Devices at upload. Use it only to restrict a build on purpose |

Rules: a build must support at least one device; LIVE must always keep one build with Future
devices; every build in an app submission must be forward-compatible; a build with no targeting
("All Devices") already includes VR Glasses. Removing a device from a live build rolls that
device back to the previous build, so confirm with the user first. To stop selling on a device,
set **Purchase-Blocked Devices** in a new app submission.

Before targeting VR Glasses, check the build: 64-bit, every required flow works with hands (a
controller-required immersive app installs but a **Switch to Controllers** dialog blocks it), key
UI fits a 70° × 66° view (`metavr device fov-sim enable` on a Quest 3/3S/Pro), and the VRC audit
passes (no VRC is specific to VR Glasses). **Conflict, unverified:** the release-manifest page
(2026-10-15) lists `vrglasses` as a `com.oculus.supportedDevices` value, while the compatibility
mode page (2026-09-04) says it is not a supported public value yet. Rely on Future devices.
Details: `docs/meta-reference/store-2026.md` in this repo.

## Step 6: Promote a build between channels

```bash
ovr-platform-util set-release-channel-build --app-id "$META_APP_ID" --token "$META_UPLOAD_TOKEN" \
  --age-group TEENS_AND_ADULTS --source-channel beta --destination-channel rc
```

- Promote the build you tested. Don't rebuild for RC or production.
- **First time in production:** nothing goes public until Submission Info is complete and someone
  clicks **Submit for Review** in the Dashboard. After the app is live, a build copied to production
  updates entitled users. Confirm with the user before you copy anything to production.
- Release channels show up in users' Library and under "My Preview Apps".

## Step 7: Before Submit for Review

1. Run `horizon-vrc-checklist`, then `horizon-store-listing-assets`.
2. Data Use Checkup: required if the app uses Platform SDK features such as User ID, Profile,
   Followers, IAP, Subscriptions or User Age Group. Auto-approval ends at first submission, so
   finish feature testing first (`hz-store-submit` covers the one-way doors).
3. Submit at least 2 weeks before the target launch date. Review is technical first, then content.
4. Hand off to `hz-store-submit` for the Dashboard Submission tab.

## CI

See [references/ci.md](references/ci.md) for a pipeline outline, secret handling, and a
GitHub Actions skeleton. The order is: license activation (per `unity-cli`), version code from
the CI run number, Option A build, `verify-quest-apk.sh`, upload to `alpha` with `--draft`
or straight to a test channel, and archived artifacts (APK, symbols, provenance manifest).
For a CI check of Meta project settings, run
`-executeMethod OVRProjectSetupCLI.GenerateProjectSetupReport` (see `quest-unity-project-settings`).

## Troubleshooting

| Symptom | Likely cause / fix |
|---|---|
| "must first agree to our Developer Distribution Agreement" | An org admin signs it under Legal Documents, then retry (Step 0). |
| Version code rejected as too low | Bump **Bundle Version Code**, not Version. Check `get-release-channel-data`. |
| Upload fails the manifest/SDK test | Manifest is checked right after each upload (VRC.Quest.Packaging.1/4). Run `verify-quest-apk.sh`. Usual causes: targetSdk not 32-34, debuggable, missing VR category. |
| Upload fails for permissions | A prohibited permission is in the merged manifest. Remove it with `tools:node="remove"`. |
| Build runs on device but entitlement fails | Wrong App ID in Meta > Platform > Edit Settings, or a non-entitled test account. See `hz-unity-platform-sdk`. |
| APK over 1 GB | Turn on Split Application Binary (OBB under 4 GB), or move content to asset files/DLC. |
| `metavr store dist` GraphQL / "Invalid parameter" | Don't retry with guessed IDs. Use ovr-platform-util or the Dashboard. |
| Can't remove a device from the live build | LIVE must keep a forward-compatible build and every launched device must stay covered. Hide the app from a device with Purchase-Blocked Devices instead (Step 5a). |

## Sources

Every `developers.meta.com/horizon/...` page now redirects to `/vr/`; the links below are the
final URLs.

- https://developers.meta.com/vr/resources/publish-reference-platform-command-line-utility/ (updated 2026-07-20)
- https://developers.meta.com/vr/resources/publish-mobile-manifest/ (updated 2026-10-15)
- https://developers.meta.com/vr/resources/publish-release-channels-device-targeting/ (updated 2026-09-04)
- https://developers.meta.com/vr/essentials/app-compatibility/ (updated 2026-10-13), /compatibility-mode/ (updated 2026-09-04), /field-of-view/ (updated 2026-09-25)
- https://developers.meta.com/vr/documentation/unity/unity-prepare-for-publish/ (updated 2026-04-27)
- https://developers.meta.com/vr/resources/publish-release-channels/ (updated 2026-03-23)
- https://developers.meta.com/vr/resources/publish-organization-verification/ (updated 2026-08-24)
- https://developers.meta.com/vr/resources/age-groups/ (updated 2026-04-02)
- https://developers.meta.com/vr/resources/vrc-quest-packaging-1/, -packaging-4/, -packaging-5/
- https://developers.meta.com/vr/resources/permissions-prohibited/ , /permissions-remove/
- https://developers.meta.com/vr/documentation/unity/unity-platform-tool/ (updated 2026-04-22)
- https://developers.meta.com/vr/documentation/unity/ps-assets/ (updated 2024-09-06)
- https://developers.meta.com/vr/resources/publish-data-use/ (updated 2026-10-14)
- https://developers.meta.com/vr/resources/publish-submit/ (updated 2026-05-08)
- Meta `metavr-cli` docs (`metavr store dist`, `metavr tools`; `docs/metavr-cli-reference.md`, metavr 1.8.0.17.10) and Unity `unity-cli` skill (`unity build`)

Last verified: 2026-10-17
