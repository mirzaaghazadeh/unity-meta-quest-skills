# Source: Unity Skills (official, Unity Technologies)

| Field | Value |
|---|---|
| Publisher | Unity Technologies |
| Repository | https://github.com/Unity-Technologies/skills |
| Commit | `9d282100540601cfddc4bbd69f36a239d7a6f55c` (2026-10-08, "chore: publish 3 skills (create-2d-physics, create-tile-palette-or-rule-tiles, unity-pipeline) (#114)") |
| License | Unity Companion License v1.4: notice in `LICENSE.md`, full text in `UNITY-COMPANION-LICENSE.md` |
| Fetched | 2026-10-16 |
| Unity's docs page | https://docs.unity.com/en-us/ai/unity-plugin/about-unity-plugin |

The skill folders here are copied unchanged from `skills/` at the commit above. The only addition is
a copy of the repo's `LICENSE.md` in each skill folder, because section 5 of the license asks for the
notice in "all substantial portions of the Work".

## License check

The Unity Companion License allows reproduction and distribution "in connection with the authoring
and/or distribution of applications, software, or other content under a valid Unity ... Engine
License" (section 1). A private skill library used to build Unity games fits that. It may not be
used "for competitive analysis or to develop a competing product or service". Keep the notices with
the files if the library is shared.

## The same skills are published in two places

| Channel | What it is | Notes |
|---|---|---|
| `Unity-Technologies/skills` (this copy) | Plain skill folders, for `npx skills add Unity-Technologies/skills` / skills.sh | 34 skills. Includes `unity-pipeline` and `project-auditor-fixes` |
| `Unity-Technologies/unity-agent-plugin` (commit `cf6b2da`, 2026-10-06, plugin version `0.1.8-beta`) | Claude Code / Codex / Grok plugin: `/plugin marketplace add Unity-Technologies/unity-agent-plugin` then `/plugin install unity@unity-agent-plugin` | 32 skills: no `unity-pipeline`, no `project-auditor-fixes`. Its `unity-cli/SKILL.md` adds one paragraph telling the agent to pass `--caller plugin --skill <name>` on every `unity command`. Unity's own reference calls those flags analytics labels, "inert to the command itself". |
| Embedded in the `unity` CLI binary | `unity skill show`, `unity skill install claude-code [--local]`, `unity skill refresh` | Only the `unity-cli` skill, which always matches the installed CLI version. `--local` in a project with `com.unity.pipeline` also mirrors the package's own `unity-pipeline` skill |
| `unity setup claude` | Runs the two `claude plugin ...` commands for you | Changes Claude Code config. Run it only when the user asks |

Nothing was installed. Use only one channel per machine. Installing the plugin or running
`unity skill install claude-code` on top of this folder gives Claude two copies of `unity-cli`.

## What was copied (20 of 34)

