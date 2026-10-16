# Unity × Meta Quest skill library

Claude Code skills for building **Meta Quest VR games in Unity**, driven through the **official Unity
CLI**, and shipping them on the **Meta Horizon Store**. Collected and written on 2026-10-16.

**Nothing here is installed.** This folder is only a library. Copy the skills you want when you're
ready (see [Installing later](#installing-later)).

```
unity-meta/
├── skills/
│   ├── meta-official/    26  Meta's own skills (meta-quest/agentic-tools, Apache-2.0) + ERRATA.md
│   ├── unity-official/   20  Unity's own skills (Unity-Technologies/skills, Unity Companion License)
│   ├── custom/            8  Written for this library from Meta and Unity docs, to fill gaps
│   └── community/        23  Vetted third-party skills (MIT / Apache / Unlicense)
├── mcp/                      MCP server notes + mcp.json.example (metavr, unity)
├── docs/                     Research notes, sources, metavr CLI reference, rejected candidates
├── LICENSE                   MIT, for the original work only
└── THIRD_PARTY_NOTICES.md    Who wrote what, and under which license
```

Every collected folder has a `SOURCE.md` with repo, commit, license and a security review. Each
author's license is kept with its files.

---

## The two tools everything runs on

| Tool | What it is | Install (when you're ready) |
|---|---|---|
| **`unity`**: Unity CLI (beta, July 2026) | Editors, modules, projects, builds, tests. It also controls the open Editor live through the `com.unity.pipeline` package (`unity command …`, `run_script`, `eval`) | `curl -fsSL https://unity.com/install.sh \| bash` or `brew install --cask unity-cli`. Then, in each project: `unity pipeline install` |
| **`metavr`**: Meta VR CLI (formerly `hzdb`) | Quest device control, logs, screenshots, Perfetto traces, Meta docs search, XR Simulator, **store upload** (`metavr store dist upload`) | `curl -fsSL https://developers.meta.com/horizon/install-cli/ \| sh`, or `npx -y metavr` with no install |

Meta's Unity skills run their C# against a live Editor through Unity's **`unity-cli`** skill, so
`unity-official/unity-cli` is required whenever you use `meta-official/hz-unity-*`.

---

## Start here: recommended core set (about 20 skills)

Don't install all 77 at once. Every skill's description costs context, and several overlap. This
set covers the full path from an empty project to the store:

| Phase | Skills |
|---|---|
| Guardrail | `meta-official/hz-quest-verify-first` |
| Tooling | `unity-official/unity-cli`, `unity-official/unity-pipeline`, `meta-official/metavr-cli` |
| Setup | `custom/unity-quest-dev-setup`, `meta-official/hz-new-project-creation`, `custom/quest-unity-project-settings`, `meta-official/hz-unity-meta-core-sdk` |
| Build the game | `meta-official/hz-unity-meta-interaction-sdk`, `meta-official/hz-unity-meta-quest-ui`, `meta-official/hz-immersive-designer`, `meta-official/hz-unity-code-review` |
| Test and debug | `custom/unity-test-runner-cli`, `meta-official/hz-vr-debug`, `meta-official/hz-perfetto-debug` |
| Ship | `custom/unity-quest-android-build`, `custom/horizon-store-publish-unity`, `custom/horizon-vrc-checklist`, `custom/horizon-store-listing-assets`, `custom/horizon-launch-roadmap`, `meta-official/hz-unity-platform-sdk` |

Add the others below as the game needs them.

---

## Full catalog

### 1. Guardrails and tooling

| Skill | Source | Use it for |
|---|---|---|
| `hz-quest-verify-first` | Meta | **Load first in every Quest project.** Makes Claude check Quest claims against Meta's live docs (`metavr docs`) instead of stale memory |
| `metavr-cli` | Meta | The complete `metavr` reference: device, app, logs, capture, perf, docs search, MCP |
| `unity-cli` | Unity | The `unity` CLI: Editors, modules, projects, build, test, `command`/`eval`, MCP, Safe Mode recovery |
| `unity-pipeline` | Unity | Live Editor control: `run_script`, `eval`, `recompile`, `run_tests`, `audit`, autotick |
| `unity-package-management` | Unity | Add or remove UPM packages headlessly (the Meta XR SDKs are UPM packages) |
| `generate-editor-search-query` | Unity | Unity Search queries to find assets and objects |

### 2. Machine and project setup

| Skill | Source | Use it for |
|---|---|---|
| `unity-quest-dev-setup` | **custom** | CLI check, Unity 6 Editor with the Android modules, license, `unity pipeline install`, switch to Android / Meta Quest |
| `hz-new-project-creation` | Meta | Scaffold a new Meta VR Unity project (with `references/unity-project.md`) |
| `quest-unity-project-settings` | **custom** | Every recommended Quest setting, each sourced to a Meta page. Includes an audit/apply script (`QuestProjectSettingsAudit.cs`) |
| `hz-unity-meta-core-sdk` | Meta | OVRManager, OVRCameraRig, passthrough, hand tracking, anchors, boundaryless, Scene API, Android manifest |
| `hz-unity-sdk-upgrade`, `hz-api-upgrade` | Meta | Upgrading Meta XR SDK versions and replacing deprecated APIs |
| `hz-unity-project-analyzer` | Meta | Builds and maintains a `.agent-docs/` knowledge base of your Unity project |
| `migrate-birp-to-urp` | Unity | Move an older Built-in pipeline project to URP (recommended for Quest) |

### 3. Gameplay, interaction and mixed reality

| Skill | Source | Use it for |
|---|---|---|
| `hz-unity-meta-interaction-sdk` | Meta | Grab, poke, ray, locomotion with Meta's Interaction SDK (recommended on Quest) |
| `hz-unity-meta-mixed-reality-utility-kit` | Meta | MRUK: rooms, walls, furniture, placing content in the real world, scene NavMesh, room sharing |
| `hz-unity-passthrough-camera-access` | Meta | Quest 3/3S RGB camera access for computer vision and ML |
| `hz-unity-face-tracking`, `hz-unity-meta-movement-sdk-retargeting` | Meta | Face tracking on ARKit-blendshape heads; body tracking retargeting for characters |
| `hz-unity-placement` | Meta | Correct object placement using Renderer and Collider bounds |
| `hz-unity-fbx-import` | Meta | Importing external FBX models correctly |
| `hz-unity-device-readiness` | Meta | Audit for hand-tracking readiness and head-locked UI (Meta VR Glasses) |
| `physics-3d-collision` | Unity | Collisions and triggers (grabbing, throwing, hits) |
| `initialize-ai-navigation` | Unity | NavMesh for NPCs |
| `Nice-Wolf-Studio__unity-xr` | community | XR Interaction Toolkit + OpenXR route. **Pick one interaction stack**: Meta ISDK *or* XRI |

### 4. UI, text, audio, rendering

| Skill | Source | Use it for |
|---|---|---|
| `hz-unity-meta-quest-ui` | Meta | World-space canvases, sizing and viewing distance for VR |
| `hz-unity-tmp-resources`, `optimize-text-mesh-pro` | Meta, Unity | Import TextMesh Pro resources, fix pink text; optimise font atlases |
| `ui`, `ui-ugui`, `ui-uitk` | Unity | UI routing, uGUI, UI Toolkit |
| `Wu030616__unity-xr-mr-ui-director` | community | Quest 3 mixed-reality UI design and review. Needs `Wu030616__quest3-verify-first` |
| `audio-setup-mixers`, `optimize-audio` | Unity | Mixers; audio memory and CPU cost |
| `urp-postprocessing`, `validate-urp-render-graph-renderer-feature`, `shader-graph-create-custom-node` | Unity | URP volumes (keep them light on Quest), Render Graph features, Shader Graph nodes |
| `scenario-labs__scenario-unity-shaders` | community | URP shaders that are correct in both eyes (single-pass stereo) |
| `localization` | Unity | Locales, string tables, CJK fonts |

### 5. Multiplayer

| Skill | Source | Use it for |
|---|---|---|
| `setup-multiplayer-services`, `setup-vivox-voice-chat` | Unity | Unity multiplayer sessions; Vivox voice chat |
| `Nice-Wolf-Studio__unity-multiplayer` | community | Netcode for GameObjects 2.x, Relay, Lobby |

No vetted skill covers Photon or Normcore yet (see `docs/community-candidates.md`).

### 6. Performance, debugging and testing

| Skill | Source | Use it for |
|---|---|---|
| `hz-unity-code-review` | Meta | VR performance and rendering review of Unity code (see ERRATA about multiview) |
| `hz-perfetto-debug`, `hz-simpleperf-debug` | Meta | GPU/CPU frame-timing traces; CPU hotspots |
| `hz-vr-debug` | Meta | Logcat, crashes, screenshots and video from the headset |
| `hz-xr-simulator-install-and-configure`, `hz-xr-simulator-control` | Meta | Test without a headset in Meta XR Simulator |
| `project-auditor-fixes` | Unity | Project Auditor scan-and-fix loop |
| `unity-test-runner-cli` | **custom** | EditMode/PlayMode tests from the CLI or CI, with a results summariser script |
| `nowsprinting__test-designing-guide` | community | How to design test cases |
| `scenario-labs__scenario-unity-performance`, `__scenario-unity-mobile` | community | Profiler workflow; mobile-GPU (tile-based) rules that also apply to Quest |
| `scenario-labs__scenario-unity-expert`, `__scenario-unity-pipeline-automation` | community | Agent protocol for Unity 6.3 on a Mac (official CLI, batch mode); asset import and CI automation |

### 7. Ship to the Horizon Store

| Skill | Source | Use it for |
|---|---|---|
| `horizon-launch-roadmap` | **custom** | Launch timeline, which skill to use when, and the decisions you can't undo (package name, signing key, Early Access) |
| `unity-quest-android-build` | **custom** | How builds are made: dev/release APKs, three build routes, APK checks, build-failure fixes |
| `horizon-store-publish-unity` | **custom** | Dashboard setup → release APK → `ovr-platform-util` / `metavr store dist` upload → release channels, OBB, DLC, CI |
| `horizon-vrc-checklist` | **custom** | All 80 active VRCs by ID, each with a Unity check and a headset test |
| `horizon-store-listing-assets` | **custom** | Exact store art specs, metadata limits, IARC age rating, privacy policy, Data Use Checkup, pricing. Includes an image checker script |
| `hz-unity-platform-sdk` | Meta | Entitlements, achievements, leaderboards, in-app purchases, DLC, rich presence, users |
| `hz-store-submit` | Meta | Meta's submission walkthrough. **Its specs are out of date**: read `meta-official/ERRATA.md` |
| `dilmerv__submit-game-version` | community | Real example of build + upload to BETA from Claude Code. Tied to one project; adapt it, don't run it as-is |

### 8. Design, art, marketing

| Skill | Source | Use it for |
|---|---|---|
| `hz-immersive-designer` | Meta | Comfort, interaction patterns, spatial layout, accessibility |
| `the-vibey-project__xr-reference` + 4 parts: `xr-perceptual-constraints-displays-and-tracking`, `xr-platforms-audio-and-comfort`, `xr-rendering-input-and-spatial-understanding`, `xr-ux-design-and-performance-budgeting` | community | XR design reference: cybersickness, comfort, locomotion, rendering, frame budgets |
| `AlterLab-IEU__game-gdd-author` | community | Write a game design document |
| `gamedev-skills__prototype-fast` | community | One-hour "is it fun?" prototype with keep/kill criteria |
| `AlterLab-IEU__game-playtest` | community | Plan, run and analyse playtests |
| `scenario-labs__scenario-blender-expert`, `scenario-blender-retopology`, `scenario-blender-uv-baking` | community | Blender game-asset pipeline: low poly, UVs, baking |
| `GarrettPetersen__review-game-trailer` | community | Plan and review the store trailer |

---

## Decisions already made (and why)

- **Meta's values vs. Meta's skills.** Meta's own `hz-store-submit` has outdated store specs: icon
  1024 instead of **512**, hero 2560×1440 instead of **3000×900**, 3 screenshots instead of **5**,
  target SDK ≥ 32 instead of **34 for apps created since 2026-03-01**. These were checked against
  developers.meta.com. The upstream files are kept unchanged. Corrections are in
  `skills/meta-official/ERRATA.md`, and the `custom/horizon-*` skills use the current values.
- **Unity's Meta Quest build profile defaults to target API 32.** Set 34 explicitly; the custom build
  scripts enforce it.
- **One build script per project.** Use `custom/unity-quest-android-build/templates/QuestBuild.cs`
  as the standard one (release + development builds + settings preflight). Switch to
  `custom/horizon-store-publish-unity/scripts/QuestReleaseBuild.cs` only if the game is over 1 GB
  and needs an OBB split (`QUEST_SPLIT_OBB=1`). Both read the same `QUEST_*` environment variables.
- **One verify-first skill.** Use Meta's `hz-quest-verify-first`. The community
  `Wu030616__quest3-verify-first` is only here because the UI director skill depends on it.
- **Unity MCP vs CLI.** The Unity CLI (and its `unity mcp` mode) is Unity's recommended route now.
  Community bridges (CoplayDev, IvanMurzak) were left out on purpose, as were Unity IAP, LevelPlay
  ads and the 2D skills. Horizon Store purchases go through Meta's Platform SDK.
- **Horizon Worlds is a different product.** Worlds are built in Meta's own editor and hosted by
  Meta. A Unity game ships as an APK on the **Horizon Store**. See `docs/research-meta.md` §4.

## Not verified (marked inside the skills too)

Nothing was run against a real Editor, headset or Developer Dashboard. Open points include: whether
`unity build` Android signing flags override a Meta Quest build profile, on-device PlayMode tests,
the revenue-share percentage, Immersive Image Layer sizes, and asset-file (DLC) size limits. The
skills tell Claude to confirm these at runtime with `metavr docs search` or `--help`.

## Installing later

Skills load from `~/.claude/skills/<name>/` (every project) or `<project>/.claude/skills/<name>/`
(one project; recommended for a game repo). The **folder name must equal the skill's `name:`**, so
drop the `owner__` prefix on community skills. The scenario-labs specialists need their lead skill's
folder named exactly `scenario-unity-expert` / `scenario-blender-expert`.

```bash
# example: one game project, core set only
mkdir -p MyQuestGame/.claude/skills
cp -R skills/meta-official/hz-quest-verify-first skills/unity-official/unity-cli \
      skills/custom/unity-quest-android-build  MyQuestGame/.claude/skills/
cp -R skills/community/scenario-labs__scenario-unity-expert MyQuestGame/.claude/skills/scenario-unity-expert
```

Don't also install the `meta-vr` or `unity@unity-agent-plugin` plugins, `metavr init`,
`unity skill install` or `npx skills add`: they install the same official skills again, so Claude
would see duplicates. MCP setup is in `mcp/README.md`.

## License

- **Original work** (`skills/custom/`, docs, README, MCP notes): [MIT](LICENSE), © 2026 Navid Mirzaaghazadeh.
- **Meta's skills**: Apache-2.0. **Unity's skills**: Unity Companion License v1.4, for use with
  Unity-authored content. **Community skills**: each keeps its own MIT, Apache-2.0 or Unlicense file.
- Full breakdown, attribution and the Unity license terms: [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
- How to add or fix skills: [CONTRIBUTING.md](CONTRIBUTING.md).

Not affiliated with or endorsed by Unity Technologies or Meta Platforms, Inc. Unity, Meta Quest and
Meta Horizon are trademarks of their owners and are used here only to describe compatibility.

## Keeping it current

The Unity CLI is in beta with roughly weekly releases, and Meta updates its repo often. To refresh,
re-clone the upstream repos listed in each `SOURCE.md` and `diff -rq` before copying anything over.
If the installed CLI is newer than the library copy, `unity skill show` prints the current
`unity-cli` skill. Research notes and every source URL are in `docs/research-unity.md` and
`docs/research-meta.md`.
