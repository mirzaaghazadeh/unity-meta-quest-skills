---
name: quest-unity-project-settings
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.2.0"
description: >-
  Recommended Unity settings for Meta Quest performance and Horizon Store
  compatibility, each checked against Meta's docs: Unity version floor for Meta
  XR SDK v207, Unity OpenXR plugin (Oculus XR plugin deprecated), Meta XR
  feature group, Project Setup Tool, IL2CPP/ARM64/API levels, Vulkan, ASTC, MSAA
  4x in URP, Quality settings, OpenXR Meta Quest options (multiview, symmetric
  projection, optimize buffer discards, late latching), target devices
  including Meta VR Glasses, Hand/Eye Tracking Support, experimental flags,
  fixed and eye-tracked foveated rendering, Application SpaceWarp, refresh
  rates, CPU/GPU performance levels, per-device budgets for Quest 2/3/3S and VR
  Glasses. Includes an Editor audit/apply script for batch mode and CI. Use
  when the user asks "what settings for Quest", "optimize my Unity project for
  Quest", "Vulkan or OpenGL", "OpenXR or Oculus plugin", "which target
  devices", "enable foveated rendering", "enable SpaceWarp", "set 90/120 Hz",
  "target API level", or "audit project settings".
---

# Unity project settings for Meta Quest (verified against Meta docs)

