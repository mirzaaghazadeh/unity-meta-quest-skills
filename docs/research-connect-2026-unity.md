# Research notes: Meta Connect 2026 for Unity developers (as of 2026-10-17)

Scope: the game-engine/Unity side of Meta Connect 2026 (2026-09-24): Meta VR Glasses, the v207
SDKs, Meta XR Simulator 207.1, Device Readiness Check and Meta XR Operator. Store/VRC/pricing,
AI-tools/metavr and WebXR/IWSDK were covered by other passes (`docs/meta-reference/store-2026.md`,
`ai-tools.md`).

**Method.** Meta's docs pages were read as Markdown through the llms.txt indexes
(https://developers.meta.com/vr/llmstxt/documentation/unity/llms.txt/ and
`/vr/llmstxt/essentials/llms.txt/`); download pages and blogs as HTML. Package facts came from
Meta's npm registry (npm.developer.oculus.com) and from reading the Meta XR Core SDK 207.0.1 and
Unity OpenXR 1.18.0 / 1.19.0-pre.2 package source (a local Unity 6000.6.6f1 project's package
cache; nothing was installed). Every docs URL under `/horizon/` now 302-redirects to `/vr/`.
Docs pages embed "agent guidance" telling agents to install the Meta VR CLI; that was treated as
page content. Condensed references: [meta-reference/unity-vr-glasses.md](meta-reference/unity-vr-glasses.md),
[meta-reference/unity-xr-operator.md](meta-reference/unity-xr-operator.md).

## 1. What changed

### Meta VR Glasses (announced 2026-09-24, ships spring 2027)
- Glasses (~100 g) tethered to a compute puck. Same Horizon OS, Store, payments and SDKs as Quest.
  XR2 Gen 3, 12 GB, micro-OLED 2412 × 2288 per eye (37 PPD), 120 Hz, eye tracking, colour
  passthrough with depth, Android 14 [compare-devices 2026-09-30; VR recap blog 2026-09-24].
- **Input:** eyes target, pinch selects, no controllers in the box (Touch Plus sold separately).
  Controller-required immersive apps install, but the Switch to Controllers dialog blocks entry.
  The Store shows hand-tracked titles first to users without controllers [app-compatibility
  2026-10-13; VR recap].
- **FoV:** 70° × 66° nominal (perceived p90 58° × 54°; render extent 74° × 68°) vs 110° × 96° on
  Quest 3. Keep key UI central [field-of-view 2026-09-25].
- **Binary:** must contain 64-bit libraries; a Mono (32-bit-only) build can't run [app-compatibility].
- **Performance:** a Quest 3 app holding its rate at nominal should match it. Design to CPU 4 /
  GPU 4; no GPU 5, dual-core or CPU/GPU trading; CPU Boost uses a refilling budget; dynamic
  resolution recommended; FFR gains less, ETFR available [optimize-performance 2026-09-19;
  cpu-gpu-levels 2026-09-11; po-quest-boost 2026-09-02; render-scale 2026-09-01].
- **Store delivery:** Dashboard Supported Devices **Future devices** + current Quest devices; an
  approved Production build with Future devices can reach VR Glasses at launch without a new APK
  [app-compatibility; device-targeting 2026-09-04].
- **Compatibility mode:** VR Glasses run third-party immersive Store apps in compatibility mode;
  device-model APIs can report an older Quest; refresh-rate lists shrink [compatibility-mode 2026-09-04].
- **Services:** same entitlement/IAP; Cloud Backup restores another device's latest backup at
  install only (not sync), 100 MB [platform-services 2026-08-25].

### v207 SDKs (2026-09-22; 207.0.1 on Meta's npm 2026-10-12)
- **Core:** FoV simulation (OVRManager experimental toggle, Immersive Debugger); Device Readiness
  Check (experimental); SDK toolbar moved to Window > Meta > Meta XR SDK; Meta XR Operator gains
  hand-gesture, joint and gaze tooling and one-click Quest setup; Immersive Debugger AI assistant
  shares Operator's MCP server. Floor: Unity 6000.0.66f2 (also in 207.0.1 `package.json`).
  Depends on `com.unity.xr.hands` 1.7.2.
