# Research notes: Unity CLI, official Unity agent skills, Unity MCP options

Compiled 2026-10-16 for the Unity + Meta Quest skill library. Every claim links to a source at the
end. Nothing was installed: no CLI, no plugin, no skills, no MCP servers.

## TL;DR

- **Unity publishes official Agent Skills.** They live in
  [`Unity-Technologies/skills`](https://github.com/Unity-Technologies/skills) (34 skills, commit
  `9d28210`, 2026-10-08) and, as a Claude Code / Codex / Grok plugin, in
  [`Unity-Technologies/unity-agent-plugin`](https://github.com/Unity-Technologies/unity-agent-plugin)
  (32 skills, v`0.1.8-beta`, commit `cf6b2da`). The `unity` CLI also embeds its own `unity-cli`
  skill (`unity skill show|install|refresh`). License: Unity Companion License v1.4, which allows
  redistribution for Unity-dependent work as long as the notice is kept. **20 relevant skills were
  copied** to `skills/unity-official/` (see its `SOURCE.md`).
- **Meta's `hz-*` skills point at a skill named `unity-cli`.** That is Unity's official skill, now
  present in `skills/unity-official/unity-cli/`. They also use the Pipeline commands `run_script`,
  `package_add`, `get_component_properties` and `save_scene`, which the official `unity-pipeline`
  skill documents.
- **Gaps Unity's skills don't cover** (no XR or Quest content anywhere in them) became three custom
  skills: `unity-quest-dev-setup`, `unity-quest-android-build` and `unity-test-runner-cli`.
- **The CLI is beta.** The latest is `1.0.0-beta.13` (2026-10-07), with roughly weekly releases.
  The docs still label it "experimental". `unity --help` / `unity <cmd> --help` is the authoritative
  flag list.

## 1. Unity CLI facts (docs.unity.com, fetched as `.md`)

The old docs URL `https://docs.unity.com/en-us/hub/unity-cli.md` now redirects to
`https://docs.unity.com/en-us/unity-cli/unity-cli.md`, and the redirect target was broken: it
pointed at `0.0.0.0:3000`. Index: `https://docs.unity.com/en-us/unity-cli/llms.txt`.

### Install / update
- Script (recommended): macOS/Linux `curl -fsSL https://unity.com/install.sh | bash`; Windows
  `irm https://unity.com/install.ps1 | iex`. `UNITY_CLI_HOME` changes the install root. On Linux
  the binary goes to `~/.local/bin/unity`.
- Package managers: `brew install --cask unity-cli`, `winget install Unity.CLI`, apt/dnf repos
  (`unity-cli` package; beta builds publish to `unstable`).
- The **Unity Hub installs and auto-updates the CLI** and this can't be turned off. Self-update:
  `unity self-update [--target <ver>]` (alias `upgrade`), `unity self-update --check --format json`.
- The July 2026 blog and the Learn tutorial show an older beta-channel installer
  (`https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.sh | UNITY_CLI_CHANNEL=beta bash`).
  The current docs use `unity.com/install.sh`.
- Compatibility: Windows 10 21H1+, macOS 14+, Linux with glibc 2.34+ (RHEL 9, Ubuntu 22.04+;
  Debian 11 is not supported).

### Auth, license, service accounts
- `unity auth login|status|logout|consumers|revoke`. Login opens a browser.
- CI: set `UNITY_SERVICE_ACCOUNT_ID` and `UNITY_SERVICE_ACCOUNT_SECRET`, or run
  `unity auth login --client-id … --secret-from-stdin`. A service account **can't** create cloud
  projects and **can't** use `license activate --personal`. Unity's generated CI activates from a
  serial (`UNITY_LICENSE_SERIAL`).
- `unity license activate` (entitlement) | `--serial` | `--personal --accept-eula` | `--floating` |
  `--file x.ulf` | `--generate-request`. `unity license return --yes`.

### Editors and modules (Quest needs Android)
- `unity install <version|lts|latest|6.3…> -m <ids> [--cm] [-c <changeset>] [-a arm64|x86_64]`,
  plus `--yes --accept-eula` for unattended runs, as in the official skill's examples.
- `unity install-modules -e <ver> -m android --cm`, `-l` to list, `--all`. This works **only for
  Editors installed by the Hub or CLI**.
- Android module IDs: `android`, `android-sdk-ndk-tools`, `android-open-jdk`. `--cm`
  (`--child-modules`) installs the children.
- `unity editors -i --format json`, `unity editors verify <ver>`, `unity modules list <ver>`,
  `unity editors module add|remove`, `unity editors upgrade`.
- Exit `9` (`INSTALL_LOCK_BUSY`, documented in the skill only) means the Hub or another CLI holds the
  install lock. Use `--no-wait` or `--wait-timeout`.

### Projects
- `unity projects create <name> --path <dir> --editor-version <v> --template <id>`. **It links a
  Unity Cloud project by default** (needs sign-in, exit 3 otherwise). Use `--no-cloud` or
  `UNITY_NO_CLOUD` for local-only. `--with-pipeline` adds `com.unity.pipeline` at creation.
- Template ids (skill, 6000.3–6000.7): `com.unity.template.urp-blank` (default 3D),
  `com.unity.template.vr`, `.mixed-reality`, `.ar-mobile`. `templates list --editor` needs a
  concrete version.
- `unity open <path>` (`--wait` on macOS/Linux), `unity close` (does **not** save),
  `unity projects info <path>`.

### Build / run / test
- `unity build <proj>` takes **one of**: `--profile <name|.asset>` (Unity 6+), `--target <desktop>`
  with `--output-path`, or `--execute-method Class.Method`. Non-desktop targets need `--profile` or
  `--execute-method`. The release notes say Meta Quest builds "only through `--profile`".
- Android flags: `--android-export-type apk|aab|android-studio-project`,
  `--android-keystore-base64`, `--android-keystore-password`, `--android-key-alias`,
  `--android-key-alias-password`, `--android-target-sdk-version`, `--android-symbol-type`,
  `--android-version-code`.
- Other build flags: `--list-targets`, `--list-profiles`, `--create-profile <target>`, `--args`,
  `--timeout` / `UNITY_BUILD_TIMEOUT`, `--allow-install`, `--versioning-strategy`,
  `--allow-dirty-build`, and a provenance manifest. `build run` launches the last build (desktop or
  WebGL only).
- For a built-in build the outcome comes from the log's verdict (exit 6 even when Unity exited 0).
  For `--execute-method`, the method's exit code decides.
- `unity run <proj> [--command <name>] -- <editor args>`: batch mode is automatic, and
  `-batchmode`, `-quit`, `-projectPath`, `-useHub`, `-hubIPC` are reserved.
- `unity test <proj> --mode EditMode|PlayMode --output … --report-format nunit|junit|nunit,junit`
  plus `--filter`, `--shard N/M`, `--retries`, `--rerun-failed`, `--affected --since`,
  `--coverage`. **Exit 8 means tests failed (don't retry). Exit 6 means no verdict (retryable).**
- `unity recompile [--strict]`: compile check against a running Editor. 0 ok, 6 errors, 7 unreachable.
- `unity ci init [--provider github|gitlab] [--target Android] [--shards n] [--dry-run]` generates a
  CI workflow.

### Output, errors, exit codes
- Formats: `human` (TTY), `tsv` (piped default!), `json` (envelope `success, command, data,
  errors, warnings`, sometimes `notifications`), `ndjson` (progress frames then one terminal
  `result` frame), `github` (Actions annotations). `--json` = `--format json`.
- Failures in json/ndjson still go to **stdout**. Branch on `errors[0].code`, not on stderr.
- Exit codes (docs): 0 ok, 1 general, 2 usage, 3 auth, 4 configuration/precondition, 6 primary
  operation failed, 7 service or Editor unreachable (retryable), 8 tests failed, 130 SIGINT,
  143 SIGTERM. The skill adds 9 (install lock busy).
- The CLI log is `cli-log.json` in the Hub logs dir (`unity logs`, `unity doctor`). The Editor log
  is a separate file (`<project>/Logs/Editor.log` or the global `Editor.log`).

### Driving a live Editor
- `unity pipeline install [--project-path]` adds `com.unity.pipeline` (experimental, Unity
  **6.0+**) to `Packages/manifest.json`. Also `pipeline list` (shows Safe Mode), `upgrade`,
  `list-versions`.
- `unity status [--until-ready] [--project-path]`. Codes: `STATUS_NO_INSTANCES`,
  `STATUS_PIPELINE_LOAD_PENDING`, `STATUS_NOT_READY`, `STATUS_ALL_UNREACHABLE`. A batch-mode Editor
  serves commands but isn't listed by `status`.
- `unity command` lists tags, `--query/--tag/--detail` filter the catalog, and
  `unity command <name> [args] [-- <cmd params>]` runs one. Use `--detach` with
  `unity job status|wait|cancel` for long jobs. `unity list` returns the tool schemas, and
  `unity commands --grep` searches the CLI's own commands.
- C#: `unity command eval '<code>'` / `unity eval '<expr>'` (top-level, added 0.1.0-beta.6),
  `eval_file`, and **`run_script --file AgentScripts/X.cs --entry X.Run`**. `run_script` compiles in
  memory with no domain reload. Unity and Meta both prefer it over `eval` for anything longer than a
  one-liner.
- `[CliCommand]` / `[CliArg]` (namespace `Unity.Pipeline.Commands`) add project-specific commands.
- Unity's guidance: before editing any scene or asset, run `unity status`. If an Editor is reachable,
  drive it rather than hand-editing `.unity`/`.prefab` YAML.

### MCP mode and skills
- `unity mcp [--project-path]` is a stdio MCP server inside the binary that exposes the connected
  Editor's commands as tools. Since beta.13 it reads `UNITY_PROJECT_PATH`.
- `unity mcp configure <client> [--local] [--dry-run] [--yes] [--server <id>]` supports 16 clients,
  including `claude` and `claude-code`. `--server issue-tracker` configures the Unity Issue Tracker MCP.
- `unity skill show|install <client>|refresh|list|disable|enable` (the command is **singular**
  `skill`). `unity setup claude|codex|grok` installs Unity's plugin through the agent's own CLI.

## 2. com.unity.pipeline (package docs v0.8)

- An Editor server on 127.0.0.1 ports **7800–7849**, and a runtime (dev Player) server on
  **7900–7949**. Requests need a bearer token from `<project>/Library/Pipeline/.unity-pipeline-port`
  (owner-only permissions). Requests with an `Origin` header (browsers) are refused.
- 156 commands in the v0.8 index, in groups: assets/files, scenes, GameObjects/components, prefabs, scripts
  (`create_script`, `attach_script`, `set_serialized_field`, `run_script`), animation,
  materials/shaders, baking, navigation/search, capture (`capture_game_view`), build
  (`build`/`build_status`, `switch_build_target`, `list_build_profiles`), tests
  (`list_tests`/`run_tests`/`test_status`), project settings (`get_*`/`set_*_settings`), UPM
  (`package_*`), Editor lifecycle (`editor_play/stop`, `menu`, `set_autotick`,
  `get_performance_stats`, `audit`), and runtime (`console`, `eval`, `reload_file`).
- **Safety convention:** destructive commands take `confirm` and `dry_run`. Without `confirm=true`
  they refuse. Scene mutations are grouped into one Undo step. Asset paths are confined to the
  authoring root (default `Assets`).
- **Safe Mode** (compile errors): the package doesn't load, so nothing connects. Recovery: run
  `unity pipeline list`, grep `Editor.log` for `error CS`, fix, restart.
- **Runtime server on a device:** it exists only in development builds, and the eval and code-reload
  code is compiled in only under `UNITY_EDITOR || (UNITY_STANDALONE && DEBUG)`. The docs mention an
  Android descriptor location (`persistentDataPath`), but the gating names standalone. **Driving a
  Quest player through Pipeline is not documented as supported.** Use Meta's `metavr` tooling for
  the device.

## 3. Official Unity skills: what's in them, and the gaps

- The 34 skills cover the CLI, Pipeline, Project Auditor, UPM, UI (uGUI, UI Toolkit, IMGUI), 2D,
  physics, audio, TMP, URP and Render Graph, BiRP→URP, Shader Graph, AI Navigation, multiplayer,
  Vivox, Localization, UGS live ops, IAP, LevelPlay ads, Web, the Asset Transformer, Graph Toolkit,
  and Unity Search.
- **Nothing on XR, OpenXR, Meta Quest, Android signing or the Horizon Store.** That is why the three
  custom skills exist.
- `implement-in-app-purchases` (Unity IAP for Google and Apple) was left out: Horizon Store
  purchases use the Meta Platform SDK (`hz-unity-platform-sdk`). `new-unity-project` was left out
  because its trigger collides with Meta's mandatory `hz-new-project-creation` flow.
- Plugin-only difference: `unity-cli/SKILL.md` in the plugin tells agents to add
  `--caller plugin --skill <name>`. Unity's own reference says those flags are analytics labels
  with no effect on behaviour.

## 4. Interop with Meta's skills (meta-quest/agentic-tools @ 3a8553d)

- `hz-unity-meta-core-sdk`, `-interaction-sdk`, `-quest-ui`, `-placement`, `-fbx-import`,
  `-face-tracking` and others say "driven against a live Editor with unity-cli". They run
  `unity status --format json` (look for `ready`), then `unity command run_script --file
  AgentScripts/*.cs --entry X.Run`, `package_add --identifier … --confirm true --wait true`,
  `recompile_status`, `get_component_properties`, `set_transform`, `save_scene`, `console`, and
  `editor_play/stop`.
- Their `references/unity-mcp-fallback.md` targets the **deprecated** in-Editor AI Assistant MCP
  (`Unity_RunCommand`) and says to ignore it when `unity-cli` is available.
- `hz-new-project-creation` requires Meta's `metavr_unity_setup` MCP tool for new Meta VR projects
  ("Do NOT run Unity CLI directly"). That tool doesn't add `com.unity.pipeline`, so
  `unity-quest-dev-setup` covers that step.
- Store numbers used in `unity-quest-android-build` agree with Meta's skills and docs: ARM64,
  min SDK 29 (32 recommended), target SDK 34 for apps created after 2026-03-01 (Meta blog updated
  2026-02-06; manifest page updated 2026-10-15), APK + OBB, v2 signing, increasing versionCode,
  `com.oculus.intent.category.VR`.
- **Trap found:** Unity's Meta Quest build profile defaults to **target API 32** (6.3 to 6.6 docs).
- **Overlap within this library:** `skills/custom/horizon-store-publish-unity` (by the Meta
  publishing researcher) owns Dashboard setup, store release builds, `ovr-platform-util` upload and
  channels. `unity-quest-android-build` owns build mechanics: dev builds, the profile and live-Editor
  routes, and troubleshooting. Each links to the other. Both build scripts read the same `QUEST_*`
  env vars, so use one script per project.

## 5. Unity MCP servers, one-line recommendations

| Server | What it is | Recommendation |
|---|---|---|
| **`unity mcp` (Unity CLI)** | stdio MCP server inside the CLI, backed by `com.unity.pipeline`. Unity 6.0+. Free, offline, localhost only | **Use this if you need MCP at all.** In Claude Code, prefer plain `unity command`/`eval` calls, which Unity calls faster and cheaper in tokens |
| Unity AI Assistant in-Editor MCP (`com.unity.ai.assistant`) | Older first-party MCP server inside the Editor | **Deprecated** by Unity in favour of the CLI. Don't adopt it. AI Assistant 2.13+ fixes a conflict with the CLI |
| [CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp) | Community bridge, MIT, about 15k stars, v10.3.0 (2026-10-04). Unity 2021.3–6.x, Python 3.10+ via `uv`. UPM git URL `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main` or OpenUPM `com.coplaydev.unity-mcp` | Consider only for **pre-Unity-6** projects, where Pipeline can't run. Otherwise it duplicates the official path |
| [IvanMurzak/Unity-MCP](https://github.com/IvanMurzak/Unity-MCP) ("AI Game Developer") | Community, Apache-2.0, about 4.5k stars, v1.0.1 (2026-10-16). OpenUPM `com.ivanmurzak.unity.mcp`, its own npm CLI `unity-mcp-cli`, editor plus runtime tools | Skip unless you need its runtime-in-player tools. Its `unity-mcp-cli` name is easy to confuse with Unity's `unity` CLI and the `unity-cli` skill |
| [Unity-Technologies/industry-ai-workflows](https://github.com/Unity-Technologies/industry-ai-workflows) | Experimental Claude Code marketplace of MCP plugins for Asset Manager, Asset Transformer (CAD) and Pipeline Automation | Not relevant to Quest game development |

### MCP config snippets (text only, not installed)

The authoritative way is to let the CLI print the exact entry without writing anything:

```bash
unity mcp configure claude-code --project-path /ABS/PATH/MyQuestGame --dry-run
```

A hand-written equivalent for Claude Code (project-scoped `.mcp.json`), the shape of a stdio server:

```json
{
  "mcpServers": {
    "unity": {
      "type": "stdio",
      "command": "unity",
      "args": ["mcp", "--project-path", "/ABS/PATH/MyQuestGame"]
    }
  }
}
```

The same through the Claude Code CLI: `claude mcp add --scope project unity -- unity mcp --project-path /ABS/PATH/MyQuestGame`.

Community servers generate their own client config from their Editor windows or CLIs. Use those
generators instead of hand-writing entries.

## 6. Open questions and things not verified

- Whether `unity build --profile … --android-*` flags override values set in a Meta Quest profile.
  The skill tells the agent to verify the APK with `aapt2` and `apksigner` instead of assuming.
- `unity build --create-profile` with a Meta Quest value isn't documented. The skill uses the
  Editor UI or Meta's setup script.
- `unity test --mode Android` for on-device PlayMode runs isn't documented (the docs say
  `EditMode|PlayMode`). The skill gives the documented raw Test Framework arguments.
- Android SDK tool paths: **checked on macOS** with the local Unity 6000.6.6f1 install. They are
  `PlaybackEngines/AndroidPlayer/SDK/build-tools/36.0.0/{aapt2,apksigner}`,
  `SDK/platform-tools/adb`, `SDK/cmdline-tools/16.0/bin/apkanalyzer` and `OpenJDK/bin/keytool`.
  Windows and Linux were not checked, so the skill still uses `find`.
- `QuestBuild.cs` and `QuestPreflight.cs` compile with 0 errors and 0 warnings against Unity
  6000.6.6f1's DLLs (checked with Unity's bundled Roslyn compiler). They have not been run in a
  project.
- An existing `unity` CLI is at `~/.unity/bin/unity`, probably put there by the Hub. It was **not
  run**: every invocation sends an anonymous telemetry ping, and the task said not to touch the
  install. `unity build --help` and `unity test --help` would settle the flag questions above.
- Pipeline command parameter names come from the project's package version (docs v0.8). The skills
  say to confirm them with `unity command --query <name> --format json`.
- Running CI on a Unity **Personal** license isn't documented.
- Unity Personal and the Horizon Store: nothing found that restricts it. Not researched further.
- Nothing was executed against a real CLI or Editor here, because nothing was installed.

## 7. Files produced by this research

- `skills/unity-official/`: 20 official Unity skills, `SOURCE.md`, `LICENSE.md`,
  `UNITY-COMPANION-LICENSE.md`.
- `skills/custom/unity-quest-dev-setup/SKILL.md`
- `skills/custom/unity-quest-android-build/`: `SKILL.md`, `templates/QuestBuild.cs`,
  `templates/QuestPreflight.cs`, `references/{signing-and-versioning,ci,troubleshooting}.md`
- `skills/custom/unity-test-runner-cli/`: `SKILL.md`, `references/test-assemblies.md`,
  `scripts/summarize_test_results.py` (tested on sample NUnit3 and JUnit XML)

## Sources

Unity CLI and Pipeline
- https://docs.unity.com/en-us/unity-cli/llms.txt
- https://docs.unity.com/en-us/unity-cli/unity-cli.md
- https://docs.unity.com/en-us/unity-cli/use-unity-cli.md
- https://docs.unity.com/en-us/unity-cli/unity-cli-reference.md
- https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli.md
- https://docs.unity.com/en-us/unity-cli/release-notes.md
- https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package.md
- https://docs.unity.com/en-us/unity-production-pipeline/local-tools-cli/unity-cli-pipeline-package.md
- https://docs.unity3d.com/Packages/com.unity.pipeline@0.8/manual/index.html (and `commands/*.html`, `connectivity.html`, `runtime-setup.html`, `safety-and-mutations.html`)
- https://docs.unity.com/en-us/hub/hub-cli-reference.md (module IDs)
- https://unity.com/blog/meet-the-unity-cli (2026-07-20)
- https://learn.unity.com/tutorial/introduction-to-unity-cli
- https://www.youtube.com/watch?v=DgNrgZeJOxQ (Unity CLI introduction video, not watched)

Official Unity skills and plugin
- https://github.com/Unity-Technologies/skills (commit 9d282100540601cfddc4bbd69f36a239d7a6f55c)
- https://github.com/Unity-Technologies/unity-agent-plugin (commit cf6b2da24e424b0a60d560a57f39f676cb6f79f3)
- https://docs.unity.com/en-us/ai/unity-plugin/about-unity-plugin.md
- https://docs.unity.com/en-us/ai/unity-plugin/claude-code.md
- https://unity.com/legal/licenses/unity-companion-license (v1.4)
- https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.18/manual/integration/unity-mcp-landing.html (deprecated MCP)

Unity Engine (Quest build, tests)
- https://docs.unity3d.com/6000.3/Documentation/Manual/xr-meta-quest-build-profile.html
- https://docs.unity3d.com/Manual/xr-meta-quest-build-profile-settings.html
- https://docs.unity.com/en-us/engine/6000.3/manual/unity-editor/command-line-arguments/editor.md
- https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/playersettings/android.md
- https://docs.unity3d.com/6000.3/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html
- https://docs.unity3d.com/6000.3/Documentation/ScriptReference/EditorUserBuildSettings-buildAppBundle.html
- https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PlayerSettings.Android-textureCompressionFormats.html
- https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction/reference-command-line.md
- https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction/running-tests/workflow-run-playmode-test-standalone.md

Meta
- https://developers.meta.com/vr/resources/publish-mobile-manifest/ (updated 2026-10-15)
- https://developers.meta.com/horizon/blog/meta-quest-apps-android-14-march-1 (2025-11-24, updated 2026-02-06)
- https://developers.meta.com/horizon/documentation/unity/ps-assets/ (OBB and asset files; the page contains embedded agent instructions, which were ignored)
- https://github.com/meta-quest/agentic-tools (local clone @ 3a8553d: `hz-unity-meta-core-sdk`, `hz-new-project-creation`, `hz-store-submit`, `hz-api-upgrade`)

Community MCP
- https://github.com/CoplayDev/unity-mcp
- https://github.com/IvanMurzak/Unity-MCP
- https://github.com/Unity-Technologies/industry-ai-workflows
