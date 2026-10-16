# Meta VR Glasses for Unity developers (condensed reference)

Checked 2026-10-17 against developers.meta.com and the Meta XR Core SDK 207.0.1 package. Summaries
are in our own words. Each row cites a source key; the key table at the end gives the URL and the
page's own last-updated date. **Code** means read from the Core SDK 207.0.1 / Unity OpenXR package
source, not stated on a docs page. Re-check anything here with `metavr docs search` before relying
on it: Meta says guidance will keep changing until the device ships.

## 1. The device

| Fact | Value | Src |
|---|---|---|
| What it is | Lightweight glasses tethered to a compute puck; same Horizon OS, Store and SDKs as Quest | G1, CMP |
| Ships | Spring 2027. Build now with v207 SDKs and simulators | BLOG1, BLOG2 |
| Weight | About 100 g | BLOG1 |
| Chipset / RAM | Snapdragon XR2 Gen 3, 12 GB | CMP |
| Memory kill limit (PSS) | 5.75 GiB (same as Quest 3/3S) | MEM |
| Display | micro-OLED, Dolby Vision / HDR10 / HLG; 2412 × 2288 per eye, 37 PPD | CMP, FOV |
| Refresh | 120 Hz (spec table). XR Simulator profile offers 90 and 120 Hz, default 90. Not yet in Unity's refresh-rate table | CMP, SIM |
| Default render target (scale 1.0) | 1728 × 1760 per eye (render-scale page); the simulator profile reports 1680 × 1760 | RS, SIM |
| Tracking | Head, hands, **eyes** (Quest 3/3S have no eye tracking) | CMP, ETFR |
| Face tracking | No camera face tracking for apps; Audio To Expressions only (Movement SDK) | CMP, MOVE |
| Passthrough | Colour stereo, 26 PPD, autofocus, depth sensing | CMP |
| IPD | 56–70 mm, manual per eye. Simulator range 58–72 mm | CMP, SIM |
| Controllers | None in the box. Touch Plus sold separately | CMP, COMPAT |
| Android | Android 14 | CMP |
| Play area | Stationary or roomscale (min 6.5' × 6.5'), same as Quest | CMP |
| Passthrough Camera Access | Docs list Quest 3/3S only. **unverified** for VR Glasses | PCA |

## 2. Field of view

| Value | Measurement | Use it for | Src |
|---|---|---|---|
| Nominal straight-ahead FoV | 70° H × 66° V | Upper bound of what a person sees. Not a safe UI boundary | FOV |
| Perceived p90 target | 58° H × 54° V (`metavr device fov-sim enable --perceived`) | Conservative layout tests | FOV |
| Per-eye render extent | 74° H × 68° V (sim profile: 37° temporal, 37° nasal, 34° up, 34° down) | What the runtime renders. Never a UI boundary | FOV, SIM |
| Quest 3 / 3S for comparison | 110° × 96° / 96° × 90° | | CMP |
| Core SDK FoV mask default | inner 35°, outer 35°, **up 25°, down 41°** (asymmetric: less room above eye level) | Head-locked HUD placement. **Code**, FovSimulator.cs | Code |

Tangent half-angles for a frustum test (left, right, up, down), e.g. Meta's
`hz-unity-device-readiness` FoV pass:

| Frustum | tan values |
|---|---|
| Render extent 37/37/34/34 (documented) | 0.7536, 0.7536, 0.6745, 0.6745 |
| Core SDK mask 35/35/25/41 (code) | 0.7002, 0.7002, 0.4663, 0.8693 |
| Nominal 70 × 66 split evenly (**assumption**) | 0.7002, 0.7002, 0.6494, 0.6494 |
| Perceived 58 × 54 split evenly (**assumption**) | 0.5543, 0.5543, 0.5095, 0.5095 |

Layout rules from Meta: keep primary actions and status near the centre; no dense small targets
at the edge; give an edge-only action another route; place fixed-to-viewer UI by angle, never by
pixel offsets from the display resolution; use the engine's per-eye view/projection, don't
hard-code display values (FOV). Meta found apps putting UI at the extreme edge (BLOG1).

## 3. Input

