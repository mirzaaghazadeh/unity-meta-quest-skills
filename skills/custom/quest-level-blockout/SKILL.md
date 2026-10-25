---
name: quest-level-blockout
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.2.0"
description: >-
  Blocks out a Quest level from a data file and reviews it without a headset:
  every layout value in one JSON file, a validator that measures jump gaps,
  height steps and landing widths and refuses to build when a value breaks the
  design rules, an Editor generator that rebuilds the scene from the data (grid
  textured boxes with world-space UVs, separate gameplay collision, editor-only
  labels), and headless review images (viewpoint screenshots, a top-down map,
  and a Quest 3 or Meta VR Glasses per-eye stereo render). Includes three compile-tested C#
  templates. Use when the user asks to block out, grey-box or prototype a
  level, design a parkour, platformer or shooter route, check jump distances,
  regenerate a scene from data, or "show me what the player sees" without a
  headset.
---

# Level blockout from data, reviewed without a headset

> Made by [Navid Mirzaaghazadeh](https://github.com/mirzaaghazadeh) · part of [unity-meta-quest-skills](https://github.com/mirzaaghazadeh/unity-meta-quest-skills) · MIT

Hand-offs:
- Comfort for the movement you're designing (jumps, drops, speed): **`hz-immersive-designer`**,
  **`xr-platforms-audio-and-comfort`**.
- Draw-call and triangle budgets: **`quest-unity-project-settings`**.
- Art for the art pass: **`quest-asset-sourcing`**.
- The true runtime view: **`hz-xr-simulator-control`** (simulator) or **`hz-vr-debug`** (device
  screenshots).
- Running these scripts headless: **`unity-quest-dev-setup/references/batch-mode-traps.md`**.

## Why data-driven

- **One file holds every layout number.** Changing a gap is a one-line edit, and the scene
  regenerates in seconds. In the reference project, 8 rooftops, 3 walls, 3 slide obstacles,
  4 ledges, 9 checkpoints, 40 enemy slots and a 200-building background took about 24 s in batch
  mode on an Apple Silicon Mac.
- **The validator turns design rules into checks.** It catches a broken jump before anyone plays it.
- **Gameplay collision comes from the same data.** Collision is separate boxes on a `Gameplay`
  layer, and visual meshes have no colliders, so later art can't change a jump distance or a
  walkable surface.

## Steps

1. **Write the rules first,** as ranges in the data file. They are the game's own design choices,
   not Meta limits. The parkour reference used:
   - gaps 2–5 m, height steps 1–4 m, landing width ≥ 2 m
   - wall-run walls 10–15 m long and 4–5 m high
   - ledges 1.2–1.6 m (chest height), slide clearance about 1 m
2. **Describe the layout** in the same JSON: platforms as `x: [min, max]`, `z: [min, max]`,
   `top`, plus walls, obstacles, checkpoints, spawn slots and camera viewpoints. A complete example
   is in [references/example-route.json](references/example-route.json).
3. **Validate.** Feed the route to `RouteValidator.CheckRoute` (template). It returns a report
   line per jump (`R2 -> R3: gap 5.0 m, height +2.0 m, landing width 14.0 m`) plus errors. Add
   checks for your other elements the same way: walls' length and height, ledges' rise above the
   roof they sit on, enemies standing on a walkable surface, counts. **Throw on any error** so a
   bad file never produces a scene.
4. **Generate** with an Editor script that has a menu item (`Tools > Build Test Scene`) and a static
   entry for batch mode or `unity command script run` (`run_script` in Pipeline 0.8):
   - Start from `EditorSceneManager.NewScene(NewSceneSetup.EmptyScene)`, and delete and recreate a
     `Generated/` folder for materials and meshes.
   - Visual boxes from `BlockoutMesh.Box(name, center, size, 0.25f)` with a grid material from
     `BlockoutMesh.GridTexture(...)`: one grid square is 1 m, so screenshots show distances.
     Two submeshes give roof and facade colors.
   - Collision: a `BoxCollider` per element under a `Collision` root, on a `Gameplay` layer
     (create it through `ProjectSettings/TagManager.asset`).
   - Labels (`R2 h22`, `LD1 +1.4`, `C3`) as TextMesh Pro under a root tagged **`EditorOnly`**. Unity
     strips it from builds. Parent each 3D text under a plain `Transform` (see the traps file).
   - Mark static geometry `BatchingStatic`. Save generated meshes into one asset
     (`CreateAsset`, then `AddObjectToAsset`).
5. **Review** with `SceneCapture` (template) and a `capture.json`
   ([references/capture.json](references/capture.json)):
   - **Viewpoints:**
     - eye-level shots at 1.6–1.7 m above the floor
     - an aerial overview and a top-down orthographic map (`orthoSize`), both with `noFog: true`
     - `hideEditorOnly: true` for clean shots
   - **Quest 3 eyes:** `SceneCapture.Eyes` renders one image per eye, side by side, as the headset
     would draw them. That's 2064 × 2208 per eye (Meta's device-optimization-comparison), with an
     asymmetric frustum per eye, the eyes an IPD apart, and editor-only objects hidden. The default
     angles and IPD are approximations (**unverified**). Not reproduced: lens blur, foveated
     rendering, the device GPU.
   - **Meta VR Glasses eyes:** override the same `eyes` block (no code change). The render extent
     from Meta XR Simulator's VR Glasses profile is 37° temporal, 37° nasal, 34° up and down, at
     1728 × 1760 per eye (render-scale page; the profile itself reports 1680 × 1760):
     `"fovTowardsNose": 37, "fovAwayFromNose": 37, "fovUp": 34, "fovDown": 34, "width": 1728, "height": 1760`.
     To approximate what a person actually sees (70° × 66°), use the Core SDK FoV mask's default
     angles instead: `"fovTowardsNose": 35, "fovAwayFromNose": 35, "fovUp": 25, "fovDown": 41`
     (read from Core 207.0.1 source, not a docs page). Compare it with the Quest 3 render to find
     landmarks, signs or HUD that fall outside the narrower view. Details: `meta-vr-glasses-unity`.
   - Look at every image after every change, and fix what you see before showing the user.
