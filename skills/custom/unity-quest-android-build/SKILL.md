---
name: unity-quest-android-build
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.1.0"
description: Builds Meta Quest (Horizon OS) Android APKs from a Unity project with the unity CLI, as development or release builds, for Quest and Meta VR Glasses. Offers three routes (a Meta Quest build profile with no code, a BuildPipeline script through --execute-method, or a build inside the open Editor), plus toolchain and module checks, the Quest packaging baseline (APK, ARM64, IL2CPP, API 32/34, ASTC), build-failure troubleshooting, and APK checks with aapt2, apksigner and unzip (64-bit libraries only, hand/eye-tracking declarations, no experimental flag or Meta XR Operator layer in release). Use when asked to build or export a Quest APK, make a dev build to try on the headset, pick a headless or CI build route, check a build for Meta VR Glasses, or fix an Android, Gradle or IL2CPP build failure. For uploading to Horizon Store release channels, use horizon-store-publish-unity.
---

# Unity → Meta Quest APK build

> Made by [Navid Mirzaaghazadeh](https://github.com/mirzaaghazadeh) · part of [unity-meta-quest-skills](https://github.com/mirzaaghazadeh/unity-meta-quest-skills) · MIT

Build an APK that installs on Quest and passes the Horizon Store's packaging checks, using the
`unity` CLI. This skill owns the **Quest-specific** part. For CLI install, flags, `--format json`
and exit codes in general, read the official **`unity-cli`** skill. Its
`references/build-run-test.md` documents `unity build` in full.

Hand-offs (other skills in this library):
- OVRManager, OpenXR features, AndroidManifest permissions, Project Setup Tool → **`hz-unity-meta-core-sdk`**
- Installing on the headset and reading logs → **`metavr-cli`**, **`hz-vr-debug`**
- Meta-verified Player, XR and URP settings for Quest → **`quest-unity-project-settings`**
- Developer Dashboard setup, `ovr-platform-util` upload to ALPHA/BETA/RC/store, moving builds
  between channels → **`horizon-store-publish-unity`**. It ships its own release-build script,
  `QuestReleaseBuild.cs`, which reads the same `QUEST_*` env vars as path B here. Use one script
  per project, not both.
- Review submission and the VRC checklist → **`hz-store-submit`**, **`horizon-vrc-checklist`**
- Editor and Android module setup on a new machine → **`unity-quest-dev-setup`**

## Horizon Store packaging baseline

Check every row before calling a release build done. Re-check against the sources when a build is
rejected: Meta changes these rules.

| Setting | Required value | Source |
|---|---|---|
| Package format | **APK** (`buildAppBundle = false`). Unity documents AAB as "only applicable to builds for Google Play". Meta uploads APK plus optional OBB/asset files | Unity `EditorUserBuildSettings.buildAppBundle`; Meta `ps-assets` |
| ABI | **ARM64 only** (no ARMv7, no x86). Meta VR Glasses can't run a binary whose native libraries are only 32-bit (a Mono build), even though Quest still can | Meta `hz-store-submit`; Unity Meta Quest profile default; Meta app-compatibility (2026-10-13) |
| Scripting backend | **IL2CPP** | Unity Meta Quest profile default (ARM64 needs IL2CPP) |
| `minSdkVersion` | **32** recommended (store floor 29) | Meta manifest requirements page (updated 2026-10-15) |
| `targetSdkVersion` | **34**. Required for apps created in the Developer Dashboard after **2026-03-01**. Older apps may use 32–34 | Meta blog "Apps must target Android 14", same page |
| Texture compression | **ASTC** | Meta `hz-new-project-creation` |
| Graphics API | Vulkan (Unity's Meta Quest profile default) | Unity Meta Quest profile docs |
| Color space | Linear | Meta Unity setup reference |
| Signing | **Release keystore**, the same one for the app's whole life. APK Signature Scheme v2 | Meta `hz-store-submit` |
| `versionCode` | Strictly higher than every uploaded build | Meta `hz-store-submit` |
| Manifest | `installLocation="auto"`, not `debuggable`, `com.oculus.intent.category.VR` on the launcher activity, `com.oculus.supportedDevices` meta-data | Meta manifest page. The Meta XR SDK generates these. Never hand-edit them (see `hz-unity-meta-core-sdk`) |
| Input declaration | Matches the app: no `oculus.software.handtracking` = controllers only; `required="false"` + `com.oculus.permission.HAND_TRACKING` = both; `required="true"` = hands only. Eye tracking: `oculus.software.eye_tracking` + `com.oculus.permission.EYE_TRACKING` only if gaze or ETFR is used | Meta app-compatibility (2026-10-13); Unity support-meta-vr-glasses (2026-09-30) |
| No test-only features | Development Build off; no `com.oculus.experimental.enabled` (Experimental Features Enabled marks a build experimental); no Meta XR Operator layer (`libXrApiLayer_METAX_operator`) | Unity in-headset-fov-simulation (2026-09-24); meta-xr-operator/quest (2026-09-10) |

> **Trap:** Unity's **Meta Quest build profile defaults to target API 32** (Unity 6.3 to 6.6 docs).
> New store apps need **34**. Override it in the profile or Player settings, or pass
> `--android-target-sdk-version 34`.

## Step 0: Check the toolchain (read-only)

```bash
unity --version                                   # missing? see unity-quest-dev-setup
unity projects info <project> --format json       # data has the project's editorVersion
unity editors --installed --format json           # is that version installed? note its "location"
unity editors verify <editorVersion>              # are module files present? It prints the repair command
unity license status --format json                # batch builds need an active license
```

The Editor needs the `android` module **and its children**: Android SDK & NDK Tools
(`android-sdk-ndk-tools`) and OpenJDK (`android-open-jdk`). If any are missing, **ask the user
first** (several GB, and it accepts Android SDK EULAs), then run:

```bash
unity install-modules -e <editorVersion> -m android --cm
```

Modules can only be added to Editors that the Hub or CLI installed.

## Step 1: Check the project settings

**Live Editor open?** Use `unity status --format json` (state `ready`). A batch build can't open a
project that an Editor already has open, so either build inside that Editor (Step 2, path C) or ask
the user to save and close it. `unity close` does **not** save. Read the settings without changing
anything:

```bash
mkdir -p <project>/AgentScripts
cp <this-skill>/templates/QuestPreflight.cs <project>/AgentScripts/
unity command run_script --project-path <project> --file AgentScripts/QuestPreflight.cs \
  --entry QuestPreflight.Run --format json
unity command list_build_profiles --project-path <project> --format json
```

**No Editor:** `unity build <project> --list-profiles --format json` lists the build profiles.
Then read the settings with the build script's check, which runs in Step 2 path B.

Fix problems at the source. Prefer Meta's **Project Setup Tool** (`OVRProjectSetup.FixAllAsync`,
see `hz-unity-meta-core-sdk`). For anything it leaves, either run
`templates/QuestBuild.cs` → `QuestBuild.ApplyQuestSettings` once:

```bash
unity run <project> -- -executeMethod QuestBuild.ApplyQuestSettings
```

or set the values in the Meta Quest build profile's **Player Settings Overrides**. Commit the
`ProjectSettings/` diff after either one.

## Step 2: Build

Pick **one** path. All three produce `Builds/Quest/<App>.apk`. Keep `Builds/` out of git.

### A. Build profile, no code (Unity 6.1+, recommended)

Unity's release notes say Meta Quest builds "only through `--profile`" because it isn't in the
classic `--target` list. Create the profile once in **File > Build Profiles > Add Build Profile >
Meta Quest**, or let Meta's setup script switch to it. It lands under `Assets/Settings/Build Profiles/`.

```bash
unity build <project> --list-profiles --format json          # find the Meta Quest profile's name/path
unity build <project> \
  --profile "Assets/Settings/Build Profiles/<MetaQuestProfile>.asset" \
  --output-path Builds/Quest/<App>.apk \
  --android-export-type apk \
  --android-target-sdk-version 34 \
  --android-version-code <N> \
  --android-keystore-base64 "$QUEST_KEYSTORE_B64" \
  --android-keystore-password "$QUEST_KEYSTORE_PASS" \
  --android-key-alias "$QUEST_KEY_ALIAS" \
  --android-key-alias-password "$QUEST_KEY_ALIAS_PASS" \
  --format json
```

- The CLI writes the keystore to a temp file and deletes it afterwards, including on Ctrl-C.
  The **expanded secrets still appear in argv**, so keep them in env vars and mask them in CI logs.
- Leave out the four signing flags for a local **development** build (debug key). Never upload one.
- Unity's docs don't say whether the `--android-*` flags override values a profile sets.
  **Verify the APK** (Step 3) instead of assuming they do.
- `--create-profile <target>` takes a classic target. A Meta Quest value for it is not
  documented, so run `unity build --help` before trying.

### B. `--execute-method` with the bundled script (Unity 6.0+, full control)

```bash
cp <this-skill>/templates/QuestBuild.cs <project>/Assets/Editor/QuestBuild.cs   # commit it
export QUEST_VERSION_CODE=<N> QUEST_KEYSTORE_PATH=/secure/quest-release.keystore \
       QUEST_KEYSTORE_PASS=... QUEST_KEY_ALIAS=... QUEST_KEY_ALIAS_PASS=...    # from a secret store
# optional: export QUEST_BUILD_PROFILE="Assets/Settings/Build Profiles/<MetaQuestProfile>.asset"
unity build <project> --target Android --execute-method QuestBuild.BuildRelease \
  --output-path Builds/Quest/<App>.apk --format json
```

- `--output-path` reaches the method as `-buildOutput`, which the script reads.
- Use `QuestBuild.BuildDevelopment` for a debug-signed build (the `Development` build option).
- Without a profile, the script **fails the build** on any baseline problem and lists each one.
  With a profile it skips that check, because profile overrides aren't visible to
  `PlayerSettings`. Step 3 verifies the APK instead.
- For an `--execute-method` build, the CLI takes its result from the Editor's exit code. The script
  calls `EditorApplication.Exit(1)` on failure, so exit `6` means a real failure.

### C. Inside the open Editor (no restart, no second license seat)

```bash
unity command build --project-path <project> -- --target Android \
  --outputPath Builds/Quest/<App>.apk --dry_run true                  # validate first
unity command build --project-path <project> -- --target Android \
  --outputPath Builds/Quest/<App>.apk --confirm true                  # queue it
unity command build_status --project-path <project> --format json     # poll until status "completed"
```

- Add `--profileName "<Meta Quest profile name>"` to build from a profile, and `--options Development`
  for a dev build. The `--` fence keeps those parameters away from the CLI's own flags. Parameter
  names come from the project's `com.unity.pipeline` version, so check them first with
  `unity command --query build --format json`.
- The build uses the Editor's **current** signing settings. Unity doesn't save keystore passwords,
  so a release-signed build from a fresh Editor session needs them entered in Player Settings
  first. Use path A or B for release builds.
- `build_status` returns `result` (`Succeeded`/`Failed`), `errors[]`, `outputPath` and `files[]`.

### Build failed?

Read `data.logFile` from the JSON envelope. The default is
`<project>/Logs/build-<target>-<timestamp>.log`. **Grep the log, don't dump it**:

```bash
grep -nE "error CS[0-9]{4}|Build Failed|BuildFailedException|CommandInvokationFailure|FAILURE:|What went wrong|Unable to|Exception:" <log> | head -60
```

Exit codes: `2` bad flags (fix the command), `3` sign-in, `4` precondition such as no license,
`6` the build failed (read the log), `7` service unreachable (retry), `143` killed or timed out.
Common causes and fixes: [references/troubleshooting.md](references/troubleshooting.md).

## Step 3: Verify the APK

The build tools ship inside the Editor's Android module. On macOS the layout is
`/Applications/Unity/Hub/Editor/<ver>/PlaybackEngines/AndroidPlayer/SDK/build-tools/<n>/{aapt2,apksigner}`
and `.../SDK/platform-tools/adb`. That was checked on a 6000.6.6f1 install, where `<n>` was
`36.0.0`. On Windows and Linux the Android module sits under
`<Editor dir>/Editor/Data/PlaybackEngines/AndroidPlayer/SDK`, which was not checked. The
build-tools version changes, so search for the tools:

```bash
SDK="<Editor install dir>/PlaybackEngines/AndroidPlayer/SDK"      # adjust per OS
AAPT2=$(find "$SDK" -name aapt2 -type f | sort | tail -1)
APKSIGNER=$(find "$SDK" -name apksigner -type f | sort | tail -1)
# apksigner is a shell wrapper that runs `java` from PATH (it ignores JAVA_HOME).
# With no working system JDK, use the Editor's own:
java -version >/dev/null 2>&1 || export PATH="$SDK/../OpenJDK/bin:$PATH"
APK=Builds/Quest/<App>.apk

"$AAPT2" dump badging "$APK" | grep -E "^package:|sdkVersion|targetSdkVersion|native-code"
#   expect: versionCode='<N>'  sdkVersion:'32'  targetSdkVersion:'34'  native-code: 'arm64-v8a' (only)
"$AAPT2" dump xmltree --file AndroidManifest.xml "$APK" \
  | grep -E "installLocation|debuggable|com.oculus.intent.category.VR|com.oculus.supportedDevices"
"$APKSIGNER" verify --verbose --print-certs "$APK" | grep -E "Verified using v2|Signer #1 certificate DN"
#   expect: "Verified using v2 scheme (APK Signature Scheme v2): true"
#   and a certificate DN that is NOT "CN=Android Debug" for a release build
ls -l "$APK"                                                       # size sanity check

# Input declarations and test-only features (matters most for Meta VR Glasses)
"$AAPT2" dump xmltree --file AndroidManifest.xml "$APK" \
  | grep -E -A2 "handtracking|eye_tracking|HAND_TRACKING|EYE_TRACKING|com.oculus.experimental.enabled"
#   expect: the hand/eye entries the app's input promise needs; no com.oculus.experimental.enabled
unzip -l "$APK" | awk '$4 ~ /^lib\// {split($4, p, "/"); print p[2]}' | sort -u   # expect only arm64-v8a
unzip -l "$APK" | grep -i "METAX_operator" && echo "FAIL: Meta XR Operator layer in a release APK"
```

Report each check as pass or fail with the actual value. A failure means the build isn't releasable.
Go back to Step 1, not straight to upload.

**`com.oculus.supportedDevices` and Meta VR Glasses.** Report the value as found; don't edit it.
With the Unity OpenXR plugin, the APK lists Unity's names (`quest`, `quest2`, `cambria`, `eureka`,
`quest3s`; there is no VR Glasses entry in OpenXR 1.18.0 or 1.19.0-pre.2). If Meta's own manifest
patch writes the list and its VR Glasses target is on, Core 207.0.1 adds the internal key
`stanley` (package source). Meta's pages disagree on the public id (`vrglasses` on the manifest
page, "not a supported public value" on the compatibility-mode page), and delivery to VR Glasses is
set by ticking **Future devices** in the Dashboard (`horizon-store-publish-unity`). Details:
`quest-unity-project-settings` §1 and `meta-vr-glasses-unity`.