| Skill | Why it matters for a Unity, Quest, and Horizon Store game |
|---|---|
| `unity-cli` | Core. Install and drive the `unity` CLI: Editors, modules, projects, `build`, `test`, `command`/`eval`, `mcp`, Safe Mode recovery. **Meta's `hz-*` skills point to this skill by name** ("Connecting, `--project-path`, and Safe Mode recovery are in the `unity-cli` skill") |
| `unity-pipeline` | Live Editor control through `com.unity.pipeline`: `set_autotick`, `recompile`, `run_tests`, `run_script`, `eval`, `reload_file`, `audit`. Meta's skills run their C# through `run_script` |
| `project-auditor-fixes` | Project Auditor scan and fix loop (`unity command audit`). Useful for Quest performance and settings issues |
| `unity-package-management` | Adds or removes UPM packages headlessly (Meta XR SDK packages are UPM) |
| `ui`, `ui-ugui`, `ui-uitk` | UI routing, uGUI (world-space canvases, which Meta's UI skills use), UI Toolkit |
| `physics-3d-collision` | Collision and trigger debugging (grabbing, throwing) |
| `audio-setup-mixers`, `optimize-audio` | Mixer routing, audio memory and CPU cost (Quest is memory-bound) |
| `optimize-text-mesh-pro` | TMP font atlases and world-space text, used by most VR UI |
| `urp-postprocessing`, `validate-urp-render-graph-renderer-feature` | URP Volume setup and Render Graph review. Keep post-processing minimal on Quest |
| `migrate-birp-to-urp` | Older VR projects on Built-in RP. Meta recommends URP |
| `shader-graph-create-custom-node` | Custom Shader Graph nodes |
| `initialize-ai-navigation` | NavMesh for NPCs |
| `setup-multiplayer-services`, `setup-vivox-voice-chat` | Multiplayer sessions and voice chat |
| `localization` | Locales, string tables, CJK fonts |
| `generate-editor-search-query` | Unity Search queries to find assets and objects |

## What was left out (14), and why

| Skill | Why skipped |
|---|---|
| `new-unity-project` | Generic guided "new game" flow. Its trigger ("start a new game or prototype") would compete with Meta's `hz-new-project-creation`, which a Meta VR project must use (`metavr_unity_setup`). Its CLI recipe is still in `unity-cli` → "Bootstrap a new project from scratch" |
| `implement-in-app-purchases` | Unity IAP targets Google Play and the App Store. Horizon Store purchases go through the Meta Platform SDK (`hz-unity-platform-sdk`) |
| `levelplay-unity-integration` | Mobile ad mediation. Not a Horizon Store fit |
| `build-live-game` | Unity Gaming Services backend. Its achievements and leaderboards overlap the Meta Platform SDK equivalents that show up in Horizon. Add it back if you want UGS Cloud Save, Remote Config or Economy |
| `2d-pixel-perfect`, `create-2d-physics`, `create-tile-palette-or-rule-tiles`, `manage-sprite-atlas`, `sprite-editor`, `sprite-segment-3x3grid` | 2D-only workflows |
| `optimize-web` | WebGL builds |
| `ui-imgui` | Editor-only immediate-mode GUI |
| `build-gtk` | Graph Toolkit for authoring node-graph Editor tools. Niche |
| `asset-transformer-toolkit` | Pixyz / Asset Transformer CAD import (separate paid product) |

Some copied skills mention skipped ones by name, for example `unity-cli` mentions `new-unity-project`,
`implement-in-app-purchases`, `levelplay-unity-integration` and `build-live-game` as hand-offs.
Those are pointers only, and nothing breaks without them.

## Security review (2026-10-16)

All 34 skills were read or scanned before copying. Checks: prompt-injection phrases, pipe-to-shell
commands, destructive commands, base64 or obfuscated payloads, hidden HTML comments, zero-width and
bidi characters, and the external domains used. Findings:

- The only pipe-to-shell lines are Unity's documented CLI installers (`curl -fsSL https://unity.com/install.sh | bash`, `irm https://unity.com/install.ps1 | iex`).
  `unity-cli/SECURITY.md` explains the SHA-256 pinning.
- `generate-editor-search-query` decodes a base64 placeholder (`QUERY_BASE64`) that the agent fills in. That is benign.
- The links go to Unity, GitHub, Apple, Google, IronSource and example domains. No suspicious hosts.
- The `.cs` files are reference snippets for the agent to adapt. Nothing runs at install.
- The repo's `scripts/check-skill-frontmatter.mjs` and `.github/` were not copied.

## Updating

```bash
git clone --depth 1 https://github.com/Unity-Technologies/skills.git /tmp/unity-skills
diff -rq --exclude=LICENSE.md /tmp/unity-skills/skills/unity-cli ./unity-cli   # repeat per skill and review before copying
```

`unity-cli` changes with every CLI release (the CLI is in beta and released about weekly). If the
installed CLI is newer than this copy, `unity skill show` prints the matching version of the skill
without installing anything.