6. **Iterate:** edit the JSON → regenerate → re-capture. Once the layout is approved, swap the visuals
   (art pass) without touching the collision or the data.

## The real headset view on a Mac: Meta XR Simulator from the CLI

Once the scene has a rig (Meta's `OVRCameraRig`; set `OVRManager` `_trackingOriginType` to
FloorLevel), this ran on 2026-10-17 with Core SDK 207.0.1, OpenXR 1.18.0, simulator 207.1
(`metavr tools install xrsim`), Unity 6000.6.6f1 on Apple Silicon:

```bash
unity command menu --project-path . -- --path "Window/Meta/Meta XR Simulator/Activate"   # sets XR_SELECTED_RUNTIME_JSON
unity command editor_play --project-path .
unity command screenshot --project-path . -- --view game --output "$PWD/Screenshots/sim.png" --width 1920 --height 1080
unity command editor_stop --project-path .
```

With **Pipeline 0.9** (0.9.1-exp.1, observed 2026-10-24 and 25 in a Unity 6000.6.6f1 project):

```bash
unity command editor invoke_menu --project-path . -- --path "Window/Meta/Meta XR Simulator/Activate"
unity command editor.playmode set_run_in_background --project-path . -- --enable true   # first, or the session sticks
unity command editor.playmode start --project-path .
unity command capture game_view --project-path . -- --camera CenterEyeAnchor --source camera
unity command editor.playmode stop --project-path .
```

- **Run in background first.** Without it an unfocused Editor doesn't tick: the OpenXR session
  stays at READY with the head at (0, 0, 0), `display.running` false and focus false, and nothing
  is logged.
- In XR the plain Game view capture is blank; capture the eye camera (`--camera CenterEyeAnchor
  --source camera`). `capture game_view` returns the PNG inline as base64 in its JSON (decode it
  to a file), and any `save_path` lands under `Assets/` regardless.
- After a test run the simulator can be deactivated again: re-run the Activate menu before
  Play-mode captures.
- The simulator can report `InputFocusLost` at start; a game that pauses on lost focus (as the
  VRCs require) needs a debug hook to force focus for unattended captures.

- **The session should reach focused:** the console shows
  `[OVRPlugin] [XR_SESSION] ... running 1 visible 1 focused 1` and `SystemHeadset
  Meta_Link_Quest_3`. The `ErrorFunctionUnsupported … xrGetSpaceRoomMeshMETA` style warnings are
  Mac-simulator gaps, not project errors.
- **The Game view** then mirrors the eye camera of the live OpenXR session.
- **In the simulator window:** hold the right mouse button to look, WASD to move, E/Q for up and
  down (see `hz-xr-simulator-install-and-configure`).
