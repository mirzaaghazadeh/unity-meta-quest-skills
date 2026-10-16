# Meta VR Glasses: facts a Unity agent needs (checked 2026-10-17)

Our summary of Meta's pages; dates are each page's own last-updated date. **Code** = read from
Meta XR Core SDK 207.0.1 / Unity OpenXR 1.18.0 source, not on a docs page. The repository's
`docs/meta-reference/unity-vr-glasses.md` has the full table with every source.

## Device (compare-devices 2026-09-30; field-of-view 2026-09-25; render-scale 2026-09-01)

| | Meta VR Glasses | Quest 3 | Quest 3S |
|---|---|---|---|
| Form factor | Glasses + tethered compute puck, ~100 g, ships spring 2027 | Headset | Headset |
| Chip / RAM | XR2 Gen 3 / 12 GB | XR2 Gen 2 / 8 GB | XR2 Gen 2 / 8 GB |
| Per-eye panel | 2412 × 2288 micro-OLED, 37 PPD | 2064 × 2208, 25 PPD | 1832 × 1920, 20 PPD |
| Render target at scale 1.0 | 1728 × 1760 (sim profile says 1680 × 1760) | 1680 × 1760 | 1680 × 1760 |
| FoV | **70° × 66°** nominal | 110° × 96° | 96° × 90° |
| Refresh | 120 Hz (sim profile: 90/120, default 90) | 72–120 Hz | 72–120 Hz |
| Input | Eyes + hands; Touch Plus sold separately | Controllers + hands | Controllers + hands |
| Eye tracking | Yes | No | No |
| Face tracking for apps | Audio To Expressions only | Audio To Expressions | Audio To Expressions |
| PSS memory limit | 5.75 GiB | 5.75 GiB | 5.75 GiB |

## Field of view

| Value | Angles | Notes |
|---|---|---|
| Nominal visible | 70° H × 66° V | Upper bound; fit and gaze shrink it (field-of-view) |
| Perceived p90 | 58° H × 54° V | `metavr device fov-sim enable --perceived` |
| Render extent | 74° × 68°: 37° temporal, 37° nasal, 34° up, 34° down | XR Simulator VR Glasses profile (xrsim-getting-started 2026-09-03). Not a UI boundary |
| Core FoV mask default | inner 35°, outer 35°, up 25°, down 41° | **Code** (FovSimulator.cs): the visible window sits low, so a HUD placed high clips first |

Tangent half-angles (left, right, up, down) for a corner-in-frustum test:
render extent `0.7536, 0.7536, 0.6745, 0.6745`; Core mask `0.7002, 0.7002, 0.4663, 0.8693`;
nominal split evenly (assumption) `0.7002, 0.7002, 0.6494, 0.6494`; perceived split evenly
(assumption) `0.5543, 0.5543, 0.5095, 0.5095`.

Gaze targets ≥ 3.0° (eyes-best-practices 2026-10-13): world size = 2 × d × tan(1.5°):
3.1 cm at 0.6 m, 5.2 cm at 1 m, 7.9 cm at 1.5 m, 10.5 cm at 2 m.

## Input (unity-support-meta-vr-glasses 2026-09-30; app-compatibility 2026-10-13; bring-your-app-glasses 2026-10-13)

- Eyes target, pinch selects. Assume no controllers. A controller-required immersive app installs
  but the **Switch to Controllers** dialog blocks entry until controllers are active.
- Store: users without controllers see hand-tracked titles first (Connect VR recap 2026-09-24).
- Manifest from **Hand Tracking Support**: Controllers Only → no entry; Controllers And Hands →
  `oculus.software.handtracking` required=false + `com.oculus.permission.HAND_TRACKING`; Hands
  Only → required=true. **Eye Tracking Support** Supported/Required → `oculus.software.eye_tracking`
  required=false/true + `com.oculus.permission.EYE_TRACKING`.
- A custom `Assets/Plugins/Android/AndroidManifest.xml` keeps its existing hand-tracking entry.
  Fix with Window > Meta > Tools > Android Manifest Tool > Update AndroidManifest.xml for Store
  Compatibility.
- **Code** (OVRInput.cs): right/left index pinch sets `RawButton.A`/`X` for hand controller types
  unless `OVR_DISABLE_HAND_PINCH_BUTTON_MAPPING` is defined. Triggers, grips, sticks, B/Y have no
  hand mapping.
- No user flow may depend on hover (eyes-best-practices; Connect recap says hover never fires).

## Performance (optimize-performance 2026-09-19; cpu-gpu-levels 2026-09-11; po-quest-boost 2026-09-02)

- Design to CPU 4 / GPU 4 (nominal). OS raises clocks when there's headroom.
- No GPU level 5, no dual-core mode, no CPU/GPU trading.
- CPU Boost: level 4 → 8 (~30%), token-bucket budget that starts at 45 s and refills.
  `OVRManager.suggestedCpuPerfLevel = ProcessorPerformanceLevel.Boost;` then `SustainedHigh`.
- Use dynamic resolution (`OVRManager.instance.enableDynamicResolution`, min/max scale on the rig).
- FFR saves less than on Quest 3; ETFR (`OVRManager.eyeTrackedFoveatedRenderingSupported` →
  `eyeTrackedFoveatedRenderingEnabled = true`) needs Vulkan, multiview and the eye-tracking permission.
- Profile with dynamic resolution off and foveation fixed, then turn them on (test-your-app 2026-09-25).

## Manifest ids and Store targeting (sources disagree)

| Source | Says |
|---|---|
| publish-mobile-manifest (2026-10-15) | Canonical id `vrglasses` |
| compatibility-mode (2026-09-04) | Not a supported public value yet; use Store **Future devices** + ≥ 1 Quest; third-party immersive apps run in compatibility mode on VR Glasses |
| Core SDK 207.0.1 (**Code**) | `OVRProjectConfig.DeviceType.VRGlasses` writes the internal key `stanley` |
| Unity OpenXR 1.18.0 / 1.19.0-pre.2 (**Code**) | Meta Quest target list has no VR Glasses entry |
| device-targeting (2026-09-04) | "All Devices" includes VR Glasses; `quest3+` covers VR Glasses and future devices; submission builds must include Future devices |

Compatibility mode: headset-type APIs (e.g. `OVRPlugin.GetSystemHeadsetType()`) can report an older
Quest; refresh-rate lists shrink to rates both models support. Check capabilities, not the model.

## Testing without hardware

- Meta XR Simulator 207.1 standalone: **Device** > Meta VR Glasses, restart Play mode. Look and
  Pinch (v207+): mouse = gaze, left-click = pinch, Ctrl+mouse = move hand, scroll = depth,
  Space = reset hands, I/Y/H/G/J = thumb tap / swipe forward / back / left / right.
  Gaze needs `XR_EXT_eye_gaze_interaction`; microgestures need `XR_EXT_hand_interaction` +
  `XR_META_hand_tracking_microgestures` (xrsim-look-and-pinch 2026-09-03).
- ISDK samples: `GazeExamples` scene.
- FoV: `metavr device fov-sim enable [--perceived]` on Quest 3S/3/Pro (resets on reboot; compositor
  layers unclipped before Horizon OS v209); or OVRManager > Quest Features > Experimental >
  **Simulate Device Field of View** (needs Experimental Features Enabled: turn off before upload);
  or Immersive Debugger **FoV Simulator** (in-headset-fov-simulation 2026-09-24).
- Defer to hardware: Store-channel install, thermals with puck placement, compositor/optics,
  cross-device Cloud Backup.
