---
name: quest-unity-project-settings
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.0.0"
description: >-
  Recommended Unity settings for Meta Quest performance and Horizon Store
  compatibility, each checked against Meta's docs: Unity version floor for Meta
  XR SDK, Unity OpenXR plugin (Oculus XR plugin deprecated), Meta XR feature
  group, Project Setup Tool, IL2CPP/ARM64/API levels, Vulkan, ASTC, MSAA 4x in
  URP, Quality settings, OpenXR Meta Quest options (multiview, symmetric
  projection, optimize buffer discards, late latching), fixed foveated
  rendering, Application SpaceWarp, refresh rates, per-device frame and
  draw-call budgets for Quest 2/3/3S. Includes an Editor audit/apply script for
  batch mode and CI. Use when the user asks "what settings for Quest", "optimize
  my Unity project for Quest", "Vulkan or OpenGL", "OpenXR or Oculus plugin",
  "enable foveated rendering", "enable SpaceWarp", "set 90 Hz/120 Hz", "target
  API level", or "audit project settings".
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
| Meta SDK | Meta XR All-in-One SDK (`com.meta.xr.sdk.all`), latest **207.0** (2026-09-22), or individual packages such as `com.meta.xr.sdk.core`. Install from the Unity Asset Store (recommended) or Meta NPM tarballs | Meta-rec | downloads/package/meta-xr-sdk-all-in-one-upm; unity-package-manager |
| XR plugin | **Unity OpenXR Plugin** (recommended **1.15.1**; needs Unity 6+ and Meta XR SDK v74+). The Oculus XR Plugin is deprecated and scheduled for removal | Meta-rec | unity-xr-plugin (2026-10-07) |
| OpenXR features | XR Plug-in Management > OpenXR > enable **Meta XR** for **Standalone and Android** | Meta-rec | unity-project-setup (2026-10-15) |
| Project Setup Tool | Window > Meta > Tools > Project Setup Tool > **Fix All** and **Apply All** on Standalone and Android tabs until no Required issues remain | Meta-rec | unity-project-setup; unity-upst-overview (2026-08-25) |
| Depth API occlusion | Also needs `com.unity.xr.meta-openxr@2.1.0` and Unity 6+ | Meta-rec (if used) | unity-xr-plugin |

To automate setup, use `OVRProjectSetup.FixAllAsync(BuildTargetGroup.Android)` and `(Standalone)`
via `unity-cli` (see `hz-unity-meta-core-sdk/references/project-setup-tool.md`). For a CI or batch
report, run
`-executeMethod OVRProjectSetupCLI.GenerateProjectSetupReport [-reportFile <path>] [-buildTarget Android]`.
It writes JSON with each task's group, level and `isDone` (unity-upst-overview).

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
| Target devices | Only headsets you have tested. This sets `com.oculus.supportedDevices` | Meta-rec | unity-openxr-settings-quest; unity-conf-settings |
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

**Refresh rate** (unity-set-disp-freq, from search results, not fetched in full: verify)
- `OVRPlugin.systemDisplayFrequency = 90.0f;`. Request 90 or 120 Hz only if the game holds it.
  Fall back to a rate the device reports. `OVRManager.DisplayRefreshRateChanged` signals runtime
  changes. Allowed interactive rates: 72, 80, 90, 96, 100, 120 Hz. Not all are on every device.
  See VRC.Quest.Performance.1.

**Dynamic resolution and CPU/GPU levels**
- Keep render scale ≥ 85% for most of the game (VRC.Quest.Performance.4). Meta's
  best-practices page credits dynamic resolution with access to GPU level 5.
- `OVRManager.cpuLevel` / `OVRManager.gpuLevel` come from an older, undated page. Verify with
  `metavr docs api-details OVRManager` before using them, and lock levels only when profiling.

## 5. Budgets per device

See [references/performance-targets.md](references/performance-targets.md). The headline numbers:
interactive apps target **72 FPS minimum** (Meta perf docs). The VRC floor is 60 fps rendering.
Per-frame budgets are 13.8 ms at 72 Hz, about 11.1 ms at 90 Hz and about 8.3 ms at 120 Hz. Draw
calls for busy scenes: Quest 2/Pro 80-200, Quest 3/3S 200-300.

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
Devices, System Splash Screen.

## Sources

- https://developers.meta.com/horizon/documentation/unity/unity-development-requirements/ (2026-08-11)
- https://developers.meta.com/horizon/documentation/unity/unity-xr-plugin/ (2026-10-07)
- https://developers.meta.com/horizon/documentation/unity/unity-project-setup/ (2026-10-15)
- https://developers.meta.com/horizon/documentation/unity/unity-upst-overview/ (2026-08-25)
- https://developers.meta.com/horizon/downloads/package/meta-xr-sdk-all-in-one-upm/ (207.0, 2026-09-22)
- https://developers.meta.com/horizon/documentation/unity/unity-package-manager/ (2025-06-01)
- https://developers.meta.com/horizon/documentation/unity/unity-prepare-for-publish/ (2026-04-27)
- https://developers.meta.com/horizon/resources/publish-mobile-manifest/ (2026-10-15)
- https://developers.meta.com/horizon/documentation/unity/unity-conf-settings/ (2025-02-18)
- https://developers.meta.com/horizon/documentation/unity/unity-openxr-settings-quest/ (2026-10-07)
- https://developers.meta.com/horizon/documentation/unity/os-vulkan-opengl/ (undated)
- https://developers.meta.com/horizon/documentation/unity/unity-fixed-foveated-rendering/ (2026-10-07)
- https://developers.meta.com/horizon/documentation/unity/unity-asw/ (2026-05-13)
- https://developers.meta.com/horizon/documentation/unity/unity-best-practices-intro/ (2024-09-15)
- https://developers.meta.com/horizon/documentation/unity/unity-mobile-performance-intro/ (undated)
- https://developers.meta.com/horizon/documentation/unity/unity-set-disp-freq/ (search summary only)
- https://developers.meta.com/horizon/documentation/unity/unity-quest-runtime-optimizer/ (2026-07-31)
- https://developers.meta.com/horizon/blog/tech-note-unity-settings-for-mobile-vr/ (old)

Last verified: 2026-10-16
