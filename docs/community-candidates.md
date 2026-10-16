# Community skill candidates: Unity, Quest VR, game development and publishing

Evaluated 2026-10-16. Scope: third-party and community Agent Skills (folders with a SKILL.md). Meta's official `meta-quest/agentic-tools`, Unity Technologies' own skills, and custom-authored skills are handled elsewhere and are out of scope here.

**Method**
- **Search:** GitHub (`gh search repos`, `gh search code "filename:SKILL.md <topic>"` for Unity, OpenXR, XRI, Meta Quest, Netcode, Photon, Normcore, URP shaders, MonoBehaviour, Blender bpy, playtest, GDD, trailer, store page, VR comfort, profiler) and web search of skills.sh, claudskills.com, claudeskills.info and vibeindex.ai.
- **Metadata:** stars, license and last push were collected through the GitHub GraphQL API on 2026-10-16.
- **Downloads:** each source was shallow- or sparse-cloned into its own scratch folder. Nothing was installed or executed.
- **Review:** every collected SKILL.md was read in full. Every script was grepped for network, process, deletion, eval and credential access, and all docs for injection-style instructions.

**Selection criteria:**
1. The license permits redistribution.
2. It is accurate and useful for Unity 6 and Quest.
3. It was maintained in 2025-2026.
4. It is not a thin wrapper.
5. It is safe.

Collected skills live in `skills/community/<skill-name>/`, each with a `SOURCE.md`. The folder name equals the skill's `name:`.

---

## Picked (23)

| Folder | Upstream owner | Purpose | License | Stars / last commit |
|---|---|---|---|---|
| `unity-xr-mr-ui-director` | Wu030616 | Quest 3 spatial UI: brief in metres, one input stack, world-space Canvas, MRUK placement, device QA, plus a read-only project auditor | Apache-2.0 | 0 / 2026-07-14 |
| `quest3-verify-first` | Wu030616 | Verify Quest, Unity and OpenXR claims against primary sources and the real project before mutating it (dependency of the UI director) | Apache-2.0 | 0 / 2026-07-14 |
| `submit-game-version` | dilmerv | Template: bump version, build signed Quest APK in the open editor, upload to Horizon Store BETA with `ovr-platform-util` | MIT | 2 / 2026-08-30 |
| `unity-xr` | Nice-Wolf-Studio | Unity 6.3 XR stack: XRI 3.3, OpenXR, locomotion, haptics, simulator, anti-patterns | MIT | 40 / 2026-09-15 |
| `unity-multiplayer` | Nice-Wolf-Studio | Netcode for GameObjects 2.x (`[Rpc(SendTo)]`), Distributed Authority, Relay and Lobby | MIT | 40 / 2026-09-15 |
| `xr-perceptual-constraints-displays-and-tracking` | the-vibey-project | VR reference part 1: latency, cybersickness, VAC, presence, displays, tracking | MIT | 3 / 2026-10-11 |
| `xr-rendering-input-and-spatial-understanding` | the-vibey-project | VR reference part 2: single-pass stereo, reprojection, foveation, input, locomotion comfort table, anchors | MIT | 3 / 2026-10-11 |
| `xr-platforms-audio-and-comfort` | the-vibey-project | VR reference part 3: OpenXR, spatial audio, comfort, safety, accessibility | MIT | 3 / 2026-10-11 |
| `xr-ux-design-and-performance-budgeting` | the-vibey-project | VR reference part 4: 3D UX, frame-budget method | MIT | 3 / 2026-10-11 |
| `xr-reference` | the-vibey-project | VR reference part 5: anti-patterns, numbers, platform landscape | MIT | 3 / 2026-10-11 |
| `scenario-unity-expert` | scenario-labs | Lead Unity 6.3 on macOS agent protocol: batch, headless bridge, **official Unity CLI and com.unity.pipeline**, quality gates, 6.3 traps, shared toolkit | MIT | 1032 / 2026-10-15 |
| `scenario-unity-mobile` | scenario-labs | Android, Vulkan and TBDR budgets, ASTC rules, mobile URP, IL2CPP, signing from env (Quest-relevant parts) | MIT | 1032 / 2026-10-15 |
| `scenario-unity-performance` | scenario-labs | Measurement-first profiling, GC, SRP Batcher and GRD, PSO warm-up, spike attribution | MIT | 1032 / 2026-10-15 |
| `scenario-unity-shaders` | scenario-labs | URP 17.3 HLSL and Shader Graph, all passes, variants, PSO; templates are stereo-instancing correct | MIT | 1032 / 2026-10-15 |
| `scenario-unity-pipeline-automation` | scenario-labs | Import rules, prefabs, Addressables, content tests, CLI and CI builds, release provenance | MIT | 1032 / 2026-10-15 |
| `scenario-blender-expert` | scenario-labs | Blender 5.2 agent protocol plus audit and review toolkit (lead) | MIT | 1032 / 2026-10-15 |
| `scenario-blender-retopology` | scenario-labs | Dense or AI mesh to game low poly with budgets and topology audits | MIT | 1032 / 2026-10-15 |
| `scenario-blender-uv-baking` | scenario-labs | Game UVs, texel density, measured high-to-low bakes | MIT | 1032 / 2026-10-15 |
| `test-designing-guide` | nowsprinting | Unity test design method: layers, equivalence partitions, game error-guessing | Unlicense | 20 / 2026-10-09 |
| `game-playtest` | AlterLab-IEU | Behavioural playtest protocol, observation sheets, synthesis | MIT | 45 / 2026-03-30 |
| `game-gdd-author` | AlterLab-IEU | Socratic 10-section GDD with pillars, scope tiers and MDA tags (templates bundled) | MIT | 45 / 2026-03-30 |
| `review-game-trailer` | GarrettPetersen | Plan and audit trailers from authentic gameplay, with a timecoded review | MIT | 7 / 2026-10-06 |
| `prototype-fast` | gamedev-skills | One-question, timeboxed greybox prototype with keep or kill criteria | Apache-2.0 | 1550 / 2026-10-09 |

