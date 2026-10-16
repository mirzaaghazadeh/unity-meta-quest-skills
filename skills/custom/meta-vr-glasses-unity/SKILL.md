---
name: meta-vr-glasses-unity
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.0.0"
description: >-
  Makes a Unity Meta Quest app ready for Meta VR Glasses (announced at Meta
  Connect 2026, ships spring 2027, same Horizon OS, Store and v207 SDKs): a
  headless read-only readiness scan, Meta's Device Readiness Check, the
  controller-to-eyes-and-hands migration with Interaction SDK gaze (look and
  pinch), Hand/Eye Tracking Support and manifest declarations, the 70x66 degree
  field of view and head-locked UI, performance rules (nominal CPU/GPU 4, no GPU
  level 5, CPU Boost budget, dynamic resolution, ETFR), testing in Meta XR
  Simulator's VR Glasses profile and with FoV simulation on a Quest, APK checks
  and Store "Future devices" targeting. Use when the user mentions Meta VR
  Glasses, VR glasses, look and pinch, gaze input, hands-only or controller-free
  play, "will my Quest game work without controllers", the narrower FoV,
  compatibility mode, or porting a Quest app to the new device.
---

# Meta VR Glasses readiness for a Unity Quest app

> Made by [Navid Mirzaaghazadeh](https://github.com/mirzaaghazadeh) · part of [unity-meta-quest-skills](https://github.com/mirzaaghazadeh/unity-meta-quest-skills) · MIT

Meta VR Glasses run the same OS, Store and SDKs as Quest, so the build is familiar. What changes is
input (eyes target, a pinch selects, no controllers in the box), a 70° × 66° field of view, and a
few performance rules. Device facts with sources: [references/vr-glasses-facts.md](references/vr-glasses-facts.md).

Hand-offs:
- Code-level controller audit and the head-locked-UI frustum test → Meta's **`hz-unity-device-readiness`**
- Interaction SDK rigs, grab, poke, ray → **`hz-unity-meta-interaction-sdk`**; VR UI → **`hz-unity-meta-quest-ui`**
- Simulator install/control → **`hz-xr-simulator-install-and-configure`**, **`hz-xr-simulator-control`**
- Let an agent drive the running app (gaze + pinch, screenshots) → **`meta-xr-operator-unity`**
- Player/XR settings → **`quest-unity-project-settings`**; APK build → **`unity-quest-android-build`**
- Upload and device targeting → **`horizon-store-publish-unity`**; review rules → **`horizon-vrc-checklist`**

## Ground rules

- **Verify first.** Meta says this guidance changes until the device ships. Check anything you're
  about to rely on with `metavr docs search "<topic>"` (`hz-quest-verify-first`).
- **Ask before** changing `ProjectSettings/`, OVRProjectConfig, manifests or packages, and before
  turning on Meta's AI-powered checks (they run Claude Code with permissions bypassed; see
  `meta-xr-operator-unity`).
- **Don't declare hands before hands work.** The manifest entry only advertises support; it adds
  no hand input. A wrong declaration ships a broken app to controller-less users.
- **Don't hand-edit `com.oculus.supportedDevices`.** Meta's pages disagree on the VR Glasses id
  (facts file, "Manifest ids"). Delivery is decided in the Developer Dashboard.
- **A simulator pass is not a device pass.** Say which checks wait for hardware.

## Step 0: Decide the input promise

| The app will be… | Hand Tracking Support | What you must test |
|---|---|---|
| Controllers required (gun games, stick locomotion) | Controllers Only | Users without controllers get the **Switch to Controllers** dialog. Store reach on VR Glasses drops: users who only use hands see hand-tracked titles first |
| Controllers optional (recommended for most) | Controllers And Hands | Every required flow with eyes and hands, then with controllers, plus switching mid-session |
| Hands only | Hands Only | Starts and stays usable with no controllers active |

## Step 1: Scan (read-only)

```bash
python3 -I <this-skill>/scripts/vrglasses_readiness.py <project>            # Markdown report
python3 -I <this-skill>/scripts/vrglasses_readiness.py <project> --json     # for parsing
```

No Unity needed; it only reads files. It reports Unity/SDK versions (6000.0.66f2+, Core and ISDK
207+), IL2CPP/ARM64, Hand and Eye Tracking Support, Experimental Features and FoV-simulation flags,
a custom `AndroidManifest.xml`, Screen Space canvases, dynamic resolution, and C# that reads
controller buttons/axes/poses, `OVRGrabber`, XRI controllers, headset-model checks, hover handlers
and hard-coded refresh rates. Exit `1` = at least one High finding. `--include-samples` also scans
`Assets/Samples`. Tested on a Unity 6000.6.6f1 + Core 207.0.1 project and on a fixture.

