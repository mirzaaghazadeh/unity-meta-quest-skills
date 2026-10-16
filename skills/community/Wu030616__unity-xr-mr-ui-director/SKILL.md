---
name: unity-xr-mr-ui-director
description: Designs, implements, and reviews Unity 3D spatial UI for Meta Quest 3 mixed-reality projects. Use for MR menus, world-space canvases, hand or controller interaction, room-aware placement, Figma-to-Unity UI work, accessibility, and Quest 3 on-device UI validation.
license: Apache-2.0
compatibility: Requires Python 3.10+ for the optional read-only audit. Unity, device, browser, and MCP capabilities are detected at runtime.
metadata:
  version: "0.1.0"
---

# Unity XR/MR UI Director

Build spatial interfaces as physical objects with measurable size, distance, interaction states, and frame cost. Target Meta Quest 3. Keep the core interaction layer OpenXR/XRI-compatible unless the project already commits to Meta Interaction SDK.

## Route supporting skills

Use supporting skills when their trigger applies:

- Use the bundled `quest3-verify-first` before current Quest package, API, entitlement, device, or policy claims.
- Use the bundled references in this skill for comfort, accessibility, Canvas, and MRUK work. Do not require an unpinned device plugin.
- Use a separately audited device bridge for logs, screenshots, and traces when one is installed.

Use Figma design/library skills for tokens and flat component composition when a Figma deliverable is requested. Do not use a flat-screen mockup as evidence that an MR interface is readable or reachable.

## 1. Resolve the real project

1. Locate `ProjectSettings/ProjectVersion.txt` and `Packages/manifest.json`.
2. Match the required editor version against the Unity editors actually installed.
3. Run the read-only audit:

```bash
python3 <skill-dir>/scripts/audit_unity_xr_ui.py "/absolute/path/to/unity-project"
```

4. Report missing packages, version mismatch, and input-module conflicts before scene edits.
5. Never silently upgrade Unity or package versions. Package changes can reserialize scenes and prefabs.

Read [Unity implementation](references/unity-implementation.md) before changing packages, scenes, prefabs, or Canvas interaction components.

## 2. Write the spatial UI brief

Define these fields before implementation:

- user task and success condition;
- standing, seated, or both;
- controller, hand ray, direct poke, gaze, voice, and fallback inputs;
- world-, room-, object-, body-, or head-relative placement;
- expected viewing distance and physical panel size in metres;
- bright, dark, cluttered, and moving-background conditions;
- persistence across room relocalization;
- destructive actions and recovery path;
- tracking-loss, permission-denial, loading, empty, and error states.

Reject a brief that specifies only pixels. Spatial UI needs metres and angular readability.

## 3. Choose one interaction stack

Default foundation:

```text
URP
+ Input System
+ XR Plug-in Management
+ OpenXR
+ XR Interaction Toolkit
+ XR Hands when hand tracking is required
+ uGUI World Space Canvas
+ TextMesh Pro
+ MRUK for Quest room understanding
```

Use Meta Interaction SDK only for a feature that XRI cannot provide adequately or when the existing project already uses it. Do not leave two active EventSystems, two UI input modules, or duplicate ray/poke interactors in one scene.

Start with uGUI World Space Canvas for the first working Quest build. Evaluate world-space UI Toolkit later if its interaction and focus path is proven on the project’s exact Unity/XRI versions.

## 4. Design the visual system

Create or document:

- semantic colour tokens, including passthrough-safe surfaces;
- type scale expressed in visual angle and tested physical distance;
- spacing and target-size tokens expressed in millimetres;
- depth/elevation tokens with a small number of layers;
- focus, aim, hover, pressed, activated, disabled, busy, error, and tracking-lost states;
- icon plus label treatment for unfamiliar or destructive actions;
- audio/haptic cues for critical confirmations;
- reduced-motion, seated, one-handed, subtitle, contrast, and text-size options.

Read [Quest 3 spatial UI standards](references/quest3-spatial-ui-standards.md) for starting values. Treat those values as testable baselines, not universal constants.

## 5. Implement through the available Unity bridge

Inspect the currently exposed Unity MCP tools before invoking anything. Meta's `metavr` MCP controls Quest devices; it does not, by itself, prove that Unity Editor scene-editing tools exist.

Preferred order:

1. Use the configured local Unity Editor MCP for scene, prefab, asset, console, test, and screenshot operations.
2. Map Meta examples that mention `Unity_RunCommand` to the bridge's actual C# execution tool. Never invent the command name.
3. Keep generated editor C# scoped, reviewable, and idempotent. Name temporary commands under `Assets/Editor/Codex/` and remove them only after confirming they are not user-owned.
4. If a separately audited and explicitly configured Quest device bridge exists, use it only for an action authorized by the current request. Before APK installation, launch, capture, or diagnostics, list connected devices and select an explicit serial when more than one exists.

For every Canvas:

- use World Space rendering;
- record its world size in metres and intended viewing distance;
- keep child `RectTransform.localScale` at one;
- size children with anchors and `sizeDelta`;
- disable raycast targets on decorative graphics and labels;
- separate static and frequently changing content when rebuild cost matters;
- attach only the input-module and raycaster family selected for the project;
- build explicit tracking-loss and unavailable states.

## 6. Place UI in mixed reality

Use MRUK room data only after scene permission and room loading succeed. Choose a policy for missing room data.

- Room-locked panels: bind to semantic wall, table, floor, or room anchors.
- Object UI: place relative to stable bounds, not an arbitrary transform origin.
- Body-follow panels: use delayed follow, dead zones, and an explicit recenter action.
- Head-locked content: reserve for brief safety or system-critical cues; minimize duration and visual area.
- Real surfaces: test normals, clearance, occlusion, reachability, and user height.

Do not assume a detected surface is usable. Doorways, reflective surfaces, clutter, and narrow wall fragments need rejection rules.

## 7. Validate in increasing-cost order

1. Static audit of packages, scenes, prefabs, and input modules.
2. Unity compilation and console check.
3. Edit-mode and play-mode tests where available.
4. Editor/XR Simulator interaction smoke test.
5. Quest 3 build and launch.
6. Controller-ray, hand-ray, and poke tests as applicable.
7. Bright, dark, cluttered, transparent, and low-contrast passthrough backgrounds.
8. Permission denial, room-data absence, relocalization, anchor drift, and tracking interruption.
9. Device frame timing and trace review. Do not approve performance from Game View alone.

Read [QA checklist](references/qa-checklist.md) before calling the UI complete.

## Output contract

Return:

- resolved Unity project and editor version;
- chosen interaction stack and why;
- spatial UI brief with physical measurements;
- changed assets/scripts/scenes;
- audit results and remaining warnings;
- device model, runtime conditions, and tests actually run;
- screenshots or trace paths when generated;
- a clear list of items still requiring human headset judgment.

Never claim headset readability, comfort, or reachability without a Quest 3 test.