| Fact | Value | Src |
|---|---|---|
| Primary input | Eyes target, hand pinch selects ("look and pinch") | G1, SUP |
| Assume | Most users have no controllers | SUP, BLOG1 |
| Controller-required immersive app | Installs, but the system **Switch to Controllers** dialog blocks entry until controllers are active | COMPAT |
| Store effect | Users without controllers see hand-tracked titles first (incl. hands + controllers titles) | BLOG1 |
| Gaze target size | ≥ 3.0° visual angle. Unity world size = 2 × distance × tan(1.5°) ≈ 5.2 cm at 1 m (≈ 3.1 cm at 0.6 m, 7.9 cm at 1.5 m) | EYES |
| Spacing | Prefer one-row/one-column button groups; avoid 2D grids of small targets | EYES |
| Hover | No user flow may depend on hover. 2D Android and Browser get no gaze hover events; the Connect recap says "hover events never fire". ISDK 207 still has gaze hover visuals for engine apps, so treat hover as decoration only | EYES, BLOG2, ISDK |
| Eye tracking can drop out | Targeting falls back to hand/controller ray, then head gaze. Always keep a ray fallback | EYES, ISDKGS |
| Microgestures | Supported (thumb tap, swipes); discrete, no magnitude. Map continuous moves to pinch-and-drag | MICRO, BRING |
| OVRInput pinch mapping | Index pinch is auto-mapped to `RawButton.A` (right) / `RawButton.X` (left) for hand controller types; triggers, grips, thumbsticks, B/Y have no hand equivalent. Opt out with define `OVR_DISABLE_HAND_PINCH_BUTTON_MAPPING` | Code (OVRInput.cs) |

Controller → hands mapping (BRING): ray → eye or hand ray; trigger/grip press → pinch, poke or
palm grab; hold → pinch-and-hold; trigger/grip drag → pinch-and-drag or palm grab; thumbstick
scroll → swipe or pinch-and-drag; thumbstick locomotion → teleport.

## 4. Unity setup for the VR Glasses path

| Item | Value | Src |
|---|---|---|
| Unity | 6000.0.66f2 or later (Core 207.0.1 `package.json`: `unity 6000.0`, `66f2`); 6.1+ recommended | SUP, REQ, Code |
| Packages (ISDK OVR path) | `com.meta.xr.sdk.core`, `com.meta.xr.sdk.interaction`, `com.meta.xr.sdk.interaction.ovr` | SUP |
| Latest on Meta's npm (2026-10-12) | Core, Interaction, Interaction OVR, MRUK, Haptics **207.0.1**; Platform 207.0.0. Core 207.0.1 depends on `com.unity.xr.hands` 1.7.2 | NPM |
| XR plugin | Unity OpenXR Plugin (recommended 1.15.1; Meta XR Operator needs ≥ 1.17.0). Oculus XR Plugin deprecated | XRP, OP |
| Hand Tracking Support (OVRManager > Quest Features > General) | Controllers And Hands → `oculus.software.handtracking` required=false + `HAND_TRACKING` permission; Hands Only → required=true; Controllers Only → no entry | SUP, COMPAT, Code |
| Eye Tracking Support (same panel) | Supported → `oculus.software.eye_tracking` required=false; Required → required=true; both add `com.oculus.permission.EYE_TRACKING`; None → nothing | SUP |
| Custom `Assets/Plugins/Android/AndroidManifest.xml` | Existing hand-tracking entries are kept, not updated. Use Window > Meta > Tools > Android Manifest Tool > **Update AndroidManifest.xml for Store Compatibility** | SUP |
| Simulator check | XR Simulator runs in Play mode and writes no manifest: check declarations on a built APK | SUP |
| Binary | IL2CPP + ARM64. A 32-bit-only binary (Mono) cannot run on VR Glasses at all | SUP, COMPAT |
| Gaze interaction | ISDK 207 `EyeGaze` → `GazeConecaster` → `GazeInteractor` → `GazeInteractable`. Right-click a Canvas > Interaction SDK > **Add Gaze Interaction to Canvas**; quick actions keep **Enable Ray Fallback** on by default | GAZE, ISDKGS |
| UnityXR rig | `UnityXREyeTrackingActiveState` needs **Eye Gaze Interaction Profile** in OpenXR Enabled Interaction Profiles | GAZE |
| Face animation | Audio To Expressions via Movement SDK | SUP |

## 5. Manifest device list and Store targeting (sources disagree)