Then, with the user, run Meta's **Device Readiness Check** (Core ≥ 207; Window > Meta > Tools >
Device Readiness Check > Get started > **Standard check**) and **Export as Markdown**. Its code
scan is similar to this script's; it also offers one-click setup fixes. For a deeper agent pass use
`hz-unity-device-readiness` with the frustum values below.

## Step 2: Project configuration

| Item | Value |
|---|---|
| Unity | 6000.0.66f2 or later (6.1+ recommended) |
| Packages | `com.meta.xr.sdk.core`, `com.meta.xr.sdk.interaction`, `com.meta.xr.sdk.interaction.ovr`, 207.0.1 on Meta's registry (2026-10-12). Upgrade with `hz-unity-sdk-upgrade` |
| Remove | `com.meta.xr.simulator` (deprecated; breaks compilation with Core 207), Oculus XR Plugin |
| Binary | IL2CPP + ARM64 only. A Mono (32-bit-only) build cannot run on VR Glasses |
| OVRManager > Quest Features > General | **Hand Tracking Support** (Step 0); **Eye Tracking Support** = Supported (keeps the ray fallback) or Required |
| Custom manifest | If `Assets/Plugins/Android/AndroidManifest.xml` exists, its hand-tracking entry wins. Use Window > Meta > Tools > Android Manifest Tool > **Update AndroidManifest.xml for Store Compatibility** |
| Rendering | Dynamic resolution on (OVRCameraRig > OVRManager > Enable Dynamic Resolution, set min/max scale) |

To read or set the two tracking settings from code, copy
[templates/VRGlassesSettings.cs](templates/VRGlassesSettings.cs) into `Assets/Editor/` (or
`AgentScripts/`). It compiles with 0 errors against Unity 6000.6.6f1 and Core 207.0.1 (Roslyn
check; not run in an Editor yet):

```bash
unity command run_script --project-path <project> --file AgentScripts/VRGlassesSettings.cs --entry VRGlassesSettings.Report --format json
# after the user agrees, and only once hands work:
unity command run_script --project-path <project> --file AgentScripts/VRGlassesSettings.cs --entry VRGlassesSettings.ApplyControllersAndHands --format json
```

Commit the `OculusProjectConfig.asset` diff, then rebuild and check the APK (Step 7).

## Step 3: Move every required flow to eyes and hands

| Controller input | Eyes-and-hands replacement |
|---|---|
| Controller ray | Eye gaze or hand ray |
| Trigger/grip press | Pinch, poke or palm grab |
| Trigger/grip hold | Pinch and hold |
| Trigger/grip drag | Pinch and drag, or palm grab |
| Thumbstick scroll | Microgesture swipe or pinch and drag |
| Thumbstick locomotion | Teleport (and physical turning) |