## Step 4: Run it on the headset, then hand off

```bash
metavr app install Builds/Quest/<App>.apk        # Meta's CLI (see metavr-cli). Or: adb install -r <apk>
metavr app launch <applicationId>
metavr log --tag Unity                           # first-launch errors
```

`adb` comes with the Unity SDK module under `.../SDK/platform-tools/adb`. For store upload
(`ovr-platform-util upload-quest-build`), OBB and asset files, and release channels, use
**`horizon-store-publish-unity`**. For review, use **`hz-store-submit`**.

## CI

Run `unity ci init --target Android --dry-run` to get Unity's generated GitHub Actions workflow
(CLI install, caches, license, `doctor --ci`, tests, build, artifacts, seat return). Then change its
build step to path A or B. The full recipe, secrets layout and licensing caveats are in
[references/ci.md](references/ci.md).

## Rules

- **Keystores and passwords never go in the repo, the prompt, a command literal or a log.** Read them
  from env vars or a secret store. Back up the release keystore: losing it means a new store listing.
  See [references/signing-and-versioning.md](references/signing-and-versioning.md).
- **Ask before** installing modules (multi-GB, accepts EULAs), closing an Editor (unsaved work), or
  changing `ProjectSettings/`. Show the diff afterwards.
- **Never hand-edit `AndroidManifest.xml`** for features managed by OVRProjectConfig. Regenerate it
  through the Meta SDK (see `hz-unity-meta-core-sdk`).
