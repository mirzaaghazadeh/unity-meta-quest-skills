---
name: unity-quest-dev-setup
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.0.0"
description: Prepares a machine and a Unity project for Meta Quest development with the unity CLI. Checks or installs the CLI (with consent), installs a Unity 6 Editor with Android Build Support, SDK/NDK and OpenJDK, signs in and activates a license, adds the com.unity.pipeline package so an agent can drive the Editor, opens the project and waits until it's ready, and switches it to Android/Meta Quest. Use when setting up Unity for Quest on a new machine, when `unity` isn't found, when Android modules are missing, when `unity status` shows no Editor, or before Meta hz-unity-* skills that need a live Editor.
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
- Headset connection and developer mode → **`hz-unity-device-readiness`**, **`metavr-cli`**.
- First build → **`unity-quest-android-build`**. Store upload → **`horizon-store-publish-unity`**.
- Quest Player, XR and URP settings → **`quest-unity-project-settings`**.

## Ground rules

- **Ask before every install or account action.** That covers the CLI itself, Editors (several GB
  each), modules (accepts Android SDK EULAs), `auth login`, `license activate`, and anything that
  writes agent config (`unity mcp configure`, `unity skill install`, `unity setup claude`).
- This library already contains Unity's `unity-cli` and `unity-pipeline` skills. **Don't** run
  `unity skill install` or `unity setup claude` on top of it, or Claude ends up with duplicate skills.
- Use `--format json` for anything you parse, and branch on `success` / `errors[0].code`, not on
  text or stderr.

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
6.1, and the Pipeline package needs 6.0+. Meta XR All-in-One SDK 207.0 (2026-09-22) needs at least
**6000.0.66f2**, per the library's Meta researcher (developers.meta.com, 2026-10-16). Re-check the
current SDK's minimum before pinning an older Editor. Default to the newest **LTS** unless the
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
- **New project, CLI only** (Meta's tool unavailable, and the user agrees):
  ```bash
  unity templates list --editor <6000.x.y> --type core --format json    # needs a concrete version
  unity projects create MyQuestGame --path ~/UnityProjects --editor-version <6000.x.y> \
    --template com.unity.template.urp-blank --no-cloud --with-pipeline
  ```
  Without `--no-cloud`, `projects create` links a Unity Cloud project and needs a signed-in user.
  Then add the Meta XR SDK following `hz-unity-meta-core-sdk/references/project-setup-workflow.md`
  (it uses `unity command package_add`). Unity's `com.unity.template.vr` and `.mixed-reality`
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
and run them with `unity command run_script`. Ask the user whether to commit that folder or add it to
`.gitignore`.

## Step 6: Open, wait for ready, keep it ticking

```bash
cd <project> && unity open .
unity status --until-ready --project-path <project> --format json     # exit 6 on timeout; read errors[0].code
unity command set_autotick --project-path <project> -- --enable true   # stop an unfocused Editor stalling
unity command editor_status --project-path <project> --format json
```

If it isn't ready, rule these out in order:

| `errors[0].code` / symptom | Meaning | Do |
|---|---|---|
| `STATUS_NOT_READY` | Still opening or importing (the first open of a Meta project takes minutes) | Wait again with `--until-ready` |
| `STATUS_PIPELINE_LOAD_PENDING` | Package in manifest, not loaded yet | Ask the user to click into the Unity window, then wait again |
| No instances, and `unity pipeline list` shows Safe Mode | Compile errors | Read `<project>/Logs/Editor.log` through `grep -E 'error CS[0-9]{4}'`, fix the code, restart Unity (see `unity-cli`) |
| No instances, but the user says the Editor is open | The agent's sandbox may block the loopback connection or the discovery file | Say so plainly. Ask the user to allow 127.0.0.1 for this command or run it outside the sandbox. Never disable the sandbox, and don't silently fall back to editing files |
| `editor_status` shows `blocked_by_dialog` | A modal dialog | Ask the user to dismiss it. The CLI can't click it |

## Step 7: Platform: Android / Meta Quest

Meta's setup script switches the platform itself (Meta Quest on 6.1+, Android before that). Verify,
and switch only when the user agrees, because it re-imports the whole project:

```bash
unity command get_build_settings  --project-path <project> --format json   # activeBuildTarget
unity command list_build_profiles --project-path <project> --format json   # a Meta Quest profile?
unity command switch_build_target --project-path <project> -- --target Android --confirm true
unity command switch_build_target_status --project-path <project> --format json   # poll until completed
```

To create a **Meta Quest build profile**, the documented route is the Editor UI: File > Build
Profiles > Add Build Profile > Meta Quest. A CLI route isn't documented, so ask the user to do it
or use Meta's setup script. Unity installs the OpenXR Plugin automatically when you pick the Meta
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
- Pipeline commands (`switch_build_target`, `list_build_profiles`, `set_autotick`): https://docs.unity3d.com/Packages/com.unity.pipeline@0.8/manual/index.html
- Meta Quest build profile: https://docs.unity3d.com/6000.3/Documentation/Manual/xr-meta-quest-build-profile.html
- Meta skills `hz-new-project-creation`, `hz-unity-meta-core-sdk` (meta-quest/agentic-tools @ 3a8553d)

Last verified: 2026-10-16. Not run against a live CLI on this machine, because nothing was
installed. Confirm flags with `unity <command> --help` on first use.