> Made by [Navid Mirzaaghazadeh](https://github.com/mirzaaghazadeh) · part of [unity-meta-quest-skills](https://github.com/mirzaaghazadeh/unity-meta-quest-skills) · MIT

Use this skill together with Meta's `hz-unity-meta-core-sdk`, which covers SDK install, the rig
and the Project Setup Tool API, and Meta's `hz-unity-code-review`, which reviews code and content.

**Corrections to Meta's skills:**
- `hz-unity-code-review` says to enable multiview under "XR Plug-in Management > Oculus". The
  Oculus XR plugin is deprecated (unity-xr-plugin, 2026-10-07). With the recommended Unity OpenXR
  plugin, the render mode lives under XR Plug-in Management > OpenXR (Android).
- Meta's skill project-setup-workflow says "Unity 2022.3.15f1 or later". Meta XR SDK v207 (2026-09-22)
  and Meta's requirements page set the floor at **Unity 6000.0.66f2**.

Each row below has a level: **Store** (needed to upload or pass review), **Meta-rec** (recommended
on a Meta page), or **Older/other** (an old Meta tech note or Meta's own skill, not on current docs).

## 1. Editor, packages, XR plugin

| Setting | Value | Level | Source |
|---|---|---|---|
| Unity Editor | **6000.0.66f2 or later**; 6.1+ recommended for installing the XR plugins | Store (via VRC.Quest.Packaging.4) | unity-development-requirements (2026-08-11); unity-xr-plugin |
| Unity modules | Android Build Support + OpenJDK + Android SDK & NDK Tools | Store | unity-development-requirements |
| Meta SDK | Meta XR All-in-One SDK (`com.meta.xr.sdk.all`), latest **207.0** (2026-09-22), or individual packages such as `com.meta.xr.sdk.core`. Core, Interaction, Interaction OVR, MRUK and Haptics are **207.0.1** on Meta's npm registry (2026-10-12); Core 207.0.1 pulls in `com.unity.xr.hands` 1.7.2. Install from the Unity Asset Store (recommended) or Meta NPM tarballs | Meta-rec | downloads/package/meta-xr-sdk-all-in-one-upm; unity-package-manager; npm.developer.oculus.com |
| XR plugin | **Unity OpenXR Plugin** (recommended **1.15.1**; needs Unity 6+ and Meta XR SDK v74+). The Oculus XR Plugin is deprecated and scheduled for removal. Meta XR Operator needs **1.17.0+** (API Layers feature) | Meta-rec | unity-xr-plugin (2026-10-07); meta-xr-operator (2026-09-09) |
| Meta VR Glasses | Core **and** Interaction SDK **207+** for gaze interactions, FoV simulation and Device Readiness Check. Don't add `com.meta.xr.simulator` (deprecated; breaks with Core 207) | Meta-rec | unity-support-meta-vr-glasses (2026-09-30); device-readiness (2026-09-24) |
| OpenXR features | XR Plug-in Management > OpenXR > enable **Meta XR** for **Standalone and Android** | Meta-rec | unity-project-setup (2026-10-15) |
| Project Setup Tool | Window > Meta > Tools > Project Setup Tool > **Fix All** and **Apply All** on Standalone and Android tabs until no Required issues remain | Meta-rec | unity-project-setup; unity-upst-overview (2026-08-25) |
| Depth API occlusion | Also needs `com.unity.xr.meta-openxr@2.1.0` and Unity 6+ | Meta-rec (if used) | unity-xr-plugin |

To automate setup, use `OVRProjectSetup.FixAllAsync(BuildTargetGroup.Android)` and `(Standalone)`
via `unity-cli` (see `hz-unity-meta-core-sdk/references/project-setup-tool.md`). For a CI or batch
report, run
`-executeMethod OVRProjectSetupCLI.GenerateProjectSetupReport [-reportFile <path>] [-buildTarget Android]`.
It writes JSON with each task's group, level and `isDone` (unity-upst-overview).

**Meta Quest options from code** (Unity OpenXR Plugin 1.18.0; checked 2026-10-17). The fields are
internal but serialized, so set them as the Inspector does:

```csharp
var quest = new SerializedObject(OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android).GetFeature<MetaQuestFeature>());
quest.FindProperty("m_symmetricProjection").boolValue = true;
quest.FindProperty("m_optimizeBufferDiscards").boolValue = true;
quest.FindProperty("lateLatchingMode").boolValue = true;
quest.FindProperty("lateLatchingDebug").boolValue = false;
quest.FindProperty("m_multiviewRenderRegionsOptimizationMode").intValue = (int)OpenXRSettings.MultiviewRenderRegionsOptimizationMode.AllPasses;
var devices = quest.FindProperty("targetDevices");   // manifest names: quest, quest2, cambria (Pro), eureka (3), quest3s
for (var i = 0; i < devices.arraySize; i++)
{
    var d = devices.GetArrayElementAtIndex(i);
    d.FindPropertyRelative("enabled").boolValue = d.FindPropertyRelative("manifestName").stringValue is "eureka" or "quest3s";
}
quest.ApplyModifiedPropertiesWithoutUndo();
```

**Target devices trap (observed):**
- With the OpenXR plugin, this `targetDevices` list on Unity's Meta Quest feature is what
  validation checks, and it defaults to *every* Quest, including the original one.
- Setting Meta's `OVRProjectConfig.targetDeviceTypes` alone left it unchanged.
- With the original Quest still enabled, turning on Symmetric Projection fails OpenXR validation
  ("Symmetric Projection is only available on Quest 2 or higher").
- An APK built with the defaults listed `quest|quest2|cambria|eureka|quest3s` in
  `com.oculus.supportedDevices`.

**Meta VR Glasses and target devices (v207, checked 2026-10-17):**
- Unity OpenXR 1.18.0 and 1.19.0-pre.2 have **no VR Glasses entry** in this list (only `quest`,
  `quest2`, `cambria`, `eureka`, `quest3s`; package source).
- Core 207.0.1 adds `OVRProjectConfig.DeviceType.VRGlasses` (on by default in a newly created
  config). When it's selected, Meta's manifest patch writes the internal key `stanley`, not the
  documented `vrglasses` (package source).
- Meta's pages disagree: the manifest page (2026-10-15) lists `vrglasses`; the compatibility-mode
  pages (2026-09-04) say it isn't a supported public value yet and VR Glasses run Store apps in
  compatibility mode.
- **Do this:** keep the list above to the Quest devices you test, don't hand-add a VR Glasses id,
  and deliver to VR Glasses by ticking **Future devices** in the build's Supported Devices on the
  Dashboard (`horizon-store-publish-unity`). Read the built APK's value with `aapt2`
  (`unity-quest-android-build`). Port details: `meta-vr-glasses-unity`.

Without a live Editor, `FixAllAsync` needs a batch launch **without** `-quit` that waits for the
task, and OpenXR validation of newly enabled features needs a second launch. A working pattern
is in `unity-quest-dev-setup/references/batch-mode-traps.md`. Run that way on Unity 6000.6.6f1 with
Core SDK 207.0.1 (2026-10-16), a URP-blank project with OpenXR + Meta XR feature group reached:
all 36 Required and 10 Optional tasks done, 31 of 34 Recommended (left: Data Use Checkup and App
ID, which only matter with the Platform SDK, and the simulator before it was installed); OpenXR
validation for Android 0 errors, 0 warnings.

## 2. Player settings (Android / Meta Quest build profile)

| Setting | Value | Level | Source |
|---|---|---|---|
| Scripting backend | IL2CPP | Store | unity-prepare-for-publish (2026-04-27) |
| Target architectures | ARM64 only | Store | VRC.Quest.Packaging.6 |
| Minimum API level | 32 recommended (29-34 allowed) | Store | publish-mobile-manifest (2026-10-15) |
| Target API level | **34 explicitly**. "Automatic (highest installed)", as Meta's Unity page suggests, can exceed the immersive cap of 34 | Store | publish-mobile-manifest |
| Install location | Automatic | Store | publish-mobile-manifest |
| Package name | Unique, permanent | Store | unity-prepare-for-publish |
| Graphics API | **Vulkan** only (untick Auto Graphics API). OpenGL ES is legacy and new Quest features ship only on Vulkan. AppSW, ETFR, Depth API and Subsampled Layout need Vulkan | Meta-rec | os-vulkan-opengl; unity-asw; unity-fixed-foveated-rendering |
| Texture compression | **ASTC** as the global setting (UI textures may need uncompressed) | Meta-rec | unity-mobile-performance-intro; tech-note-unity-settings-for-mobile-vr |
| Color space | Linear (a stylistic choice per the old tech note; current pages are silent) | Older/other | tech-note-unity-settings-for-mobile-vr |
| Multithreaded rendering | Keep on for release; disabling it can help while debugging | Meta-rec (debug note) | unity-perf (2024-10-30) |
| Development Build | Off for release builds | Store | unity-conf-settings (2025-02-18) |
| Target devices | Only headsets you have tested. This sets `com.oculus.supportedDevices`. VR Glasses: see the box in §1 | Meta-rec | unity-openxr-settings-quest; unity-conf-settings |
| Hand Tracking Support (OVRManager > Quest Features > General) | **Controllers And Hands** once every required flow works with hands (writes `oculus.software.handtracking` required=false + `HAND_TRACKING`); Hands Only → required=true. Controllers Only on VR Glasses means the Switch to Controllers dialog for users without controllers | Meta-rec (VR Glasses reach) | unity-support-meta-vr-glasses (2026-09-30); app-compatibility (2026-10-13) |
| Eye Tracking Support (same panel) | **Supported** if you use gaze (keeps ray fallback); Required only if the app can't run without it. Adds `com.oculus.permission.EYE_TRACKING` | Meta-rec (if gaze/ETFR) | unity-support-meta-vr-glasses |
| Custom `Assets/Plugins/Android/AndroidManifest.xml` | Its hand-tracking entry is kept, not updated. Window > Meta > Tools > Android Manifest Tool > Update AndroidManifest.xml for Store Compatibility | Store (input declaration) | unity-support-meta-vr-glasses |
| Experimental Features Enabled (OVRManager > Quest Features > Experimental) | **Off** for release builds. It marks the build experimental (`com.oculus.experimental.enabled`); the Unity FoV simulation toggle and on-device Meta XR Operator need it on | Store | in-headset-fov-simulation (2026-09-24); meta-xr-operator/quest (2026-09-10) |
| System splash screen | A custom splash for published apps (OVRManager > Quest Features > General) | Meta-rec | unity-openxr-settings-quest; unity-splash-screen |
| Link Time Optimization | Enable in OVRManager build settings (release only) | Meta-rec | unity-best-practices-intro |

## 3. Rendering, quality, OpenXR Meta Quest options

| Setting | Value | Level | Source |
|---|---|---|---|
| Render pipeline | URP. Meta's setup workflow uses the Universal 3D template. Every quality level gets either no RP asset or the same URP asset | Meta-rec | unity-project-setup; Meta skill project-setup-workflow |
| Rendering path | Forward or Forward+ | Meta-rec | unity-best-practices-intro (2024-09-15) |
| MSAA | **4x**. With URP, set it **manually** on the URP asset | Meta-rec | unity-conf-settings |
| Quality | Global Mipmap Limit = Full Resolution; Anisotropic Textures = Per Texture; Billboards Face Camera Position on; Realtime Reflection Probes only if needed | Meta-rec | unity-conf-settings |
| HDR (URP asset) | Off | Older/other | Meta `hz-unity-code-review` (not on current docs) |
| Render mode | Multiview (OpenXR "Single Pass Instanced" on Android) | Meta-rec (prerequisite for Symmetric Projection, AppSW, late latching) | unity-openxr-settings-quest |
| Symmetric Projection (Vulkan) | On for most apps | Meta-rec | unity-openxr-settings-quest (2026-10-07) |
| Optimize Buffer Discards (Vulkan) | On for virtually every app | Meta-rec | unity-openxr-settings-quest; unity-asw |
| Multiview Render Regions (Vulkan, Unity 6.1+) | All Passes; Final Pass if post-processing shows edge artifacts | Meta-rec | unity-openxr-settings-quest |
| Late Latching (Vulkan) | On for most apps (consider off if exact physics-to-hand alignment matters); Debug Mode never in release | Meta-rec | unity-openxr-settings-quest |
| Force Remove Internet Permission | Only for fully offline games | Meta-rec | unity-openxr-settings-quest |
| Post-processing and effects | Avoid SSAO, motion blur, global fog, parallax mapping; no more than 2 render passes; no realtime GI; avoid multipass shaders | Meta-rec | unity-best-practices-intro |
| Textures | No more than 2k × 2k on mobile after LOD bias | Meta-rec | unity-best-practices-intro |
| Physics | Sleep Threshold ≥ 0.005; Default Contact Offset ≥ 0.01; Solver Iterations ≤ 6 | Meta-rec | unity-best-practices-intro |
| Startup | No large textures or many prefabs in the first scene (VRC.Quest.Performance.3: 4 s) | Meta-rec | unity-best-practices-intro |

## 4. Frame-rate features

**Fixed foveated rendering** (unity-fixed-foveated-rendering, 2026-10-07)
- `OVRManager.foveatedRenderingLevel = OVRManager.FoveatedRenderingLevel.High;` Levels are Off,
  Low, Medium, High and HighTop. HighTop is treated as High under OpenXR.
- `OVRManager.useDynamicFoveatedRendering = true;` lets the runtime scale foveation with GPU load,
  up to the level you set.
- Meta XR Subsampled Layout (OpenXR > Android > Meta XR feature group) needs Vulkan and Unity OpenXR
  ≥ 1.9.0.
- **Eye-tracked foveated rendering** (Quest Pro and Meta VR Glasses only; unity-eye-tracked-foveated-rendering
  2026-10-07): Vulkan + multiview, Foveated Rendering Method = Eye Tracked Foveated Rendering on
  the Android OpenXR tab, then `if (OVRManager.eyeTrackedFoveatedRenderingSupported)
  OVRManager.eyeTrackedFoveatedRenderingEnabled = true;`. It stays off when the user declines the
  eye-tracking permission. On VR Glasses FFR saves less than on Quest 3 (larger GPU tiles), so ETFR
  matters more there (optimize-performance 2026-09-19).

**Application SpaceWarp** (unity-asw, 2026-05-13)
- Requirements: Unity 2022.3.15f1+, 6000.0.9f1+, or **6000.4.0f1+ (recommended)**; OpenXR
  OVRPlugin v34+; **Vulkan only**; a URP with SpaceWarp support (Meta's URP fork is the reference).
- Enable **Meta XR Space Warp** (OpenXR > All Features). Toggle it at runtime with
  `OVRManager.SetSpaceWarp(...)`, and call it again whenever the main camera changes.
  `cameraData.xr.motionVectorRenderTargetValid` confirms it is active.
- Every shader needs a `LightMode="MotionVectors"` pass (built-in URP Lit gets one from the fork).
  Custom and alpha-clip shaders need their own, using `OculusMotionVectorCore.hlsl`. Transparents
  are problematic. Unmodified materials show artifacts.
- AppSW lets VRC.Quest.Performance.1 accept 30 fps at 60 Hz or 36 fps at 72 Hz, as long as motion
  vectors keep artifacts low.

**Refresh rate** (unity-set-disp-freq, 2026-10-01)
- Default is 72 Hz. Read `OVRManager.display.displayFrequenciesAvailable` (or
  `OVRPlugin.systemDisplayFrequenciesAvailable`), then set `OVRPlugin.systemDisplayFrequency = 90.0f;`
  only if the game holds it. `OVRManager.DisplayRefreshRateChanged` signals runtime changes;
  thermal throttling may drop to 72 Hz first.
- Per the page's table: Quest 2, Quest 3 and Quest 3S list 72/80/90/96/100/120 Hz (96/100 on
  Quest 3 need OpenXR 1.0.63+; Meta flags Quest 2/3S 96/100 as still to verify); Quest Pro
  72/80/90. 60 Hz is for media apps only, or the app fails review. Quest 3 alone goes above 120 Hz;
  never require it.
- Meta VR Glasses aren't in that table: the spec page says 120 Hz and the simulator profile offers
  90/120 (default 90). In compatibility mode the list shrinks to rates both models support, so
  always pick from the reported list.

**Dynamic resolution and CPU/GPU levels** (os-cpu-gpu-levels 2026-09-02; po-quest-boost 2026-09-02)
- Keep render scale ≥ 85% for most of the game (VRC.Quest.Performance.4). Meta's
  best-practices page credits dynamic resolution with access to GPU level 5.
- Hint levels with `OVRManager.suggestedCpuPerfLevel` / `suggestedGpuPerfLevel`
  (`ProcessorPerformanceLevel`: PowerSavings, SustainedLow, SustainedHigh (default), Boost).
  `OVRManager.cpuLevel` / `gpuLevel` are `[Obsolete]` in Core 207.0.1 (package source).
- Meta VR Glasses: design to CPU 4 / GPU 4; no GPU level 5, dual-core mode or CPU/GPU trading;
  CPU Boost (4 → 8) draws from a refilling budget; turn on dynamic resolution
  (`OVRManager.enableDynamicResolution`, min/max scale on the rig) because no single render scale
  matches the panel (render-scale 2026-09-01; optimize-performance 2026-09-19).

## 5. Budgets per device

See [references/performance-targets.md](references/performance-targets.md). The headline numbers:
interactive apps target **72 FPS minimum** (Meta perf docs). The VRC floor is 60 fps rendering.
Per-frame budgets are 13.8 ms at 72 Hz, about 11.1 ms at 90 Hz and about 8.3 ms at 120 Hz. Draw
calls for busy scenes: Quest 2/Pro 80-200, Quest 3/3S 200-300. Meta VR Glasses: no draw-call or
triangle numbers published; Meta says a Quest 3 app that holds its rate at nominal levels should
match it there, and the PSS memory limit is 5.75 GiB as on Quest 3/3S.

## 6. Audit and apply

Copy `scripts/QuestProjectSettingsAudit.cs` into `Assets/Editor/`. It compiles without XR, OpenXR or
URP packages because it reads them by reflection. It was compile-checked against Unity 6000.6.6f1.

```bash
# Report only. Exit 1 if a Store-level (Required) check fails. JSON written to Logs/
unity run /path/to/Project -- -executeMethod QuestProjectSettingsAudit.Audit -auditReport Logs/quest-settings-audit.json
# Apply store-critical + Meta-recommended Player/Quality values (not color space, not XR loaders)
unity run /path/to/Project -- -executeMethod QuestProjectSettingsAudit.ApplyRecommended
```

In the Editor: **Tools > Quest > Audit Project Settings**. After applying, run the Project Setup
Tool Fix All. Then make a release build (`horizon-store-publish-unity`), run `verify-quest-apk.sh`,
and profile on device: OVR Metrics Tool (`metavr tools install ovrmetric`), `hz-perfetto-debug`,
and Meta Quest Runtime Optimizer (Asset Store, experimental, Horizon OS v78+).

Manual checks the script can't read (shown in the JSON `manualChecks`): Meta XR feature group,
Meta Quest feature options (Symmetric Projection, Optimize Buffer Discards, Late Latching), Target
Devices, System Splash Screen. For Hand/Eye Tracking Support, Experimental Features, FoV simulation
and the other VR Glasses items, run `meta-vr-glasses-unity/scripts/vrglasses_readiness.py`
(read-only, no Unity needed).

## Sources

- https://developers.meta.com/horizon/documentation/unity/unity-development-requirements/ (2026-08-11)
- https://developers.meta.com/horizon/documentation/unity/unity-xr-plugin/ (2026-10-07)
- https://developers.meta.com/horizon/documentation/unity/unity-project-setup/ (2026-10-15)
- https://developers.meta.com/horizon/documentation/unity/unity-upst-overview/ (2026-08-25)
- https://developers.meta.com/horizon/downloads/package/meta-xr-sdk-all-in-one-upm/ (207.0, 2026-09-22)
- https://developers.meta.com/horizon/documentation/unity/unity-package-manager/ (2025-06-01)
- https://developers.meta.com/horizon/documentation/unity/unity-prepare-for-publish/ (2026-04-27)
- https://developers.meta.com/horizon/resources/publish-mobile-manifest/ (2026-10-15)
- https://developers.meta.com/vr/documentation/unity/unity-project-configuration/ (was `unity-conf-settings`; 2025-02-18)
- https://developers.meta.com/horizon/documentation/unity/unity-openxr-settings-quest/ (2026-10-07)
- https://developers.meta.com/horizon/documentation/unity/os-vulkan-opengl/ (undated)
- https://developers.meta.com/horizon/documentation/unity/unity-fixed-foveated-rendering/ (2026-10-07)
- https://developers.meta.com/horizon/documentation/unity/unity-asw/ (2026-05-13)
- https://developers.meta.com/horizon/documentation/unity/unity-best-practices-intro/ (2024-09-15)
- https://developers.meta.com/horizon/documentation/unity/unity-mobile-performance-intro/ (undated)
- https://developers.meta.com/vr/documentation/unity/unity-set-disp-freq/ (2026-10-01)
- https://developers.meta.com/horizon/documentation/unity/unity-quest-runtime-optimizer/ (2026-07-31)
- https://developers.meta.com/horizon/blog/tech-note-unity-settings-for-mobile-vr/ (old)
- https://developers.meta.com/vr/documentation/unity/unity-support-meta-vr-glasses/ (2026-09-30)
- https://developers.meta.com/vr/documentation/unity/optimize-performance/ (2026-09-19)
- https://developers.meta.com/vr/documentation/unity/os-cpu-gpu-levels/ (2026-09-02), /po-quest-boost/ (2026-09-02)
- https://developers.meta.com/vr/documentation/unity/unity-eye-tracked-foveated-rendering/ (2026-10-07)
- https://developers.meta.com/vr/documentation/unity/in-headset-fov-simulation/ (2026-09-24)
- https://developers.meta.com/vr/documentation/unity/meta-xr-operator/ (2026-09-09)
- https://developers.meta.com/vr/essentials/compatibility-mode/ (2026-09-04), /render-scale/ (2026-09-01), /memory-ram/ (2026-08-31)
- https://npm.developer.oculus.com/ package metadata (207.0.1, 2026-10-12)
- Package source read 2026-10-17: `com.meta.xr.sdk.core` 207.0.1 (`OVRProjectConfig.cs`,
  `OVRManifestPreprocessor.cs`, `OVRManager.cs`); `com.unity.xr.openxr` 1.18.0 and 1.19.0-pre.2
  (`MetaQuestFeature.cs`)

`/horizon/` documentation URLs now 302-redirect to the same path under `/vr/`.

Last verified: 2026-10-17