- **`EditorOnly` objects are stripped from builds but stay in Play mode.** Hide debug labels and
  markers with a component that calls `gameObject.SetActive(false)` in `Awake`, so the simulator
  view matches the game.

## Review traps found in a real project

- **Unity's procedural skybox** looked green-yellow at dusk. A small gradient-sky shader with
  stereo macros read correctly.
- **Labels for far-away objects** pile up in eye-level shots. Show labels only 3–40 m from the
  camera in those, and only route labels in overview and map shots. Turn each label to face the
  camera and scale it with distance.
- **Fog washes out aerial and map shots,** whose cameras sit beyond the fog's end distance.
- **A placeholder enemy** sat 2.5 m in front of one viewpoint. Check each viewpoint image, not just
  the overview.
- **Box faces:** with hand-built meshes, Unity's front faces wind clockwise. `BlockoutMesh.Box` is
  tested so every face's normal points outward.

## Templates

| File | What it does | Tested |
|---|---|---|
| [templates/BlockoutMesh.cs](templates/BlockoutMesh.cs) | `Box` (world-space UVs, roof/facade submeshes), `Torus` (rings), `GridTexture` (4 × 4 m grid, 1 m lines) | Faces point outward; grid imports with Repeat wrap |
| [templates/RouteValidator.cs](templates/RouteValidator.cs) | Gap, height step and landing width for platforms in route order | Matched the reference project's measured table exactly; a moved platform (7 m gap) gave 2 errors |
| [templates/SceneCapture.cs](templates/SceneCapture.cs) | `Viewpoints()` and `Eyes()` from `capture.json` (or `-captureConfig <path>`), menu Tools > Capture | Both ran in batch mode and produced correct images |

All three use the `QuestBlockout` namespace and go in `Assets/Editor/`. They were compiled and run
on 2026-10-16 in a Unity 6000.6.6f1 URP project with Meta XR Core SDK 207.0.1 on macOS.

**Prefer the live Editor.** With `com.unity.pipeline` installed and the project open
(`unity open .`, `unity status --until-ready`), run the menus through the CLI. In the reference
project, regenerating the scene took 1–2 s this way versus about 24 s for a batch launch, and the
per-eye render under 1 s versus about 20 s:

```bash
unity command menu --project-path . -- --path "Tools/Build Test Scene"    # your generator's menu item
unity command menu --project-path . -- --path "Tools/Capture/Quest 3 Eyes"
unity command console --project-path . --format json -- --since <cursor>   # cursor from console_status first
# Pipeline 0.9: editor invoke_menu, console get (--level, --tail), console status
```

- **After editing a C# file,** run `unity command recompile`, then poll `recompile_status` until it
  reports `completed`. A `menu` call sent during the domain reload can fail with "Connection reset
  by server": run `unity status --until-ready` and retry.
- **Both capture methods reload the scene from disk when they finish,** so a live Editor isn't left
  with hidden or moved objects.

Headless, with the project closed:

```bash
unity run <project> -- -executeMethod QuestBlockout.SceneCapture.Viewpoints -captureConfig Assets/Capture/capture.json
unity run <project> -- -executeMethod QuestBlockout.SceneCapture.Eyes
```

`unity run` reserves `-batchmode`/`-quit` itself. The templates were run with a raw
`Unity -batchmode -quit -projectPath … -executeMethod …`; the `unity run` form is unverified.

## Sources

- Quest 3 per-eye resolution 2064 × 2208, budgets: https://developers.meta.com/horizon/resources/device-optimization-comparison/ (2026-05-11, via `docs/research-meta.md`)
- `Matrix4x4.Frustum`: https://docs.unity3d.com/ScriptReference/Matrix4x4.Frustum.html
- EditorOnly tag stripped from builds: https://docs.unity3d.com/Manual/Tags.html
- Comfort for VR locomotion: Meta `hz-immersive-designer`
- Meta VR Glasses simulator profile (37/37/34/34°): https://developers.meta.com/vr/documentation/unity/xrsim-getting-started/ (2026-09-03); render target 1728 × 1760: https://developers.meta.com/vr/essentials/render-scale/ (2026-09-01); 70° × 66° FoV: https://developers.meta.com/vr/essentials/field-of-view/ (2026-09-25)

Last verified: 2026-10-25 (the Pipeline 0.9 simulator loop); 2026-10-17 (VR Glasses eyes, not
rendered yet); 2026-10-16 (the rest)
