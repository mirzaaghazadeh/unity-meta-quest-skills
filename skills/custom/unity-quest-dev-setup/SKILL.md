---
name: unity-quest-dev-setup
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.3.0"
description: Prepares a machine and a Unity project for Meta Quest and Meta VR Glasses development with the unity CLI. Checks or installs the CLI (with consent), installs a Unity 6 Editor (6000.0.66f2+ for Meta XR SDK v207) with Android Build Support, SDK/NDK and OpenJDK, signs in and activates a license, adds the com.unity.pipeline package so an agent can drive the Editor, opens the project and waits until it's ready, switches it to Android/Meta Quest, and covers the optional MCP setup (unity, metavr, Meta's AI Tools panel) with its known Claude Code issue. Use when setting up Unity for Quest on a new machine, when `unity` isn't found, when Android modules are missing, when `unity status` shows no Editor, when metavr tools don't load in Claude Code, or before Meta hz-unity-* skills that need a live Editor.
---

# Unity + Quest development setup (CLI)

> Made by [Navid Mirzaaghazadeh](https://github.com/mirzaaghazadeh) · part of [unity-meta-quest-skills](https://github.com/mirzaaghazadeh/unity-meta-quest-skills) · MIT

Gets a machine and a project to the state Meta's `hz-unity-*` skills and the official `unity-cli`
/ `unity-pipeline` skills expect: **a Unity 6 Editor with Android support, the project open, and
`unity status` reporting `ready`.**

Hand-offs:
- Creating a **new Meta VR project** → **`hz-new-project-creation`** (Meta says to use its
  `metavr_unity_setup` tool rather than creating the project with the Unity CLI). Come back here at
  Step 4.
- CLI details (every flag, exit codes, Safe Mode, sandbox caveats) → **`unity-cli`**.
- Headset connection and developer mode → **`metavr-cli`**.
- Meta VR Glasses readiness (eyes and hands, FoV) → **`meta-vr-glasses-unity`**,
  **`hz-unity-device-readiness`**. No headset → Meta XR Simulator
  (**`hz-xr-simulator-install-and-configure`**; with Core 207 use the standalone app from
  `metavr tools install xrsim`, not the old `com.meta.xr.simulator` package).
- Letting the agent drive the running app → **`meta-xr-operator-unity`**.
- First build → **`unity-quest-android-build`**. Store upload → **`horizon-store-publish-unity`**.
- Quest Player, XR and URP settings → **`quest-unity-project-settings`**.
- Running Editor scripts **without** a live Editor (`unity run … -executeMethod`, raw
  `-batchmode`, CI) → read **[references/batch-mode-traps.md](references/batch-mode-traps.md)**
  first: imports and Meta's Fix All don't finish in a `-quit` run, OpenXR validation needs a second
  run, plus disk-space and capture traps found in a real project.

## Ground rules

- **Ask before every install or account action.** That covers the CLI itself, Editors (several GB
  each), modules (accepts Android SDK EULAs), `auth login`, `license activate`, and anything that
  writes agent config (`unity mcp configure`, `unity skill install`, `unity setup claude`).
- This library already contains Unity's `unity-cli` and `unity-pipeline` skills. **Don't** run
  `unity skill install` or `unity setup claude` on top of it, or Claude ends up with duplicate skills.
- Use `--format json` for anything you parse, and branch on `success` / `errors[0].code`, not on
  text or stderr.
- **Meta's AI Tools panel** (Meta XR Core SDK 207: Window > Meta > Meta XR SDK > AI Tools) installs
  metavr and Meta XR Operator, registers an MCP server for Claude Code (local scope in
  `~/.claude.json`) and can copy skills into `<project>/.claude/skills`. Its Agent Bridge (Device
  Readiness AI check, Immersive Debugger assistant) launches Claude Code with
  `--dangerously-skip-permissions` (Core 207.0.1 source). Treat each of these as an install or
  config change: explain it and ask first. Skills it copies can duplicate ones from this library.

## Step 1: The `unity` CLI

```bash
which unity && unity --version
unity self-update --check --format json      # the official skill says to stay on the latest CLI
```

If it's missing, present the options and let the user pick and run one. The Unity Hub also installs
and updates the CLI automatically.

| OS | Command (from Unity's docs) |
|---|---|
| macOS / Linux | `curl -fsSL https://unity.com/install.sh \| bash`, or `brew install --cask unity-cli` |
| Windows | `irm https://unity.com/install.ps1 \| iex`, or `winget install Unity.CLI` |

Requirements: macOS 14+, Windows 10 21H1+, or Linux with glibc 2.34+ (RHEL 9, Ubuntu 22.04+). After
installing, open a **new** shell and check `unity --version`. Don't mix the script install and
Homebrew: `unity self-uninstall -y` removes the script copy.

## Step 2: Editor and Android modules

Version choice: Meta recommends **Unity 6.1 or later**. The Meta Quest build profile exists from
6.1, and the Pipeline package needs 6.0+. Meta XR SDK v207 needs at least **6000.0.66f2**: the
All-in-One 207.0 page (2026-09-22), Meta's Support Meta VR Glasses page (2026-09-30) and Core
207.0.1's `package.json` (`"unity": "6000.0"`, `"unityRelease": "66f2"`) agree. Meta XR Operator
also needs the Unity OpenXR Plugin 1.17.0+. Re-check the current SDK's minimum before pinning an
older Editor. Default to the newest **LTS** unless the
project pins a version. For XR, use the **Unity OpenXR Plugin**; Meta lists the Oculus XR Plugin as
deprecated. Detailed Player, XR and URP values are in **`quest-unity-project-settings`**.

```bash
unity projects info <project> --format json          # existing project? use its editorVersion
unity releases --stream lts --limit 5 --format json  # otherwise pick from here
unity editors --installed --format json              # what's already installed (and each "location")
```

Install with Android Build Support and its child modules, Android SDK & NDK Tools
(`android-sdk-ndk-tools`) and OpenJDK (`android-open-jdk`). `--cm` pulls them in:

```bash
unity install <version> -m android --cm --yes --accept-eula        # new Editor (ask first)
unity install-modules -e <version> -m android --cm                 # Editor already installed
unity editors verify <version>                                      # structural check; prints repair commands
```

- Use `--format ndjson` for streamed progress on long installs. Exit `9` (`INSTALL_LOCK_BUSY`)
  means the Hub or another CLI is installing. Retry later, or bound the wait with `--wait-timeout <s>`.
- Modules can only be added to Editors the Hub or CLI installed. A hand-installed Editor has to
  be reinstalled through them first.
- macOS: `-a arm64` / `-a x86_64` selects Apple silicon or Intel builds.
- **Disk:** the Android module alone was 5.4 GB installed (6000.6.6f1), and a low-disk install
  can abort halfway. Keep at least 25 GB free; recovery steps are in
  [references/batch-mode-traps.md](references/batch-mode-traps.md) §5 (observed 2026-10-16).

## Step 3: Sign-in and license

```bash
unity auth status --format json        # signed out → the user runs: unity auth login (browser)
unity license status --format json     # none active → see below
```

`unity license activate` uses the signed-in user's subscription. Add `--personal --accept-eula` for
Unity Personal, but ask the user first because it accepts a EULA. CI uses a service account and a
serial instead (see `unity-quest-android-build/references/ci.md`). Exit `3` means sign in again,
and `4` means a precondition such as a license is missing.

## Step 4: The project

- **New Meta VR project:** follow `hz-new-project-creation`, which creates and configures it.
  Then continue with Step 5. Meta's tool doesn't add the Pipeline package, but the `unity-cli`
  driven skills need it.
  - Observed 2026-10-24 (metavr 1.8.0.17.10): `metavr_unity_setup` also has an `apply` action
    that runs the whole setup headlessly and waits; the skills only describe `inject`.
  - Its bootstrap project had **no URP pipeline asset**: GraphicsSettings and every quality level
    were null, so the Built-in pipeline was active. Fix: copy `Mobile_RPAsset` and
    `Mobile_Renderer` out of the Editor's `com.unity.template.urp-blank-*.tgz` and assign them
    (`quest-unity-project-settings`). The tell is that settings audit reporting `quality.msaa`
    instead of `urp.msaa`.
  - It pinned OpenXR 1.16.1; Meta XR Operator needs 1.17 or later (`meta-xr-operator-unity`).
- **New project, CLI only** (Meta's tool unavailable, and the user agrees):
  ```bash
  unity templates list --editor <6000.x.y> --type core --format json    # needs a concrete version
  unity projects create MyQuestGame --path ~/UnityProjects --editor-version <6000.x.y> \
    --template com.unity.template.urp-blank --no-cloud --with-pipeline
  ```
  Without `--no-cloud`, `projects create` links a Unity Cloud project and needs a signed-in user.
  Then add the Meta XR SDK following `hz-unity-meta-core-sdk/references/project-setup-workflow.md`
  (it uses `unity command package_add`, which Pipeline 0.9 calls `package add`). Unity's `com.unity.template.vr` and `.mixed-reality`
  templates also exist, but Meta's own flow starts from URP blank.
- **Existing project:** check that `editorVersion` from `unity projects info` is installed (Step 2).

## Step 5: Pipeline package (live-Editor control)

```bash
grep -q '"com.unity.pipeline"' <project>/Packages/manifest.json || unity pipeline install --project-path <project>
```

Install it **before** opening the project. An already-open Editor picks up the change only on its
next refresh, and until then `unity status` reports `STATUS_PIPELINE_LOAD_PENDING`. It's an
experimental package, resolved from the Unity registry, and serves only on 127.0.0.1. Commit the
`Packages/manifest.json` change.

Meta's skills write helper scripts to `<project>/AgentScripts/` (outside `Assets/`, so no reimport)
and run them with `unity command run_script` (`script run` in Pipeline 0.9). Ask the user whether
to commit that folder or add it to `.gitignore`.

**Command names depend on the Pipeline version.** Pipeline 0.9 (0.9.1-exp.1) names commands
`<group> <name>`: `script run`, `script eval`, `editor status`, `editor set_auto_tick`,
`build list_profiles`, `build.target switch`, `package add`. Pipeline 0.8 and Meta's and Unity's
skills use the older single names (`run_script`, `editor_status`, `set_autotick`,
`list_build_profiles`, `switch_build_target`, `package_add`). Check with `unity command --tag
<group> --project-path <project>`; the full table is in `skills/meta-official/ERRATA.md`. This
skill uses the 0.9 names.

## Step 6: Open, wait for ready, keep it ticking

```bash
cd <project> && unity open .
unity status --until-ready --project-path <project> --format json     # exit 6 on timeout; read errors[0].code
unity command editor set_auto_tick --project-path <project> -- --enable true   # stop an unfocused Editor stalling
unity command editor status --project-path <project> --format json
```

If it isn't ready, rule these out in order:

| `errors[0].code` / symptom | Meaning | Do |
|---|---|---|
| `STATUS_NOT_READY` | Still opening or importing (the first open of a Meta project takes minutes) | Wait again with `--until-ready` |
| `STATUS_PIPELINE_LOAD_PENDING` | Package in manifest, not loaded yet | Ask the user to click into the Unity window, then wait again |
| No instances, and `unity pipeline list` shows Safe Mode | Compile errors | Read `<project>/Logs/Editor.log` through `grep -E 'error CS[0-9]{4}'`, fix the code, restart Unity (see `unity-cli`) |
| No instances, but the user says the Editor is open | The agent's sandbox may block the loopback connection or the discovery file | Say so plainly. Ask the user to allow 127.0.0.1 for this command or run it outside the sandbox. Never disable the sandbox, and don't silently fall back to editing files |
| `editor status` shows `blocked_by_dialog` | A modal dialog | Ask the user to dismiss it. The CLI can't click it |
| Calls time out, `editor status` too | A modal dialog the Pipeline can't see (observed 2026-10-25: a build decision dialog) | `sample <Unity pid>` on macOS shows `NSAlert runModal`. Ask the user, or close the Editor and work headless (`unity-quest-android-build`) |

## Step 7: Platform: Android / Meta Quest

Meta's setup script switches the platform itself (Meta Quest on 6.1+, Android before that). Verify,
and switch only when the user agrees, because it re-imports the whole project:

```bash
unity command build get_settings  --project-path <project> --format json   # activeBuildTarget
unity command build list_profiles --project-path <project> --format json   # a Meta Quest profile?
unity command build.target switch --project-path <project> -- --target Android --confirm true
unity command build.target switch_status --project-path <project> --format json   # poll until completed
```

To create a **Meta Quest build profile**, the documented route is the Editor UI: File > Build
Profiles > Add Build Profile > Meta Quest. Pipeline 0.9 also lists `build create_profile`, which
created one without the UI in a 2026-10-24 project (unverified beyond that); otherwise ask the
user or use Meta's setup script. Unity installs the OpenXR Plugin automatically when you pick the Meta
Quest platform.

## Step 8 (optional): MCP instead of shell commands

Claude Code can run shell commands, so prefer `unity command` / `unity eval` directly. Unity says
they are "faster and use fewer tokens than MCP". Unity's in-Editor AI Assistant MCP server is
deprecated, and the CLI's own `unity mcp` stdio server replaces it. Set up MCP only if the user
asks, and preview first:

```bash
unity mcp configure --list                                              # supported clients, current state
unity mcp configure claude-code --project-path <project> --dry-run      # shows the entry it would write
```

**metavr MCP in Claude Code (known issue, 2026-10-16).** With metavr 1.8.0.17.10 and Claude Code
2.1.301, `claude mcp get metavr` shows `Connected · tools fetch failed` (`ttlMs` / `cacheScope` in
the error). metavr advertises MCP protocol `2026-07-28`, which Claude Code uses, but omits those two
required fields from its list results. Route it through
[scripts/metavr_mcp_relay.py](scripts/metavr_mcp_relay.py) (standard library; adds the fields with
no caching, passes everything else through), with the user's OK since it edits Claude config:

```bash
claude mcp add metavr -- python3 -I <this skill>/scripts/metavr_mcp_relay.py ~/.metavr/bin/metavr mcp server --update
claude mcp get metavr      # expect ✔ Connected; tools load in the next Claude Code session
```

Re-check without the relay after metavr or Claude Code updates. Note that `metavr mcp install
claude-code` only prints the `claude mcp add` command unless you pass `--execute`. npm metavr
1.8.1 (2026-10-01) changed only the npm wrapper and still installs binary 1.8.0.17.10, so the
issue should still apply (not re-tested). Meta XR Operator's tools are federated into this same
`metavr` server by default, so they fail to load the same way; `meta-xr-operator-unity` has a
route that needs no MCP registration.

## Final check

```bash
unity doctor --format json        # auth, installed Editors, environment health in one snapshot
```

Report: CLI version, Editor version, Android modules ok, license ok, Pipeline installed, Editor
`ready`, active build target, and any notification in the JSON envelopes'
`notifications` array, such as a CLI update. When a notification has
`remediation.requiresUserApproval: true`, show its command to the user rather than running it.

## Sources

- Use the Unity CLI (install, modules, projects, auth, service accounts): https://docs.unity.com/en-us/unity-cli/use-unity-cli.md
- Unity CLI reference (commands, `--cm`, exit codes, env vars): https://docs.unity.com/en-us/unity-cli/unity-cli-reference.md
- Hub CLI module IDs (`android`, `android-sdk-ndk-tools`, `android-open-jdk`): https://docs.unity.com/en-us/hub/hub-cli-reference.md
- Pipeline package install: https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package.md
- Replace MCP Server with Unity CLI: https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli.md
- Official `unity-cli` skill (status codes, Safe Mode, sandbox guidance, `--with-pipeline`): https://github.com/Unity-Technologies/skills/tree/main/skills/unity-cli
- Pipeline commands, 0.8 names (`switch_build_target`, `list_build_profiles`, `set_autotick`): https://docs.unity3d.com/Packages/com.unity.pipeline@0.8/manual/index.html
- Pipeline 0.9.1-exp.1 commands (`<group> <name>`: `script run`, `editor status`, `build.target switch`, `package add`): https://docs.unity3d.com/Packages/com.unity.pipeline@0.9/manual/index.html (read 2026-10-25)
- Meta Quest build profile: https://docs.unity3d.com/6000.3/Documentation/Manual/xr-meta-quest-build-profile.html
- Meta skills `hz-new-project-creation`, `hz-unity-meta-core-sdk` (meta-quest/agentic-tools @ 3a8553d)
- Meta XR All-in-One SDK 207.0: https://developers.meta.com/vr/downloads/package/meta-xr-sdk-all-in-one-upm/ (2026-09-22)
- Support Meta VR Glasses (Unity): https://developers.meta.com/vr/documentation/unity/unity-support-meta-vr-glasses/ (2026-09-30)
- Meta XR Operator getting started: https://developers.meta.com/vr/documentation/unity/meta-xr-operator/getting-started/ (2026-09-09)
- Core SDK 207.0.1 source read 2026-10-17: `package.json`, `Editor/AgentBridge/ClaudeCodeService.cs`, `Editor/MetaXROperator/MetaXROperatorProvider.cs`

Last verified: 2026-10-25 (the Pipeline 0.9 command names, Steps 6-7, against a live unity CLI
1.0.0-beta.13 and Pipeline 0.9.1-exp.1 in a Unity 6000.6.6f1 project); 2026-10-17 (the rest).
Confirm flags with `unity <command> --help` on first use. The batch-mode traps reference was
observed in a real Unity 6000.6.6f1 + Meta XR Core SDK 207.0.1 project on macOS.