- **Increase `versionCode` on every upload.** In CI, use the run number or a value derived from a git tag.
- **Don't trust a successful exit alone.** Run Step 3 on every release candidate.
- **Don't invent flags.** If a flag above is rejected, run `unity build --help`. The CLI is in beta
  and changes weekly.

## Sources

- Unity CLI reference (`build`, exit codes, env vars): https://docs.unity.com/en-us/unity-cli/unity-cli-reference.md
- Unity CLI release notes (Meta Quest builds via `--profile`, Android signing flags): https://docs.unity.com/en-us/unity-cli/release-notes.md
- Official `unity-cli` skill, `references/build-run-test.md`: https://github.com/Unity-Technologies/skills/tree/main/skills/unity-cli
- Pipeline `build` / `build_status` / `list_build_profiles`: https://docs.unity3d.com/Packages/com.unity.pipeline@0.8/manual/commands/build-and-compilation.html
- Meta Quest build profile (6.3 LTS): https://docs.unity3d.com/6000.3/Documentation/Manual/xr-meta-quest-build-profile.html
- Meta Quest build profile settings (defaults): https://docs.unity3d.com/Manual/xr-meta-quest-build-profile-settings.html
- `BuildPipeline.BuildPlayer`: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html
- `EditorUserBuildSettings.buildAppBundle`: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/EditorUserBuildSettings-buildAppBundle.html
- `PlayerSettings.Android`: https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/playersettings/android.md
- Unity Editor command-line arguments: https://docs.unity.com/en-us/engine/6000.3/manual/unity-editor/command-line-arguments/editor.md
- Meta manifest and SDK version requirements: https://developers.meta.com/vr/resources/publish-mobile-manifest/
- Meta "Apps must target Android 14" (2025-11-24, updated 2026-02-06): https://developers.meta.com/horizon/blog/meta-quest-apps-android-14-march-1
- Meta OBB and asset files: https://developers.meta.com/horizon/documentation/unity/ps-assets/
- Meta skills `hz-store-submit`, `hz-new-project-creation`, `hz-api-upgrade` (meta-quest/agentic-tools @ 3a8553d)
- Meta app compatibility for VR Glasses (64-bit, input declarations, Future devices): https://developers.meta.com/vr/essentials/app-compatibility/ (2026-10-13)
- Support Meta VR Glasses (Unity; Hand/Eye Tracking Support → manifest): https://developers.meta.com/vr/documentation/unity/unity-support-meta-vr-glasses/ (2026-09-30)
- Compatibility mode (supportedDevices ids): https://developers.meta.com/vr/essentials/compatibility-mode/ (2026-09-04)
- Unity FoV simulation (experimental flag): https://developers.meta.com/vr/documentation/unity/in-headset-fov-simulation/ (2026-09-24)
- Meta XR Operator on Quest (Development builds): https://developers.meta.com/vr/documentation/unity/meta-xr-operator/quest/ (2026-09-10)
- Package source read 2026-10-17: Core 207.0.1 `Editor/OVRManifestPreprocessor.cs`, `Editor/MetaXROperator/MetaXROperatorBuildProcessor.cs`; Unity OpenXR 1.18.0 / 1.19.0-pre.2 `Runtime/MetaQuest/MetaQuestFeature.cs`

Last verified: 2026-10-17 (VR Glasses rows and APK checks; the `unzip` lines were run on a test
archive, not a real APK); 2026-10-16 (the rest). `templates/QuestBuild.cs` and `templates/QuestPreflight.cs` compile
with 0 errors and 0 warnings against Unity 6000.6.6f1's managed DLLs, using Unity's bundled
Roslyn; neither has been run in a project. The macOS SDK tool paths were checked on a 6000.6.6f1
install. Not verified against a live CLI: whether `--android-*` flags apply on top of a `--profile`
build, and `--create-profile` with a Meta Quest value.