Interaction SDK 207 route (Meta's documented Unity path):
1. Right-click Hierarchy > Interaction SDK > **Add OVR Comprehensive Interaction Rig** (or the
   UnityXR rig; then add **Eye Gaze Interaction Profile** under OpenXR Enabled Interaction Profiles).
2. Right-click a Canvas > Interaction SDK > **Add Gaze Interaction to Canvas**; accept the
   `PointableCanvasModule` fix. Leave **Enable Ray Fallback** on so the UI still works when eye
   tracking is off, declined or on a Quest 3/3S. To reach controllers too, add **Controllers** in
   **Add Required Interactor(s)**.
3. Objects: **Add Gaze Grab Interaction** (two-hand rotate/scale via `GazeMultiGrab`). Sliders:
   `SliderGazePointerOffset`. Scroll lists: `MicrogestureScroll` / `DiscreteScrollInputProvider`.
4. Stack: `EyeGaze` → `GazeConecaster` → `GazeInteractor` → `GazeInteractable`. `EyeGaze.Active`
   is the per-frame validity; `EyeGaze.IsSyntheticPose` tells head-emulated gaze apart.

Rules:
- Size gaze targets ≥ 3.0°: world size = 2 × distance × tan(1.5°) (5.2 cm at 1 m). Prefer one row
  or one column of buttons; avoid dense 2D grids.
- Hover is decoration only. No required action or information may sit behind hover.
- Microgestures (thumb tap, swipes) fire once with no magnitude; use pinch and drag for continuous input.
- `OVRInput` already maps right/left index pinch to A/X for hand controller types, so A/X reads keep
  working. Triggers, grips, stick clicks, axes and B/Y do not.
- Face animation: there is no camera face tracking for apps; use Audio To Expressions (Movement SDK).
- Keep controller support as a fallback unless the app is Hands Only.

## Step 4: Field of view and UI

Nominal visible FoV is 70° × 66° (Quest 3: 110° × 96°); fit and gaze direction shrink it, and
Meta's perceived p90 target is 58° × 54°. Keep primary actions and status near the centre, give
edge-only actions another route, place viewer-fixed UI by angle, and never derive offsets from the
display resolution. The Core SDK's FoV mask leaves only 25° above centre but 41° below (code-observed),
so high HUD elements clip first.

Frustum test for head-locked UI (tangent half-angles left, right, up, down), for
`hz-unity-device-readiness`: Core mask `0.7002, 0.7002, 0.4663, 0.8693`; perceived, split evenly
(assumption) `0.5543, 0.5543, 0.5095, 0.5095`. Use the running app's resolved transforms, not YAML.

See it before hardware (pick one; ask before changing device or project settings):
- `metavr device fov-sim enable` (`--perceived` for 58° × 54°), then `disable`. Quest 3S/3/Pro,
  resets on reboot, look inside the headset (casts show the pre-lens image), compositor layers
  aren't clipped before Horizon OS v209.
- OVRManager > Quest Features > Experimental > **Experimental Features Enabled** + **Simulate Device
  Field of View** (Editor, Link, Simulator or a QA APK). Turn both off and rebuild before upload.
- Immersive Debugger > Custom Inspectors > **FoV Simulator** to tune the mask live.
- Grey-box levels: `quest-level-blockout` can render the VR Glasses eye frustum headless.

Test the first view after launch and after recentering, menus, dialogs, captions, errors, targets
near each edge, camera cuts/teleports and full-screen overlays.

## Step 5: Performance

- An app that holds its frame rate at nominal on Quest 3 should match it on VR Glasses. Design the
  steady state to **CPU 4 / GPU 4**; the OS raises clocks when it can.
- **GPU level 5, dual-core mode and CPU/GPU trading don't exist** on VR Glasses. Remove code that
  depends on them.
- CPU Boost (level 4 → 8): spend it on loading and scene changes only;
  `OVRManager.suggestedCpuPerfLevel = ProcessorPerformanceLevel.Boost;`, then `SustainedHigh`. It
  draws from a refilling budget (starts at 45 s), not Quest 3's fixed limits.
- Turn headroom into quality: dynamic resolution, and scale LOD, MSAA, shadows and post by measured load.
- FFR saves less here (larger GPU tiles). Prefer ETFR: `if (OVRManager.eyeTrackedFoveatedRenderingSupported) OVRManager.eyeTrackedFoveatedRenderingEnabled = true;`
  plus `OVRManager.foveatedRenderingLevel` and `useDynamicFoveatedRendering`. Vulkan and multiview
  only; it stays off if the user declines eye tracking.
- Geometry-heavy forward passes gain most; full-screen passes gain little.
- Measure a baseline with dynamic resolution off and fixed foveation, change one thing, re-run
  (`hz-perfetto-debug`, OVR Metrics). The VRC floor is still ≥ 60 rendered FPS.
- Refresh rates: pick from `OVRManager.display.displayFrequenciesAvailable`; don't hard-code.

## Step 6: Test in Meta XR Simulator (no device)

Needs the standalone simulator (207.1; `metavr tools install xrsim`, with consent) and Core 207.

1. Play mode with the simulator active. **Inputs > Device info > Device > Meta VR Glasses**, then
   stop and re-enter Play mode (the profile binds at OpenXR instance creation). CLI equivalent:
   `metavr xrsim device list`, then `metavr xrsim device set --name "<name from the list>" --wait`.
2. **Input settings > Left/Right input > Look and Pinch**. Mouse = gaze, left-click = pinch,
   Ctrl+mouse = move hand, Ctrl/click + scroll = depth, Space = reset hands, I/Y/H/G/J = thumb
   tap / swipe forward / back / left / right.
3. Start from the ISDK **GazeExamples** sample scene, then your own scenes.
4. Gaze ignored? The app must enable `XR_EXT_eye_gaze_interaction`. Microgestures ignored? It needs
   `XR_EXT_hand_interaction` and `XR_META_hand_tracking_microgestures`.
5. To repeat a pass without clicking, drive the session with `meta-xr-operator-unity`.

The simulator writes no Android manifest and is not Horizon OS: platform services, comfort and
the real gaze feel need a device.

## Step 7: Build and check the APK

Build with `unity-quest-android-build`, then (paths from that skill):

```bash
unzip -l "$APK" | awk '$4 ~ /^lib\// {split($4, p, "/"); print p[2]}' | sort -u     # expect only arm64-v8a
"$AAPT2" dump xmltree --file AndroidManifest.xml "$APK" | grep -E -A2 'handtracking|eye_tracking|HAND_TRACKING|EYE_TRACKING|experimental|supportedDevices'
unzip -l "$APK" | grep -i 'METAX_operator' && echo "Operator layer present: not a release build"
```

Expect: only `arm64-v8a`; the hand-tracking and eye-tracking entries matching Step 0/2; no
`com.oculus.experimental.enabled`; no Meta XR Operator layer. Report the `supportedDevices` value
as found (Core 207.0.1 writes the internal key `stanley` when its VR Glasses target is on; Unity
OpenXR has no VR Glasses entry). Don't "fix" it by hand.

## Step 8: Store targeting and services

- Developer Dashboard > the build's **Supported Devices**: tick **Future devices** plus each Quest
  you support. That is what delivers to VR Glasses; builds in a submission must include it anyway.
  An approved Production build with Future devices can reach VR Glasses at launch with no new APK.
- `com.meta.store.defaultDeviceTargets` (e.g. `quest3+`, which includes VR Glasses) only preselects
  targets at upload. Use it only for deliberate device splits.
- Compatibility mode: on VR Glasses, headset-type APIs such as `OVRPlugin.GetSystemHeadsetType()`
  can report an older Quest and refresh-rate lists shrink. Branch on capabilities.
- Same Store app = same entitlement, IAP and add-ons. Cross-device play needs compatible networking.
  Cloud Backup restores another device's latest backup **at install only** (100 MB, supported
  directories). Test it if saves must carry over; OVRProjectConfig **Disable Backups** writes
  `android:allowBackup="false"` (effect on Cloud Backup unverified).