| Source | What it says | Src |
|---|---|---|
| Manifest requirements page (2026-10-15) | Canonical `com.oculus.supportedDevices` id for VR Glasses is `vrglasses` (`quest2\|questpro\|quest3\|quest3s\|vrglasses`) | MAN |
| Compatibility-mode pages (2026-09-04) | VR Glasses are **not currently a supported public value**; select **Future devices** + at least one Quest in Store targeting; Horizon OS runs third-party immersive Store apps in compatibility mode on VR Glasses | CM, CMU |
| Core SDK 207.0.1 | `OVRProjectConfig.DeviceType.VRGlasses = 6` (on by default in new configs); the Gradle manifest patch writes the internal key **`stanley`**, not `vrglasses` | Code |
| Unity OpenXR 1.18.0 and 1.19.0-pre.2 | Meta Quest feature target list has only `quest`, `quest2`, `cambria` (Pro), `eureka` (3), `quest3s`. No VR Glasses entry | Code |
| Device targeting page (2026-09-04) | "All Devices" includes VR Glasses. `com.meta.store.defaultDeviceTargets` specifier `quest3+` covers Quest 3 family, VR Glasses and future devices. Every build in a submission must be forward-compatible (Future devices) | DT |

**Practical rule:** don't hand-edit `supportedDevices`. Ship to VR Glasses through Developer
Dashboard targeting (**Future devices** + each Quest you support), read the merged manifest with
`aapt2` to see what was written, and treat the manifest id as unresolved until Meta's pages agree.

**Compatibility mode consequences** (CM, CMU): device-ID APIs (including
`OVRPlugin.GetSystemHeadsetType()`) can report an older Quest model; refresh-rate lists are limited
to rates both models support; backends see the compatibility model. Check capabilities (eye
tracking supported, hands/controllers connected), never the model string.

## 6. Performance

| Fact | Value | Src |
|---|---|---|
| Baseline | An app running well at nominal on Quest 3 reaches the same frame rate on VR Glasses (net of higher resolution and narrower FoV). Meta expects most apps at parity or better | PERF, BLOG1 |
| Design target | CPU level 4 / GPU level 4 ("nominal") steady state; OS raises clocks when headroom exists | PERF, LVL |
| Not available | GPU level 5, dual-core mode, CPU/GPU level trading | LVL, BOOST |
| CPU Boost | Raises CPU level 4 → 8 (~30%). Token-bucket budget (starts at 45 s, refills) instead of Quest 3's 45 s / 20% limits. `OVRManager.suggestedCpuPerfLevel = ProcessorPerformanceLevel.Boost;` then back to `SustainedHigh` | BOOST |
| Dynamic resolution | Recommended on VR Glasses (no single render scale matches the panel). `OVRManager.instance.enableDynamicResolution` / OVRCameraRig **Enable Dynamic Resolution** with min/max scale | RS, DYN |
| Foveation | FFR saves less than on Quest 3 (larger GPU tiles). ETFR available (eye tracking + permission). `OVRManager.foveatedRenderingLevel`, `useDynamicFoveatedRendering`; ETFR: check `eyeTrackedFoveatedRenderingSupported`, set `eyeTrackedFoveatedRenderingEnabled`; Vulkan + multiview only | PERF, ETFR |
| GPU gain | More GMEM → fewer tiles: geometry-heavy tiled forward passes gain most; full-screen passes (post, resolves) gain little | PERF |
| Thermal | Separate thermal domains for glasses and puck; test 45 min (or content length) with a fixed puck placement, vents clear | THERM, TEST |
| Store gate | Same VRCs (VRC.Quest.Performance.1: no extended period below 60 rendered FPS) | TEST |

## 7. Platform services

Same Store delivery and Platform Services as Quest (PS). Same Store app → existing entitlement,
IAP and add-ons carry over. Cross-device play is not automatic. Cloud Backup restores the most
recent backup from another owned device **on install only**; it is not save sync; 100 MB per
backup; only supported directories (PS, CB). **Check** (unverified effect): OVRProjectConfig
**Disable Backups** (on by default in new configs, Code) writes `android:allowBackup="false"`.

## 8. Testing without the device

