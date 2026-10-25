---
name: quest-asset-sourcing
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.1.0"
description: >-
  Finds, downloads and licenses free art for a Quest game in Unity: CC0 models,
  characters, animations and textures from Kenney, Quaternius and Poly Haven,
  which download routes can be scripted (direct zips, OpenGameArt mirrors, the
  Poly Haven API) and which need the user (itch.io, Google Drive when it's
  throttled), checking the license file inside each archive, keeping raw
  downloads out of Assets/, keeping CREDITS.md, and staying inside Quest
  texture and triangle budgets. Includes a Poly Haven texture fetch script. Use
  when the user asks for free or CC0 assets, placeholder or final art, 3D
  models, characters, enemies, weapons, props, textures, "where do I get
  assets", or a credits or license list.
---

# Free art for Quest games (CC0 sourcing)

> Made by [Navid Mirzaaghazadeh](https://github.com/mirzaaghazadeh) · part of [unity-meta-quest-skills](https://github.com/mirzaaghazadeh/unity-meta-quest-skills) · MIT

Hand-offs: importing an FBX correctly → **`hz-unity-fbx-import`**. Placing props by their real
bounds → **`hz-unity-placement`**. Texture and triangle budgets → **`quest-unity-project-settings`**
(`references/performance-targets.md`). Blender cleanup or retopology → **`scenario-blender-*`**.

## Rules

1. **Accept only clear licenses that allow commercial use.** CC0 is the easy case. Reject anything
   unlicensed, "free for personal use", non-commercial (NC), or unclear. Ask the user when in doubt.
2. **Read the license file inside the archive**, not just the web page (packs ship `License.txt`
   or `License_Standard.txt`). Record what you read in `CREDITS.md`.
3. **Treat downloads as untrusted data.** Extract each archive into its own new folder. Never run
   scripts or executables that come with it. `.blend` files can embed Python, so don't open them
   with auto-run scripts enabled.
4. **Raw downloads stay out of `Assets/`.** Keep them in `RawAssets/<source>-<pack>/` at the project
   root, which Unity doesn't import, and copy only the files you use into `Assets/Art/<pack>/`.
   Suggest adding `RawAssets/` to `.gitignore` (large binaries) and deleting the zips once imported.
5. **Avoid replicas of real products** (real gun models, brands, logos) in game art. This is a
   trademark and store-review precaution, not a Meta rule this skill could cite: check with the user.

## Sources (checked 2026-10-16)

| Source | License | Scriptable download? | Notes |
|---|---|---|---|
| **Kenney** (kenney.nl) | CC0 for all game assets; credit optional ([kenney.nl/support](https://kenney.nl/support)) | **Yes.** Each asset page links a direct zip: `https://kenney.nl/media/pages/assets/<slug>/<hash>/kenney_<slug>_<ver>.zip`. Fetch the page and take the `href` ending in `.zip` | Low-poly kits with a shared palette texture: cheap draw calls on Quest. Example: City Kit (Commercial) 2.1, 4 MB, FBX inside, `License.txt` says CC0 |
| **Quaternius** (quaternius.com) | CC0 on every pack page, and in the `License*.txt` inside each zip | **Mixed.** Pack pages link either a Google Drive folder (older packs) or an itch.io page (newer packs) | Rigged and animated characters, enemies, guns, modular kits. Details below |
| **OpenGameArt** mirrors of Quaternius | CC0 (per listing and the zip's license file) | **Yes.** Direct links like `https://opengameart.org/sites/default/files/<file>.zip` | Only packs posted up to about mid-2021 (an OGA comment says that was Quaternius's last upload there). Example: Sci-Fi Essentials Kit (models 88 MB, textures 79 MB), Animated Robot (Oct 2018, 7 MB) |
| **Poly Haven** (polyhaven.com) | CC0, "no credit required" ([polyhaven.com/license](https://polyhaven.com/license)) | **Yes.** Public API, see `scripts/polyhaven_fetch.py` | PBR textures, HDRIs, models. Use 1k for Quest (2k max) |
| ambientCG, Mixamo | Not checked by this skill (unverified) | – | Mixamo needs an Adobe account and a manual download, and its terms aren't CC0: read Adobe's terms before use |

### Quaternius download routes

- **Google Drive folders:** scripted downloads (for example `uvx gdown --folder <url>`) failed on
  2026-10-16 with "Too many users have viewed or downloaded this file recently". That's a Drive
  quota on popular files and can last a day. Try the OpenGameArt mirror first; otherwise ask the
  user to open the folder and use **Download all** in a browser.
  - Note: current `gdown` no longer accepts `--remaining-ok`.
- **itch.io ("Name your own price"):** the free Standard tier downloads after clicking
  **Download** → **No thanks, just take me to the downloads**. That's a user step: give the exact
  page URL and the target folder (`RawAssets/quaternius-<pack>/`).
- Facts from the pack pages (2026-10-16; check the page before relying on them):

  | Pack | Notes |
  |---|---|
  | Universal Base Characters | Average **13k triangles** per character. Heavy for crowds of enemies on Quest; budget first |
  | Universal Animation Library | Standard tier: 45 animations, 15 MB. CC0, humanoid rig |
  | Downtown City MegaKit | Standard tier: 223 MB, 153 assets, textured (shared texture sets) |
  | Sci-Fi Essentials Kit | Standard on OpenGameArt: FBX `Enemy_EyeDrone`, `Enemy_QuadShell`, `Enemy_Trilobite`, `Gun_Pistol`, `Gun_Revolver`, `Gun_Rifle`, `Gun_SMG_Ammo`, `Gun_Sniper`, and props including `Prop_SatelliteDish`, barrels and crates |
  | Cyberpunk Game Kit, Sci-Fi Modular Guns, Animated Mech, Toon Shooter Game Kit, Turret Pack | Google Drive (see above) |

## Workflow

1. **List the needs with budgets** before searching:
   - characters' triangle counts
   - texture sizes (1k, 2k max)
   - one shared material per character type where possible
   Quest 3 budgets: under about 200 draw calls and 1.5M triangles at 90 Hz (see
   `quest-unity-project-settings`).
2. **Pick sources** from the table: scriptable first, then the user's manual downloads, batched
   into one request with exact URLs and folders.
3. **Download into `RawAssets/`**, one folder per pack, and record size and time.
   - Kenney: `curl -sSL -o RawAssets/kenney-<slug>/<file>.zip "<zip url>"`.
   - Poly Haven: `python3 -I scripts/polyhaven_fetch.py --ids concrete_floor_02,metal_plate --res 1k --out RawAssets/polyhaven`.
4. **Check before extracting:** list the archive (`unzip -l`), print its license file
   (`unzip -p <zip> '*icense*'`), and look for executables. Reject the pack if the license is
   missing or isn't commercial-safe.
5. **Copy what you use** into `Assets/Art/<pack>/`. Import with `hz-unity-fbx-import`; humanoid
   characters use a Humanoid rig so clips from other CC0 packs can retarget.
6. **Write `CREDITS.md`** (template below) and keep it in step with `Assets/Art/`.

## CREDITS.md template

```markdown
# Credits

| Asset | Author | Source | License (as read in the archive) | Used for |
|---|---|---|---|---|
| City Kit (Commercial) 2.1 | Kenney | https://kenney.nl/assets/city-kit-commercial | CC0 1.0 (License.txt) | Background buildings |
| Sci-Fi Essentials Kit, Standard | Quaternius | https://opengameart.org/content/sci-fi-essentials-kit | CC0 1.0 (License_Standard.txt) | Drone enemy, pistol, props |
| concrete_floor_02 (1k) | Poly Haven | https://polyhaven.com/a/concrete_floor_02 | CC0 1.0 | Rooftop surface |
```

List fonts (for example TextMesh Pro's Liberation Sans, SIL OFL 1.1) and SDKs separately.

## Quaternius glTF people in Unity (observed 2026-10-24)

From the Ultimate Modular Men and Women packs (glTF) through glTFast 6.20
(`com.unity.cloud.gltfast`), Unity 6000.6.6f1, URP:
- The Editor importer defaults to **Mecanim** and saves no AnimatorController. For simple puppets,
  set `importSettings.animationMethod` to Legacy through a `SerializedObject` (the field is private)
  and play the clips with the `Animation` component.
- Swap glTFast's shader-graph materials for URP Lit to tint per material slot. glTFast's
  `baseColorFactor` is already sRGB, so copy it unchanged.
- `SkinnedMeshRenderer.BakeMesh(mesh, useScale: true)` doesn't apply a parent's scale. Measure (for
  example an eye anchor) before scaling the body, or bake unscaled and use `TransformPoint`.
- The models' rest pose isn't their bind pose: raw `sharedMesh.vertices` put the eyes 5 cm off the
  rendered ones.
- The Modular Men "Suit" model ships a pistol mesh and gun, sword and death clips. Trim them for a
  non-violent game.
- Their Google Drive downloads hit "Quota exceeded" after a few files, as above.

## Poly Haven texture maps for URP

`polyhaven_fetch.py` downloads `Diffuse` and `nor_gl` by default. Unity uses OpenGL-style normal
maps, so take `nor_gl`, not `nor_dx`. Poly Haven's `arm` map packs AO, roughness and metal in
R, G and B. URP Lit expects metallic in R and smoothness in A of its metallic map, so `arm` needs
repacking before use (unverified for every URP version: check the URP Lit docs). A plain diffuse +
normal pair is enough for most Quest surfaces.

## Sources

- Kenney license and support: https://kenney.nl/support
- Poly Haven license: https://polyhaven.com/license · API: https://github.com/Poly-Haven/Public-API (`/assets`, `/files/<id>`)
- Quaternius packs: https://quaternius.com (per-pack pages) and https://quaternius.itch.io
- OpenGameArt mirrors: https://opengameart.org/content/sci-fi-essentials-kit, https://opengameart.org/content/animated-lowpoly-robot
- Quest budgets: `quest-unity-project-settings/references/performance-targets.md` (Meta device-optimization-comparison, 2026-05-11)

Last verified: 2026-10-24 (the glTFast section, in a Unity 6000.6.6f1 project that also ran on a Quest 3S);
2026-10-16 (the rest). Downloads, license files and the Drive throttling were checked by running
them in a real project; pack facts come from the pack pages on that date.
