---
name: quest-level-blockout
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.0.0"
description: >-
  Blocks out a Quest level from a data file and reviews it without a headset:
  every layout value in one JSON file, a validator that measures jump gaps,
  height steps and landing widths and refuses to build when a value breaks the
  design rules, an Editor generator that rebuilds the scene from the data (grid
  textured boxes with world-space UVs, separate gameplay collision, editor-only
  labels), and headless review images (viewpoint screenshots, a top-down map,
  and a Quest 3 per-eye stereo render). Includes three compile-tested C#
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
   entry for batch mode or `unity command run_script`:
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
   - Look at every image after every change, and fix what you see before showing the user.
6. **Iterate:** edit the JSON → regenerate → re-capture. Once the layout is approved, swap the visuals
   (art pass) without touching the collision or the data.

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

```bash
# headless, project closed
unity run <project> -- -executeMethod QuestBlockout.SceneCapture.Viewpoints -captureConfig Assets/Capture/capture.json
unity run <project> -- -executeMethod QuestBlockout.SceneCapture.Eyes
# or with the Editor open: Tools > Capture > Viewpoints / Quest 3 Eyes
```

`unity run` reserves `-batchmode`/`-quit` itself. The templates were run with a raw
`Unity -batchmode -quit -projectPath … -executeMethod …`; the `unity run` form is unverified.

## Sources

- Quest 3 per-eye resolution 2064 × 2208, budgets: https://developers.meta.com/horizon/resources/device-optimization-comparison/ (2026-05-11, via `docs/research-meta.md`)
- `Matrix4x4.Frustum`: https://docs.unity3d.com/ScriptReference/Matrix4x4.Frustum.html
- EditorOnly tag stripped from builds: https://docs.unity3d.com/Manual/Tags.html
- Comfort for VR locomotion: Meta `hz-immersive-designer`

Last verified: 2026-10-16