| Tool | What it does | Src |
|---|---|---|
| Meta XR Simulator 207.1 (standalone app) | **Meta VR Glasses** device profile (restart Play mode after switching); eye gaze + hands by default, no controllers. Look and Pinch mode (v207+): mouse = gaze, left-click = pinch, I/Y/H/G/J = microgestures. Hands appear as `/interaction_profiles/ext/hand_interaction_ext`; gaze needs `XR_EXT_eye_gaze_interaction`; microgestures need `XR_EXT_hand_interaction` + `XR_META_hand_tracking_microgestures` | SIM, LP, SIMDL |
| `metavr device fov-sim enable [--perceived]` / `disable` | OS-level FoV crop on a Quest 3S / 3 / Pro; resets on reboot. Doesn't clip compositor layers before Horizon OS v209; casts/screenshots show the pre-lens image | FOV, FOVU |
| OVRManager > Quest Features > Experimental > **Simulate Device Field of View** | Unity mask (70° × 66° default) in Editor, Link, Simulator or an APK. Needs **Experimental Features Enabled**, which marks the build experimental: turn it off and rebuild before Store submission | FOVU |
| Immersive Debugger > **[Meta] FoV Simulation** | Toggle/tune the same mask live | FOVU |
| ISDK sample `GazeExamples` scene | Starting point for Look and Pinch tests | LP, BRING |
| Device Readiness Check (Core ≥ 207, ISDK ≥ 205; experimental) | Window > Meta > Tools > Device Readiness Check. Read-only scan; Standard check (no AI) or AI-powered check; Export as Markdown. See [unity-xr-operator.md](unity-xr-operator.md) §3 for what it scans and the AI-mode caveat | DRC |

Device-only checks to defer until hardware: Store-channel install, sustained thermals and puck
placement, compositor/optics, cross-device Cloud Backup (TEST).

## 9. v207 SDK changes that matter here

| Package | Change | Src |
|---|---|---|
| Core 207.0 | FoV simulation (OVRManager + Immersive Debugger); Device Readiness Check (experimental); SDK toolbar moved to Window > Meta > Meta XR SDK (pin via toolbar right-click on 6000.3+); Meta XR Operator gains hand-gesture, joint-animation and gaze tooling and one-click Quest setup; Immersive Debugger AI assistant shares Operator's MCP server | CORE |
| Interaction SDK 207.0 | Gaze interactions (OVR and UnityXR data sources), Gaze Quick Actions and Building Blocks, `GazeMultiGrab` two-hand rotate/scale, gaze hover spotlight for 3D, `SliderGazePointerOffset`, `OSPanelTransformer`, `DiscreteScrollInputProvider` + `MicrogestureScroll` prefab, MR Tabletop sample; Ray grabs now default to `MoveFromTargetAtAnchor`; `RayInteractor` candidate properties are reused per frame (clone to keep them) | ISDKRN |
| MRUK 207.0 | `SceneNavigation.MinRegionArea`; shaders split URP/BiRP (default URP); Scene model V2 (HiFi) disabled | MRUK |
| Platform SDK 207.0 | Error codes now match docs (re-test branches on codes); `Users.GetLoggedInUser` no longer returns ID "0" when logged out; several Link/PC fixes | PSDK |
| XR Simulator 207.1 (2026-10-14) | AMD GPU support; camera hand tracking reports grab/pinch; fixes for memory growth, exit crashes, stuck Look and Pinch hands and VR Glasses input. Known: restart the Editor after updating the OpenXR package | SIMDL |
| `com.meta.xr.simulator` package | Deprecated since v83 (last 81.0.1); the standalone app + Core SDK replace it | SIMOLD |

## Source keys

Paths are the canonical `/vr/` form; the older `/horizon/` paths 302-redirect to them (checked
2026-10-17).

