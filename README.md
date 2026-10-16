# Unity × Meta Quest skill library

[![License: MIT](https://img.shields.io/badge/license-MIT%20%2B%20third--party-blue.svg)](THIRD_PARTY_NOTICES.md)
![Skills: 82](https://img.shields.io/badge/skills-82-brightgreen.svg)
![Claude Code plugin](https://img.shields.io/badge/Claude%20Code-plugin%20marketplace-orange.svg)

Agent skills for building **Meta Quest VR games in Unity** with Claude Code and the **official Unity
CLI**, getting them ready for **Meta VR Glasses**, and shipping them on the **Meta Horizon Store**.
Meta's and Unity's official skills, original skills that fill the publishing gaps, and vetted
community skills, all checked against the live docs (updated after Meta Connect 2026, on
2026-10-17).

Curated and written by **[Navid Mirzaaghazadeh](https://github.com/mirzaaghazadeh)**. Sister
library for WebXR with Meta's Immersive Web SDK:
[iwsdk-webxr-skills](https://github.com/mirzaaghazadeh/iwsdk-webxr-skills).

| Folder | Skills | From | License |
|---|---|---|---|
| [`skills/custom/`](skills/custom) | 13 | **Original, by Navid Mirzaaghazadeh**: workflow router, Quest setup and settings, Meta VR Glasses readiness, Meta XR Operator, free art sourcing, level blockout and headless capture, builds, tests, VRC audit, store listing, upload, launch plan | MIT |
| [`skills/meta-official/`](skills/meta-official) | 26 | Meta ([meta-quest/agentic-tools](https://github.com/meta-quest/agentic-tools)), unchanged, plus [ERRATA](skills/meta-official/ERRATA.md) | Apache-2.0 |
| [`skills/unity-official/`](skills/unity-official) | 20 | Unity ([Unity-Technologies/skills](https://github.com/Unity-Technologies/skills)), unchanged | Unity Companion |
| [`skills/community/`](skills/community) | 23 | Vetted third-party skills, unchanged, each with a `SOURCE.md` | MIT / Apache / Unlicense |

---

## Quick start

**1. Install the core bundle** (in a Claude Code session, v2.1.275 or later):

```
/plugin install quest-core --marketplace mirzaaghazadeh/unity-meta-quest-skills
```

On older versions: `/plugin marketplace add mirzaaghazadeh/unity-meta-quest-skills`, then
`/plugin install quest-core@unity-meta-quest-skills`.

**2. Open your Unity project folder in Claude Code and ask:**

> Use quest-dev-workflow to plan my Quest game. It's a [one-line pitch]. Check my machine first.

**3. Follow the steps it gives.** It checks your tools (Unity CLI, Unity 6 with Android modules,
`metavr`) and asks before installing anything.

Other ways to install (`npx skills`, `gh skill`, `install.sh`, by hand), updating, and
troubleshooting: **[docs/install.md](docs/install.md)**.

```bash
npx skills add mirzaaghazadeh/unity-meta-quest-skills --list          # browse, pick with --skill
gh skill install mirzaaghazadeh/unity-meta-quest-skills               # GitHub CLI
git clone https://github.com/mirzaaghazadeh/unity-meta-quest-skills && cd unity-meta-quest-skills
./install.sh --project ~/Games/MyQuestGame quest-core                 # from a clone
```

## Bundles

The five bundles don't overlap. Start with `quest-core` and add others when a phase needs them.

| Bundle | Skills | Context per session | What's in it |
|---|---|---|---|
| **`quest-core`** | 27 | ~4.2k tokens | Router, verify-first guardrail, Unity CLI + live Editor, metavr, setup, settings, Meta VR Glasses readiness (+ Meta's Device Readiness Check), Meta XR Core + Interaction SDKs, CC0 art sourcing, level blockout + headless capture, VR UI, comfort, code review, tests, headset debugging, Perfetto, release build, VRCs, store listing, upload, launch plan |
| `meta-quest-extras` | 15 | ~1.8k | MRUK, passthrough camera, face and body tracking, placement, FBX, TMP, Meta XR Operator (agent control of a running app), SDK upgrades, simpleperf, XR Simulator, Meta's submission walkthrough |
| `unity-extras` | 17 | ~1.0k | uGUI and UI Toolkit, audio, physics, NavMesh, URP, Shader Graph, Project Auditor, multiplayer, Vivox, localization |
| `community-unity-xr` | 11 | ~1.2k | XR Interaction Toolkit, Netcode, Unity 6.3 agent protocol with mobile, performance, shader and CI specialists, test design, MR UI |
| `community-design-art` | 12 | ~1.3k | XR design reference, GDD, prototyping, playtesting, trailer review, Blender asset pipeline |

```
/plugin install unity-extras@unity-meta-quest-skills
/plugin install meta-quest-extras --marketplace mirzaaghazadeh/unity-meta-quest-skills
/plugin install community-unity-xr --marketplace mirzaaghazadeh/unity-meta-quest-skills
/plugin install community-design-art --marketplace mirzaaghazadeh/unity-meta-quest-skills
```

## How to use it

Describe what you want, and Claude picks the skill. The path from idea to store:

| Phase | Say something like | Main skills |
|---|---|---|
| Plan | "Write a one-page GDD and a 1-hour prototype plan" | `game-gdd-author`, `prototype-fast`, `horizon-launch-roadmap` |
| Set up | "Check my Mac can build for Quest, then create the project" | `unity-quest-dev-setup`, `hz-new-project-creation`, `quest-unity-project-settings` |
| Block out | "Block out the level from a data file, check the jump distances and show me what the player sees" | `quest-level-blockout`, `quest-asset-sourcing` |
| Build | "Add hand grabbing, teleport and a world-space menu" | `hz-unity-meta-interaction-sdk`, `hz-unity-meta-quest-ui`, `hz-unity-meta-core-sdk` |
| Test | "Run the tests, install a dev build on my Quest, show the errors" | `unity-test-runner-cli`, `unity-quest-android-build`, `hz-vr-debug` |
| VR Glasses | "Make my game work on Meta VR Glasses without controllers and check it in the simulator" | `meta-vr-glasses-unity`, `hz-unity-device-readiness` |
| Optimise | "It stutters here. Trace it on the headset" | `hz-perfetto-debug`, `hz-unity-code-review` |
| Ship | "Run the VRC audit, check my store art, upload to ALPHA" | `horizon-vrc-checklist`, `horizon-store-listing-assets`, `horizon-store-publish-unity` |

Claude asks before any store upload, submission or install. Some steps stay yours: the Developer
Dashboard, the final **Submit for Review**, and backing up your keystore. Full walkthrough with
prompts for every phase: **[docs/usage.md](docs/usage.md)**.

**AI agents**: read **[AGENTS.md](AGENTS.md)** for install rules, safety rules and repo conventions.

### Tools the skills use

| Tool | What it is | Install |
|---|---|---|
| **`unity`**, the Unity CLI (beta, July 2026) | Editors, modules, projects, builds, tests, and live control of the open Editor through `com.unity.pipeline` | `curl -fsSL https://unity.com/install.sh \| bash` or `brew install --cask unity-cli`; then `unity pipeline install` in each project |
| **`metavr`**, the Meta VR CLI (formerly `hzdb`) | Quest device control, logs, screenshots, Perfetto traces, Meta docs search, XR Simulator, store upload | `curl -fsSL https://developers.meta.com/horizon/install-cli/ \| sh`, or `npx -y metavr` |

Meta's Unity skills run their C# through Unity's **`unity-cli`** skill, so it's part of `quest-core`.
MCP setup is optional: see [mcp/README.md](mcp/README.md).

---

## Full catalog

### 1. Guardrails and tooling

| Skill | Source | Use it for |
|---|---|---|
| `quest-dev-workflow` | **original** | **Start here.** Routes every Quest task to the right skill; the end-to-end order and the hard rules |
| `hz-quest-verify-first` | Meta | Makes Claude check Quest claims against Meta's live docs (`metavr docs`) instead of memory |
| `metavr-cli` | Meta | The complete `metavr` reference: device, app, logs, capture, perf, docs search, MCP |
| `unity-cli` | Unity | The `unity` CLI: Editors, modules, projects, build, test, `command`/`eval`, MCP, Safe Mode recovery |
| `unity-pipeline` | Unity | Live Editor control: `run_script`, `eval`, `recompile`, `run_tests`, `audit`, autotick |
| `unity-package-management` | Unity | Add or remove UPM packages headlessly (the Meta XR SDKs are UPM packages) |
| `generate-editor-search-query` | Unity | Unity Search queries to find assets and objects |

### 2. Machine and project setup

| Skill | Source | Use it for |
|---|---|---|
| `unity-quest-dev-setup` | **original** | CLI check, Unity 6 Editor with the Android modules, license, `unity pipeline install`, switch to Android / Meta Quest |
| `hz-new-project-creation` | Meta | Scaffold a new Meta VR Unity project |
| `quest-unity-project-settings` | **original** | Every recommended Quest setting, each sourced to a Meta page, with an audit/apply script; target devices including Meta VR Glasses |
| `meta-vr-glasses-unity` | **original** | Make a Unity Quest app ready for Meta VR Glasses: eyes + pinch input, 70° × 66° view, controller-free flows, performance levels, manifest, testing in the v207 simulator; with a read-only readiness scanner |
| `hz-unity-device-readiness` | Meta | Meta's Device Readiness Check: controller dependencies, hand-tracking migration, head-locked UI the narrower view would clip |
| `hz-unity-meta-core-sdk` | Meta | OVRManager, OVRCameraRig, passthrough, hand tracking, anchors, boundaryless, Scene API, Android manifest |
| `hz-unity-sdk-upgrade`, `hz-api-upgrade` | Meta | Upgrade Meta XR SDK versions and replace deprecated APIs |
| `hz-unity-project-analyzer` | Meta | Build and maintain a `.agent-docs/` knowledge base of your project |
| `migrate-birp-to-urp` | Unity | Move an older Built-in pipeline project to URP (recommended for Quest) |

### 3. Gameplay, interaction and mixed reality

| Skill | Source | Use it for |
|---|---|---|
| `hz-unity-meta-interaction-sdk` | Meta | Grab, poke, ray, locomotion with Meta's Interaction SDK (recommended on Quest) |
| `hz-unity-meta-mixed-reality-utility-kit` | Meta | MRUK: rooms, walls, furniture, placing content in the real world, scene NavMesh, room sharing |
| `hz-unity-passthrough-camera-access` | Meta | Quest 3/3S RGB camera access for computer vision and ML |
| `hz-unity-face-tracking`, `hz-unity-meta-movement-sdk-retargeting` | Meta | Face tracking on ARKit-blendshape heads; body tracking retargeting |
| `hz-unity-placement` | Meta | Correct object placement using Renderer and Collider bounds |
| `hz-unity-fbx-import` | Meta | Import external FBX models correctly |
| `physics-3d-collision` | Unity | Collisions and triggers (grabbing, throwing, hits) |
| `initialize-ai-navigation` | Unity | NavMesh for NPCs |
| `unity-xr` | community (Nice-Wolf-Studio) | XR Interaction Toolkit + OpenXR route. **Pick one stack**: Meta ISDK *or* XRI |

### 4. UI, text, audio, rendering

| Skill | Source | Use it for |
|---|---|---|
| `hz-unity-meta-quest-ui` | Meta | World-space canvases, sizing and viewing distance for VR |
| `hz-unity-tmp-resources`, `optimize-text-mesh-pro` | Meta, Unity | Import TextMesh Pro resources and fix pink text; optimise font atlases |
| `ui`, `ui-ugui`, `ui-uitk` | Unity | UI routing, uGUI, UI Toolkit |
| `unity-xr-mr-ui-director` (+ `quest3-verify-first`) | community (Wu030616) | Quest 3 mixed-reality UI design and review |
| `audio-setup-mixers`, `optimize-audio` | Unity | Mixers; audio memory and CPU cost |
| `urp-postprocessing`, `validate-urp-render-graph-renderer-feature`, `shader-graph-create-custom-node` | Unity | URP volumes (keep them light on Quest), Render Graph features, Shader Graph nodes |
| `scenario-unity-shaders` | community (scenario-labs) | URP shaders that are correct in both eyes (single-pass stereo) |
| `localization` | Unity | Locales, string tables, CJK fonts |

### 5. Multiplayer

| Skill | Source | Use it for |
|---|---|---|
| `setup-multiplayer-services`, `setup-vivox-voice-chat` | Unity | Unity multiplayer sessions; Vivox voice chat |
| `unity-multiplayer` | community (Nice-Wolf-Studio) | Netcode for GameObjects 2.x, Relay, Lobby |

No vetted skill covers Photon or Normcore yet (see [docs/community-candidates.md](docs/community-candidates.md)).

### 6. Performance, debugging and testing

| Skill | Source | Use it for |
|---|---|---|
| `hz-unity-code-review` | Meta | VR performance and rendering review of Unity code (see ERRATA about multiview) |
| `hz-perfetto-debug`, `hz-simpleperf-debug` | Meta | GPU/CPU frame-timing traces; CPU hotspots |
| `hz-vr-debug` | Meta | Logcat, crashes, screenshots and video from the headset |
| `meta-xr-operator-unity` | **original** | Set up Meta XR Operator so Claude can see and act inside a running XR app (Meta XR Simulator, Quest Link or a development build on the headset), with safety rules and an HTTP client script |
| `hz-xr-simulator-install-and-configure`, `hz-xr-simulator-control` | Meta | Test without a headset in Meta XR Simulator (207.1 has a Meta VR Glasses profile). With Core SDK ≥ 207, **don't** add `com.meta.xr.simulator` (see [ERRATA](skills/meta-official/ERRATA.md)) |
| `project-auditor-fixes` | Unity | Project Auditor scan-and-fix loop |
| `unity-test-runner-cli` | **original** | EditMode/PlayMode tests from the CLI or CI, with a results summariser |
| `test-designing-guide` | community (nowsprinting) | How to design test cases |
| `scenario-unity-expert`, `scenario-unity-performance`, `scenario-unity-mobile`, `scenario-unity-pipeline-automation` | community (scenario-labs) | Agent protocol for Unity 6.3 on a Mac; profiling; mobile-GPU rules that also apply to Quest; asset import and CI automation |

### 7. Ship to the Horizon Store

| Skill | Source | Use it for |
|---|---|---|
| `horizon-launch-roadmap` | **original** | Launch timeline, which skill to use when, and the decisions you can't undo |
| `unity-quest-android-build` | **original** | Dev/release APKs, three build routes, APK checks, build-failure fixes |
| `horizon-store-publish-unity` | **original** | Dashboard setup → release APK → upload → release channels, device targeting (`quest3+`, "Future devices" for Meta VR Glasses), OBB, DLC, CI |
| `horizon-vrc-checklist` | **original** | All 80 active VRCs by ID, each with a Unity check and a headset test |
| `horizon-store-listing-assets` | **original** | Store art specs, metadata limits, IARC rating, privacy policy, Data Use Checkup, pricing, price A/B tests, Try Before You Buy, bundles, analytics, with an image checker |
| `hz-unity-platform-sdk` | Meta | Entitlements, achievements, leaderboards, IAP, DLC, rich presence, users |
| `hz-store-submit` | Meta | Meta's submission walkthrough. **Its specs are out of date**: read [ERRATA.md](skills/meta-official/ERRATA.md) |
| `submit-game-version` | community (dilmerv) | Real example of build + upload to BETA from Claude Code. Tied to one project; adapt it |

### 8. Design, art, marketing

| Skill | Source | Use it for |
|---|---|---|
| `quest-level-blockout` | **original** | Block out a level from one JSON file: a validator for gaps, height steps and landing widths, generated grey-box geometry with separate gameplay collision, viewpoint screenshots, a top-down map and a Quest 3 per-eye render, all headless. Three compile-tested C# templates |
| `quest-asset-sourcing` | **original** | Free CC0 art: Kenney, Quaternius, Poly Haven. Which downloads can be scripted (direct zips, OpenGameArt mirrors, Poly Haven API) and which need the user (itch.io, throttled Google Drive), license checks, `CREDITS.md`, Quest budgets. Includes a Poly Haven fetch script |
| `hz-immersive-designer` | Meta | Comfort, interaction patterns, spatial layout, accessibility |
| `xr-reference`, `xr-perceptual-constraints-displays-and-tracking`, `xr-rendering-input-and-spatial-understanding`, `xr-platforms-audio-and-comfort`, `xr-ux-design-and-performance-budgeting` | community (the-vibey-project) | XR design reference: cybersickness, comfort, locomotion, rendering, frame budgets |
| `game-gdd-author`, `game-playtest` | community (AlterLab-IEU) | Write a game design document; plan, run and analyse playtests |
| `prototype-fast` | community (gamedev-skills) | One-hour "is it fun?" prototype with keep/kill criteria |
| `scenario-blender-expert`, `scenario-blender-retopology`, `scenario-blender-uv-baking` | community (scenario-labs) | Blender game-asset pipeline: low poly, UVs, baking |
| `review-game-trailer` | community (GarrettPetersen) | Plan and review the store trailer |

---

## Notes and decisions

- **Meta VR Glasses (announced at Connect 2026, shipping spring 2027).** Same OS, Store and v207
  SDKs as Quest. Eyes aim and a pinch selects; controllers are sold separately, and an app that
  requires them is blocked behind a "Switch to Controllers" dialog. The view is about 70° × 66°
  (Quest 3: 110° × 96°). Build for it now with `meta-vr-glasses-unity` and test in Meta XR
  Simulator 207.1. Condensed facts with sources: [docs/meta-reference/](docs/meta-reference).
- **Security: Meta's AI Agent Bridge runs Claude Code without permission checks.** In Meta XR Core
  SDK 207.0.1, the opt-in "Enable AI Agent Bridge" feature (used by the Device Readiness Check's AI
  mode and the in-Editor assistant) starts `claude` with `--dangerously-skip-permissions`
  (`Editor/AgentBridge/ClaudeCodeService.cs`). It's off by default. Leave it off, or use it only
  in a throwaway copy of the project; the skills here drive Claude Code normally instead.
- **Meta's values vs. Meta's skills.** Meta's own `hz-store-submit` has outdated store specs: icon
  1024 instead of **512**, hero 2560×1440 instead of **3000×900**, 3 screenshots instead of **5**,
  target SDK ≥ 32 instead of **34 for apps created since 2026-03-01**. These were checked against
  developers.meta.com. The upstream files stay unchanged; corrections are in
  [ERRATA.md](skills/meta-official/ERRATA.md), and the `horizon-*` skills use the current values.
- **Unity's Meta Quest build profile defaults to target API 32.** Set 34 explicitly; the build
  scripts enforce it.
- **One build script per project.** Use `unity-quest-android-build/templates/QuestBuild.cs`.
  Switch to `horizon-store-publish-unity/scripts/QuestReleaseBuild.cs` only for games over 1 GB
  that need an OBB split (`QUEST_SPLIT_OBB=1`). Both read the same `QUEST_*` variables.
- **One verify-first skill.** Use `hz-quest-verify-first`. `quest3-verify-first` is here because the
  MR UI director depends on it.
- **Unity CLI over community MCP bridges.** Unity now recommends its CLI (and `unity mcp`).
  CoplayDev and IvanMurzak bridges, Unity IAP, LevelPlay ads and the 2D skills were left out on
  purpose. Horizon Store purchases go through Meta's Platform SDK.
- **Horizon Worlds is a different product.** Worlds are built in Meta's own editor and hosted by
  Meta. A Unity game ships as an APK on the **Horizon Store** ([research notes](docs/research-meta.md) §4).
- **Found by running a real project** (Unity 6000.6.6f1, Meta XR Core SDK 207.0.1, macOS,
  2026-10-16):
  - `com.meta.xr.simulator` 81.0.1 doesn't compile with Core 207, which already contains the
    simulator integration ([ERRATA](skills/meta-official/ERRATA.md)).
  - metavr 1.8's MCP tools don't load in Claude Code 2.1.301 because of a protocol-field bug; a
    small relay that ships with `unity-quest-dev-setup` fixes it ([mcp/README.md](mcp/README.md)).
  - Headless Editor scripts hit several traps: TMP and package imports and Meta's Fix All don't
    finish in a `-quit` run, and OpenXR validation needs a second run
    ([batch-mode-traps.md](skills/custom/unity-quest-dev-setup/references/batch-mode-traps.md)).
- **Not verified yet:** whether `unity build` Android signing flags override a Meta Quest build
  profile, on-device PlayMode tests, the revenue-share percentage, Immersive Image Layer sizes and
  asset-file (DLC) size limits. The skills mark these and tell Claude to confirm them at runtime.

## Keeping it current

The Unity CLI is in beta with roughly weekly releases, and Meta updates its repo often. To refresh
the copies, follow each folder's `SOURCE.md` (re-clone upstream, `diff -rq`, review, copy). Research
notes and all source URLs: [docs/research-unity.md](docs/research-unity.md),
[docs/research-meta.md](docs/research-meta.md). Rejected candidates:
[docs/community-candidates.md](docs/community-candidates.md).

## License

- **Original work** (`skills/custom/`, docs, README, install script, MCP notes): [MIT](LICENSE),
  © 2026 Navid Mirzaaghazadeh.
- **Meta's skills**: Apache-2.0. **Unity's skills**: Unity Companion License v1.4, for use with
  Unity-authored content. **Community skills**: each keeps its own MIT, Apache-2.0 or Unlicense file.
- Full breakdown and attribution: [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md). Contributing:
  [CONTRIBUTING.md](CONTRIBUTING.md).

Not affiliated with or endorsed by Unity Technologies or Meta Platforms, Inc. Unity, Meta Quest and
Meta Horizon are trademarks of their owners and are used here only to describe compatibility.