## Report back

Input promise; scan counts by severity; settings changed (with diff); flows migrated and still on
controllers; FoV issues found and fixed; simulator results (profile, Look and Pinch, scenes
covered); APK check results; Dashboard actions left for the user; checks waiting for hardware
(Store-channel install, thermals with the puck, optics, Cloud Backup restore).

## Sources

- Support Meta VR Glasses (Unity): https://developers.meta.com/vr/documentation/unity/unity-support-meta-vr-glasses/ (2026-09-30)
- Bring your app / App compatibility / Test your app: https://developers.meta.com/vr/essentials/bring-your-app-glasses/ (2026-10-13), /vr/essentials/app-compatibility/ (2026-10-13), /vr/essentials/test-your-app/ (2026-09-25)
- Field of view: https://developers.meta.com/vr/essentials/field-of-view/ (2026-09-25); Unity FoV simulation: /vr/documentation/unity/in-headset-fov-simulation/ (2026-09-24)
- Compare devices: https://developers.meta.com/vr/essentials/compare-devices/ (2026-09-30)
- Device Readiness Check: https://developers.meta.com/vr/documentation/unity/device-readiness/ (2026-09-24)
- Gaze interactions, ISDK getting started: /vr/documentation/unity/unity-isdk-gaze-interaction/ (2026-09-09), /unity-isdk-getting-started/ (2026-09-14)
- Eyes best practices: https://developers.meta.com/vr/design/eyes-best-practices/ (2026-10-13)
- Optimize for VR Glasses: /vr/documentation/unity/optimize-performance/ (2026-09-19); CPU Boost: /po-quest-boost/ (2026-09-02); ETFR: /unity-eye-tracked-foveated-rendering/ (2026-10-07)
- Look and Pinch in XR Simulator: /vr/documentation/unity/xrsim-look-and-pinch/ (2026-09-03); simulator profile: /xrsim-getting-started/ (2026-09-03)
- Compatibility mode: /vr/essentials/compatibility-mode/ (2026-09-04); manifest: /vr/resources/publish-mobile-manifest/ (2026-10-15); device targeting: /vr/resources/publish-release-channels-device-targeting/ (2026-09-04)
- Platform services: /vr/essentials/platform-services/ (2026-08-25)
- Connect 2026 VR recap: https://developers.meta.com/blog/meta-connect-recap-start-building-the-future-of-vr/ (2026-09-24)
- Code read on 2026-10-17: Meta XR Core SDK 207.0.1 (`OVRProjectConfig.cs`, `OVRManifestPreprocessor.cs`, `OVRInput.cs`, `FovSimulator.cs`), Unity OpenXR 1.18.0 and 1.19.0-pre.2 (`MetaQuestFeature.cs`)

Last verified: 2026-10-17. Not verified: the final VR Glasses manifest id; the device's refresh-rate
list (spec says 120 Hz, simulator 90/120); Passthrough Camera Access and body tracking on VR Glasses;
whether Disable Backups blocks cross-device restore; anything on real hardware (none exists yet).