| Key | URL (developers.meta.com unless noted) | Last updated |
|---|---|---|
| G1 | /vr/essentials/get-started-with-glasses/ | 2026-10-13 |
| BRING | /vr/essentials/bring-your-app-glasses/ | 2026-10-13 |
| COMPAT | /vr/essentials/app-compatibility/ | 2026-10-13 |
| FOV | /vr/essentials/field-of-view/ | 2026-09-25 |
| PS | /vr/essentials/platform-services/ | 2026-08-25 |
| TEST | /vr/essentials/test-your-app/ | 2026-09-25 |
| CMP | /vr/essentials/compare-devices/ | 2026-09-30 |
| CM | /vr/essentials/compatibility-mode/ | 2026-09-04 |
| MEM | /vr/essentials/memory-ram/ | 2026-08-31 |
| RS | /vr/essentials/render-scale/ | 2026-09-01 |
| LVL | /vr/essentials/cpu-gpu-levels/ | 2026-09-11 |
| THERM | /vr/essentials/thermal/ | 2026-09-15 |
| EYES | /vr/design/eyes-best-practices/ | 2026-10-13 |
| SUP | /vr/documentation/unity/unity-support-meta-vr-glasses/ | 2026-09-30 |
| DRC | /vr/documentation/unity/device-readiness/ | 2026-09-24 |
| FOVU | /vr/documentation/unity/in-headset-fov-simulation/ | 2026-09-24 |
| PERF | /vr/documentation/unity/optimize-performance/ | 2026-09-19 |
| BOOST | /vr/documentation/unity/po-quest-boost/ | 2026-09-02 |
| DYN | /vr/documentation/unity/dynamic-resolution-unity/ | 2026-08-06 |
| ETFR | /vr/documentation/unity/unity-eye-tracked-foveated-rendering/ | 2026-10-07 |
| CMU | /vr/documentation/unity/os-compatibility-mode/ | 2026-09-04 |
| GAZE | /vr/documentation/unity/unity-isdk-gaze-interaction/ | 2026-09-09 |
| ISDKGS | /vr/documentation/unity/unity-isdk-getting-started/ | 2026-09-14 |
| MICRO | /vr/documentation/unity/unity-microgestures/ | 2026-10-07 |
| MOVE | /vr/documentation/unity/move-overview/ | 2026-09-30 |
| PCA | /vr/documentation/unity/unity-pca-overview/ | 2026-10-07 |
| REQ | /vr/documentation/unity/unity-development-requirements/ | 2026-08-11 |
| XRP | /vr/documentation/unity/unity-xr-plugin/ | 2026-10-07 |
| CB | /vr/documentation/unity/ps-cloud-backup/ | 2026-05-11 |
| SIM | /vr/documentation/unity/xrsim-getting-started/ | 2026-09-03 |
| LP | /vr/documentation/unity/xrsim-look-and-pinch/ | 2026-09-03 |
| OP | /vr/documentation/unity/meta-xr-operator/getting-started/ | 2026-09-09 |
| MAN | /vr/resources/publish-mobile-manifest/ | 2026-10-15 |
| DT | /vr/resources/publish-release-channels-device-targeting/ | 2026-09-04 |
| CORE | /vr/downloads/package/meta-xr-core-sdk/ (207.0) | 2026-09-22 |
| ISDKRN | /vr/downloads/package/meta-xr-interaction-sdk/ (207.0) | 2026-09-22 |
| MRUK | /vr/downloads/package/meta-xr-mr-utility-kit-upm/ (207.0) | 2026-09-22 |
| PSDK | /vr/downloads/package/meta-xr-platform-sdk/ (207.0) | 2026-09-22 |
| SIMDL | /vr/downloads/package/meta-xr-simulator-mac-arm/ and -windows/ (207.1) | 2026-10-14 |
| SIMOLD | /vr/downloads/package/meta-xr-simulator/ (81.0) | 2025-10-30 |
| NPM | https://npm.developer.oculus.com/ (package metadata) | 2026-10-12 |
| BLOG1 | /blog/meta-connect-recap-start-building-the-future-of-vr/ | 2026-09-24 |
| BLOG2 | /blog/meta-connect-recap/ | 2026-09-24 |
| Code | `com.meta.xr.sdk.core` 207.0.1: `Editor/OVRProjectConfig.cs`, `Editor/OVRManifestPreprocessor.cs`, `Scripts/OVRInput.cs`, `Scripts/Util/FovSimulator/FovSimulator.cs`; `com.unity.xr.openxr` 1.18.0 / 1.19.0-pre.2 `Runtime/MetaQuest/MetaQuestFeature.cs` | read 2026-10-17 |

The `.md` copies of the docs pages are listed in https://developers.meta.com/vr/llmstxt/documentation/unity/llms.txt/
and https://developers.meta.com/vr/llmstxt/essentials/llms.txt/.