**Install notes**
- The `scenario-*` specialists find their lead skill as a sibling folder named exactly `scenario-unity-expert` or `scenario-blender-expert`. Install them into the same parent folder.
- `submit-game-version` is project-specific. Adapt the names and paths before use. Its upstream project's local env file was not copied.

---

## Rejected: licensed but not picked

| Candidate | License | Stars / last | Reason |
|---|---|---|---|
| Nice-Wolf-Studio/unity-claude-skills: `unity-ops` (`unity-cli-contract`, `unity-surface-preflight`) | MIT | 40 / 2026-09-15 | Highly relevant to the official Unity CLI, but **machine-specific**: hard-codes `~/.zshrc:46` and `~/Dev/Unity/ai_test`, depends on unpublished sibling skills (`wolf-core`, `unity-live-edit-verification`), is at v0.1.0, and hard-stops unless the `claude-code` skill shows as installed. Revisit when unity-ops ships. |
| Nice-Wolf-Studio: `unity-lifecycle`, `unity-3d-math` | MIT | 40 / 2026-09-15 | **Accuracy.** lifecycle claims `== null` is true right after `Destroy()`, but it stays false until end of frame, and it uses obsolete `FindObjectOfType`. 3d-math misstates `Plane.Raycast` "front side" semantics. A "correctness" skill with errors is worse than none. |
| Nice-Wolf-Studio: the other 28 guides (2D, animation, UI, physics and more) | MIT | 40 / 2026-09-15 | Decent doc digests that duplicate the scenario-labs picks and Unity's own skills. Not needed for a first library. |
| scenario-labs/skills: `scenario-unity-rendering-lighting`, `-ui`, `-gameplay`, `-architecture`, `-animation`, `-vfx`, `-world-building`, `-2d`, `-web`, and other Blender skills | MIT | 1032 / 2026-10-15 | Same high quality, but kept out to stay focused. **Good optional add-ons**, especially rendering-lighting (baked lighting and URP tiers) and ui. Same sibling-install rule applies. |
| adevra/unity-shader-agent-skills | MIT | 14 / 2026-03-28 | **VR-incorrect for Quest.** No single-pass-instanced stereo macros in its templates, so custom shaders render in one eye. Mixes Built-in RP advice (`noforwardadd`, `tex2D`) with URP, and overgeneralizes "avoid branching" and "never discard". Superseded by `scenario-unity-shaders`. |
| claude-dev-suite/claude-dev-suite `skills/gamedev/unity-xr` (+ `unity-performance`, `unity-netcode`) | MIT | 42 / 2026-10-12 | Concise and practical (Quest refresh rates, single-pass, comfort checklist). Runner-up that **duplicates** Nice-Wolf `unity-xr`. Kept the deeper one. |
| Dev-GOM/claude-code-marketplace (`unity-test-runner`, `unity-compile-fixer`, `blender-toolkit`, `unity-editor-toolkit`) | Apache-2.0 | 97 / 2026-01-17 | Test runner and compile fixer are **superseded** by the official `unity test` CLI and scenario-unity-expert's `ut_run`, and show 2021-era examples. The editor and Blender toolkits are wrappers around the plugin's own WebSocket servers. |
| CoplayDev/unity-mcp (`unity-mcp-orchestrator`, `blender-to-unity`) | MIT | 15,023 / 2026-10-08 | **Thin wrappers** around MCP for Unity and BlenderMCP tool calls. The user drives Unity through the official CLI. Use them only if you adopt that MCP. |
| IvanMurzak/Unity-MCP (23 generated skills) | Apache-2.0 | 4,482 / 2026-10-16 | One skill per MCP tool (`script-execute`, `screenshot-*` and others). They only work with that MCP plugin: **wrapper**. |
| Besty0728/Unity-Skills (84 skills, incl. `xr`, `netcode-design`, `pico-design`) | MIT | 1,859 / 2026-10-16 | Most skills drive a **REST server from its own Unity package** (`com.besty.unity-skills`), a competing control surface to the official CLI. The "design" skills are short. Revisit only if that package is adopted. |
| XeldarAlz/everything-claude-unity | MIT | 24 / 2026-04-24 | Built around CoplayDev's MCP plus its own hooks, agents and statusline. Generic patterns that duplicate better picks. |
| Donchitos/Claude-Code-Game-Studios (72 skills) | MIT | 26,719 / 2026-10-08 | Excellent but **framework-coupled**. Skills inject `!bash .../yaml-helper.sh` context and read `.claude/docs/*` and director gates, so single skills do not work extracted. Adopt the whole studio as a separate decision. |
| IdoCohen560/claude-unity-game-studio, pixel-cellar/Claude-Code-Game-Studios, Euraika-Labs/Codex-Code-Game-Studios, TraftG/opencode-game-studio | MIT | 22 / 359 / 25 / 45 | Ports or forks of Donchitos: **duplicates**. |
| fagemx/gstack-game (29 skills: `playtest`, `feel-pass`, `game-review` and others) | MIT | 74 / 2026-05-31 | **Framework-coupled.** Every skill starts with a bash preamble that needs `gstack-game/bin`, writes session and usage logs to `~/.gstack/`, and is generated from templates. Not portable. |
| AlterLab-IEU/AlterLab_GameForge: other 32 skills (`game-launch`, `game-brainstorm`, `game-unity-specialist` and others) | MIT | 45 / 2026-03-30 | `game-launch` has no Meta Horizon Store coverage. The rest overlaps with picks. Two skills were taken. |
| gamedev-skills/awesome-gamedev-agent-skills: Unity skills, `game-feel`, `performance-optimization`, `steam-publish` | Apache-2.0 | 1,550 / 2026-10-09 | Good quality but generic. The Unity skills overlap with the scenario-labs picks, and `game-feel` suggests screen shake, which is wrong for VR. Took `prototype-fast` only. |
| nowsprinting/unity-coding-skills: `plan-feature`, `fix-bug`, `run-tests`, `edit-scene`, `test-writing-guide` | Unlicense | 20 / 2026-10-09 | Need the JetBrains Rider MCP server or the author's Test Helper UPM packages. Took the dependency-free `test-designing-guide`. Revisit if you use Rider. |
| tjboudreaux/cc-plugin-unity-gamedev (21 skills) | MIT | 13 / 2026-02-06 | Mostly about third-party packages (Behavior Designer, FlowCanvas, MemoryPack, Wwise, Sentry, Animancer). Its mobile and profiling skills are superseded by scenario-labs. |
| devdavv/unity-ai-workflow | MIT | 44 / 2026-04-01 | Tied to its own ProjectConfig, GDD-Gherkin and Reflex DI workflow. Includes a skill-creator copy that duplicates one the user already has. |
| Codeturion/skills (`unity-headless-cli`, `unity-addressables`, `unity-object-pooling`, `unity-ios-cicd`) | MIT | 0 / 2026-08-23 | `unity-headless-cli` bootstraps the official CLI, but it duplicates Unity's own `unity-cli` skill and scenario-unity-expert, and it edits CLAUDE.md. The others are generic or iOS-specific. |
| bigdra50/unity-cli (9 skills) | MIT | 39 / 2026-06-10 | Japanese-language skills for a **different third-party `u` CLI**, which conflicts with the official Unity CLI. |
| jahro-console/unity-agent-skills | MIT | 17 / 2026-03-20 | Product-specific (Jahro debug console). |
| MelonS/MelonS-Agents (`game-dev-agent`) | MIT | 17 / 2026-08-22 | Portfolio agent for Unity 2022.3 with ComfyUI, Suno and AudioCraft. Not Unity 6 or VR, and depends on external generators. |
| delta120S/gamedev-agent-skills | MIT | 0 / 2026-10-07 | Distilled from one iOS project (PVRTC to ASTC, console forensics). Narrow. |
| batihandev/unity-mcp-skills | MIT | 6 / 2026-10-10 | C# recipes for CoplayDev's MCP: **wrapper**. |
| skysepehru/unity-claude-skills | MIT | 0 / 2026-07-18 | Opinionated Zenject and UGUI house conventions. Several SKILL.md files lack `name`. |
| sickn33/agentic-awesome-skills `game-development/vr-ar`, `unity-developer` | MIT code, CC-BY-4.0 content | 47,725 / 2026-10-16 | **Thin.** About 145 lines of generic tables (vr-ar), with no Unity or Quest depth. Widely mirrored. |
| travisjneuman/.claude `ar-vr-xr` | MIT | 101 / 2026-10-16 | **Thin** (97 lines, generic samples). |
| wshobson/agents `unity-ecs-patterns` | MIT | 40,913 / 2026-10-15 | Thin (62 lines), and DOTS is rarely needed for a solo Quest game. |
| IceMasterT/Unity-6.3_SKILLS (31 skills) | MIT | 0 / 2026-08-19 | Each skill grounds answers in **local offline Unity 6.3 docs** that must be present. Unproven, 0 stars. |
| Lagunaswift/GameDevelopmentAudit | MIT | 6 / 2026-07-13 | Very short (about 45 lines each) design "lenses". Thin. |
| rhino-ty/game-architect | MIT | 3 / 2026-04-28 | A single generic all-in-one design skill. Covered by the AlterLab GDD and playtest picks. |
| ybuild-ai/ai-game-art-pipeline-skill | MIT | 299 / 2026-06-17 | Mostly 2D sprite and animation art from AI images. Not a fit for 3D VR. |
| RobLe3/cc-blender-skill (31 skills incl. `blender-export`) | MIT | 100 / 2026-10-12 | Solid Blender 5.x skills, but they drive Blender through a Blender MCP and focus on mascot and logo reconstruction. Overlaps with scenario-labs, which also works headless. `blender-export` is a reasonable add-on. |
| Impertio-Studio/Blender-Bonsai-ifcOpenshell-Sverchok package | MIT | 41 / 2026-07-08 | Architecture and IFC (AEC) focus. Not games. |
| TerminalSkills/skills (`blender-scripting` and others) | Apache-2.0 | 168 / 2026-10-14 | Generic, broad library. Not evaluated in depth, because scenario-labs covers Blender better. |
| dontoreve/store-screenshots-skill, adamlyttleapps/claude-skill-aso-appstore-screenshots, abhishekdubey331/google-play-screenshot-skill | MIT | 12 / 1,785 / 32 | App Store, Microsoft Store and Google Play screenshot generators. They do not cover Meta Horizon Store asset specs. |
| GarrettPetersen: `plan-indie-game-marketing`, `audit-steam-store-page`, `optimize-steam-tags` | MIT | 7 / 2026-10-06 | Steam-specific. The trailer skill was taken. |
| managedcode/dotnet-skills (`mixed-reality`, C# skills) | MIT | 489 / 2026-10-16 | .NET, MAUI and HoloLens-oriented. Not Unity or Quest. |
| niaka3dayo/agent-skills-vrc-udon, felixchaos/vrchat-avatar-modding-skill | MIT | 336 / 12 | VRChat-specific (UdonSharp, avatars). |
| jame581/GodotPrompter `xr-development`, bgrenat/godot-game-dev-studio `godot-platform-vr`, thedivergentai/Game-Dev-Agentic-Skills | MIT / LGPL-3.0 | 837 / 20 / 857 | Godot, not Unity. |
| asterkin/openxr-quest-tutorial | MIT | 2 / 2026-01-07 | Native NDK OpenXR tutorial helper. Not Unity. |
| anthropics/skills | per-skill | 181,572 / 2026-10-09 | No Unity, VR or game-dev skills. `skill-creator` is already available to the user. |
| Firzus/agent-skills (`game/unity`, `figma-to-unity`) | MIT | 3 / 2026-10-14 | General-purpose personal library. The Unity skill is generic. |

## Link only: no license, or the license does not allow redistribution

Read these for ideas. Do not copy them.

| Candidate | Stars / last | Notes |
|---|---|---|
| SepEmmers/vr-volley `standalone-quest-profiler`, `xr-rig-and-input` | 0 / 2026-10-13 | No license. A Quest profiling workflow is interesting. Ask the author to add a license. |
| tommy-xr/shock2quest `oculus-profiling`, `vr-device-loop` | 21 / 2026-10-16 | NOASSERTION license. ADB and Perfetto device loop for a Rust Quest engine. |
| JDMAXilius/Cosmo-Simulation- `quest-build`, `perf-gate` | 0 / 2026-10-07 | No license. Project-specific. |
| rrrzzz/Xenobjects `unity-quest-xr` | 0 / 2026-07-07 | No license. |
| shihwesley/shihwesleys-harness `unity-crossplatform-xr` | 0 / 2026-03-17 | No license. |
| Benams5211/TeamRedHatss `openxr-meta-quest` | 1 / 2026-04-02 | No license. |
| rygo6/OpenXR-AGENTS | 0 / 2026-09-05 | No license. Native OpenXR. |
| MuharremTozan/unity-agent-skills | 17 / 2026-09-30 | No license. Unity 6 patterns. Listed on skills.sh. |
| baxatron-git/claude-game-design-suite (`gdd-author`, `playtest-protocol-designer` and 20 more) | 16 / 2026-07-08 | No license. A design-lifecycle suite worth a look. |
| truongtv22/theone-training-skills (copy of `the1studio/theone-training-skills`) | 1 | Original repo not found (removed or private). The copy has no license. |
| belokonm/claude-supercode-skills `game-developer-skill` (the `404kidwiz/claude-supercode-skills` lead now 404s) | 11 / 2026-09-03 | No license. |
| MinhLN1234/Claude-Cowork-Game-Studio | 69 / 2026-10-04 | No license. |
| eyenpi/unity-systems-skills, JEO-tech-ai/oh-my-unity3d, CryptoRabea/Claude_Unity_dev_plugin, egod1537/agent-skills, badjano/skills | 0-8 | No license. |
| alphaonedev/openclaw-graph `unity-xr` (listed on skills.sh) | n/a | Repo returns 404. Unavailable. |
| Unity-Technologies/unity-agent-plugin, keijiro/unity-skills | 421 / 10 | NOASSERTION. Unity's own skills or a mirror of them. Out of scope (handled by another collector). |
| `hz-*` copies in many game repos (fhuaquistoq/japanese-demon-hunter, DevAkre/Parallax, Abhaythakurrr/pooklet and others) | n/a | Copies of Meta's official skills. Out of scope. |

## Gaps (no good redistributable community skill found)

- **Meta Horizon Store listing assets** (cover art, hero art, screenshots, trailer specs, store copy). Use Meta's docs or a custom skill.
- **Quest on-device profiling** (OVR Metrics Tool, Perfetto, RenderDoc for Quest) for Unity. Covered by Meta's official skills. The community versions are unlicensed.
- **Photon Fusion and Normcore** multiplayer. Only project-specific or unlicensed skills exist.
- **VR-specific playtesting** (simulator-sickness questionnaire, comfort ratings). Combine `game-playtest` with the comfort sections of the five `xr-*` skills.
- **VR trailer capture** (spectator camera, mixed-reality capture).