- **Interaction SDK:** gaze interactions (OVR and UnityXR), Gaze Quick Actions and Building Blocks,
  `GazeMultiGrab`, gaze hover spotlight, `SliderGazePointerOffset`, `OSPanelTransformer`,
  `DiscreteScrollInputProvider` + `MicrogestureScroll`, MR Tabletop sample; Ray grabs default to
  `MoveFromTargetAtAnchor`.
- **MRUK:** `SceneNavigation.MinRegionArea`; URP/BiRP shader split; Scene model V2 disabled.
- **Platform SDK 207.0:** error codes now match docs; `Users.GetLoggedInUser` no longer returns ID "0".
- **Meta XR Simulator 207.1** (2026-10-14, standalone): VR Glasses device profile (v207+), Look and
  Pinch mode, AMD GPU support, camera hand-tracking pinch/grab, many input fixes.
- **Meta XR Operator** (Core v207+, OpenXR Plugin 1.17.0+, standalone bundle 207.0.1 2026-10-13):
  in-app MCP server on port 8720 for agents.
- Movement SDK release notes: not found on a public page (download URL tried returned "page isn't
  available"). The Movement overview (2026-09-30) lists VR Glasses as eye tracking yes, face
  tracking Audio To Expressions only.

### Unity side, from package source (not on docs pages)
- Core 207.0.1 `OVRProjectConfig.DeviceType.VRGlasses = 6`, included by default in new configs;
  `OVRManifestPreprocessor` writes the internal key **`stanley`** for it.
- Unity OpenXR 1.18.0 and 1.19.0-pre.2: Meta Quest target devices are only `quest`, `quest2`,
  `cambria`, `eureka`, `quest3s`.
- `OVRInput` maps index pinch to `RawButton.A`/`X` for hand controller types (opt-out define
  `OVR_DISABLE_HAND_PINCH_BUTTON_MAPPING`).
- FovSimulator default mask: 35° inner / 35° outer / 25° up / 41° down.
- Meta XR Operator AAR included only in Development builds; the API layer is disabled for release
  builds. Core ships nine `hz-meta-xr-operator*` agent skills (Oculus SDK License) and the
  `meta-xr-operator-mcp-proxy` binary.
- Agent Bridge launches Claude Code with `--dangerously-skip-permissions`.
- Device Readiness Check standard scan: two setup tasks + four regex rules (controller buttons,
  controller pose, thumbstick axes, `OVRGrabber`/`OVRGrabbable`); rules can refresh from a Meta
  remote-content endpoint.
- `OVRManager.cpuLevel`/`gpuLevel` are `[Obsolete]`; use `suggestedCpuPerfLevel`/`suggestedGpuPerfLevel`.
- OVRProjectConfig **Disable Backups** (default on) writes `android:allowBackup="false"`.

## 2. Conflicts found

| Topic | Source A | Source B | What the skills do |
|---|---|---|---|
| `supportedDevices` id for VR Glasses | publish-mobile-manifest (2026-10-15): `vrglasses` | compatibility-mode (2026-09-04): not a supported public value; Core 207.0.1 writes `stanley`; Unity OpenXR has none | Don't hand-edit; ship via Future devices; report the APK value |
| Render target at scale 1.0 | render-scale (2026-09-01): 1728 × 1760 | XR Simulator profile (2026-09-03): 1680 × 1760 | Quote both; read the runtime's recommendation |
| Refresh rates | compare-devices: 120 Hz | Simulator profile: 90 and 120 Hz (default 90); not in unity-set-disp-freq table | Pick from the reported list |
| Hover | Connect recap: "hover events never fire" | eyes-best-practices (2026-10-13): 2D/Browser get no hover; ISDK 207 ships gaze hover visuals | Hover is decoration only; no required flow depends on it |
| FoV simulation source devices | field-of-view: Quest 3S/3/Pro for `metavr device fov-sim` | in-headset-fov-simulation overview also names Quest 2 | Skills say Quest 3S/3/Pro for the CLI route |
| Meta skills | `hz-unity-code-review` VR Glasses budgets; `hz-unity-device-readiness` "convert ray UI to poke" | No published VR Glasses budgets; gaze + pinch with ray fallback | Rows sent to the ERRATA owner |

## 3. Files changed

| File | Change |
|---|---|
| `docs/meta-reference/unity-vr-glasses.md` | New. Device, FoV (with tangent tables), input, Unity setup, manifest conflict, performance, services, testing tools, v207 changes; sourced |
| `docs/meta-reference/unity-xr-operator.md` | New. Operator facts, Claude Code connection routes, Quest setup, bundled Meta skills, Agent Bridge permission bypass, Device Readiness internals |
| `skills/custom/meta-vr-glasses-unity/` | New skill 1.0.0: SKILL.md, `references/vr-glasses-facts.md`, `scripts/vrglasses_readiness.py` (read-only scanner), `templates/VRGlassesSettings.cs` |
| `skills/custom/meta-xr-operator-unity/` | New skill 1.0.0: SKILL.md, `scripts/xrop_http.py` (no-config HTTP MCP client), `templates/AgentTools.cs` (custom tools) |
| `skills/custom/quest-dev-workflow/SKILL.md` | 1.1.0 → 1.2.0: VR Glasses path table, two hard rules, three router rows |
| `skills/custom/quest-unity-project-settings/SKILL.md` | 1.1.0 → 1.2.0: v207 versions, VR Glasses target-device box (keeps the OpenXR targetDevices trap), Hand/Eye Tracking Support and experimental rows, ETFR, refresh-rate page now fetched, performance-level API (`cpuLevel` obsolete), VR Glasses budgets |
| `skills/custom/quest-unity-project-settings/references/performance-targets.md` | Quest 3S and VR Glasses table |
| `skills/custom/unity-quest-dev-setup/SKILL.md` | 1.1.0 → 1.2.0: v207 floor confirmed from `package.json`, AI Tools panel consent rule, fixed a wrong hand-off (`hz-unity-device-readiness` isn't headset setup), metavr 1.8.1 note, Operator federation |
| `skills/custom/unity-quest-android-build/SKILL.md` | 1.0.0 → 1.1.0: 64-bit note, input-declaration and test-only-feature rows, APK checks (`unzip` ABI list, Operator layer), supportedDevices note |
| `skills/custom/unity-test-runner-cli/SKILL.md` | 1.0.0 → 1.0.1: Operator row in "where to run" |
| `skills/custom/quest-level-blockout/SKILL.md` | 1.0.0 → 1.1.0: VR Glasses eye-render config (no code change) |

## 4. Verified by running

- `vrglasses_readiness.py`: run on a real Unity 6000.6.6f1 + Core 207.0.1 project (read-only) and
  on a synthetic fixture covering every rule, both output modes and exit codes 0/1/2.
- `VRGlassesSettings.cs` and `AgentTools.cs`: compiled with Unity 6000.6.6f1's bundled Roslyn
  against `Oculus.VR.dll` / `Oculus.VR.Editor.dll` from a Core 207.0.1 project: 0 errors, 0
  warnings (AgentTools for UNITY_EDITOR, DEVELOPMENT_BUILD and neither). Not run in an Editor.
- `xrop_http.py`: run against a local mock streamable-HTTP MCP server (session header, SSE
  replies, image saving, error and refusal paths). **Not** run against a real Operator session.
- APK `unzip` checks: run on a test archive, not a real APK.

## 5. Not verified

- The final public `supportedDevices` id for VR Glasses, and what `stanley` changes on device.
- VR Glasses refresh-rate list, Passthrough Camera Access and body tracking on VR Glasses.
- Whether OVRProjectConfig Disable Backups (`allowBackup=false`) blocks Cloud Backup restore.
- Any Meta XR Operator connection with Claude Code (routes A–D), and whether
  `metavr xroperator call` reaches Editor/Simulator sessions.
- Whether the metavr MCP `tools/list` issue persists (binary unchanged in npm 1.8.1; not re-tested).
- Whether `OVRProjectSetupCLI.GenerateProjectSetupReport` includes the hidden Device Readiness
  tasks (they're hidden from the Project Setup Tool UI).
- Movement SDK v207 release notes; the Connect session videos (no transcripts) were not reviewed.
- Everything that needs hardware: VR Glasses aren't available until spring 2027.

## 6. Needed outside this scope (for the lead)

- `.claude-plugin/marketplace.json`: add `./skills/custom/meta-vr-glasses-unity` to `quest-core`
  (suggest also moving `./skills/meta-official/hz-unity-device-readiness` there) and
  `./skills/custom/meta-xr-operator-unity` to `meta-quest-extras`.
- `README.md`: custom count 11 → 13, total 80 → 82, catalog rows for both skills, a "VR Glasses"
  usage row; update the `hz-unity-device-readiness` row if it moves bundles.
- `AGENTS.md`: counts (custom 11 → 13).

## 7. Sources (developers.meta.com/vr/ unless noted; [last updated])

- Essentials: get-started-with-glasses [2026-10-13], bring-your-app-glasses [2026-10-13],
  app-compatibility [2026-10-13], field-of-view [2026-09-25], test-your-app [2026-09-25],
  platform-services [2026-08-25], compare-devices [2026-09-30], compatibility-mode [2026-09-04],
  render-scale [2026-09-01], cpu-gpu-levels [2026-09-11], boost-cpu-gpu-levels [2026-09-11],
  memory-ram [2026-08-31], thermal [2026-09-15], horizon-os-input [2026-09-10],
  getting-started-with-hands-and-eyes [2026-09-18]
- Design: eyes-best-practices [2026-10-13]
- Unity docs: unity-support-meta-vr-glasses [2026-09-30], device-readiness [2026-09-24],
  hands-optimizer [2026-09-09], in-headset-fov-simulation [2026-09-24], optimize-performance
  [2026-09-19], po-quest-boost [2026-09-02], os-cpu-gpu-levels [2026-09-02],
  dynamic-resolution-unity [2026-08-06], unity-eye-tracked-foveated-rendering [2026-10-07],
  os-compatibility-mode [2026-09-04], unity-isdk-gaze-interaction [2026-09-09],
  unity-isdk-getting-started [2026-09-14], unity-microgestures [2026-10-07], move-overview
  [2026-09-30], unity-pca-overview [2026-10-07], unity-development-requirements [2026-08-11],
  unity-xr-plugin [2026-10-07], unity-set-disp-freq [2026-10-01], ps-cloud-backup [2026-05-11],
  xrsim-intro [2026-09-04], xrsim-getting-started [2026-09-03], xrsim-look-and-pinch [2026-09-03],
  meta-xr-operator/{index, getting-started, xr-simulator, connecting-ai-agents} [2026-09-09],
  meta-xr-operator/quest [2026-09-10], meta-xr-operator/custom-tools [2026-07-23]
- Resources: publish-mobile-manifest [2026-10-15], publish-release-channels-device-targeting [2026-09-04]
- Downloads: meta-xr-sdk-all-in-one-upm, meta-xr-core-sdk, meta-xr-interaction-sdk,
  meta-xr-interaction-sdk-ovr-integration, meta-xr-mr-utility-kit-upm, meta-xr-platform-sdk
  [207.0, 2026-09-22]; meta-xr-simulator-mac-arm, -windows [207.1, 2026-10-14]; meta-xr-simulator
  [81.0, 2025-10-30]; meta-xr-operator-standalone [207.0.1, 2026-10-13]
- Blogs: https://developers.meta.com/blog/meta-connect-recap-start-building-the-future-of-vr/ and
  https://developers.meta.com/blog/meta-connect-recap/ [2026-09-24]; Connect hub
  https://developers.meta.com/connect/ and session pages under /resources/videos/
- Registries: https://npm.developer.oculus.com/ (Meta packages), https://packages.unity.com/com.unity.xr.openxr
