# Installing the skills

There are four ways to install. Pick **one** per machine or project. Mixing them installs the same
skills twice, and Claude then sees duplicates.

| Method | Best for | Updates |
|---|---|---|
| [A. Claude Code plugin](#a-claude-code-plugin-recommended) | Most people. Install by bundle with one command | `claude plugin update` |
| [B. `npx skills`](#b-npx-skills) | Picking individual skills; other agents (Cursor, Codex, Copilot…) | `npx skills update` |
| [C. `gh skill`](#c-gh-skill) | GitHub CLI users; pinning to a tag or commit | `gh skill update` |
| [D. `install.sh` from a clone](#d-installsh-from-a-clone) | Offline use, symlinks that track `git pull`, custom folders | `git pull` (with `--link`) |

## Before you start

You need these to **use** the skills, not to install them:

| Tool | Why | Get it |
|---|---|---|
| Claude Code | Runs the skills. v2.1.275+ for the one-step plugin install | https://code.claude.com |
| Unity 6 (6000.0.66f2 or later) with Android Build Support, SDK/NDK and OpenJDK | Needed by Meta XR SDK v203+ | Unity Hub, or `unity install <version> -m android --cm` |
| Unity CLI (`unity`) | Drives the Editor, builds, tests | `curl -fsSL https://unity.com/install.sh \| bash` or `brew install --cask unity-cli` |
| `com.unity.pipeline` in each project | Lets Claude control the open Editor | `unity pipeline install` (inside the project) |
| Meta VR CLI (`metavr`) | Headset control, logs, traces, Meta docs search, store upload | `curl -fsSL https://developers.meta.com/horizon/install-cli/ \| sh`, or `npx -y metavr` |
| Quest in Developer Mode | Testing on the device | https://developers.meta.com/vr/documentation/native/android/mobile-device-setup/ |
| Meta developer account and organization | Publishing | https://developer.meta.com |

The `unity-quest-dev-setup` skill walks Claude through all of this, asking before it installs anything.

---

## A. Claude Code plugin (recommended)

The repository is a Claude Code plugin marketplace with five bundles. They don't overlap, so you can
install any combination.

| Bundle | Skills | Always-on context | Contents |
|---|---|---|---|
| `quest-core` | 27 | ~4.2k tokens | **Start here.** Workflow router, guardrail, Unity CLI, metavr, setup, Meta VR Glasses readiness, Meta XR Core + Interaction SDKs, CC0 art sourcing, level blockout + headless capture, UI, comfort, tests, debugging, release build, VRCs, store listing, upload |
| `meta-quest-extras` | 15 | ~1.8k | MRUK, passthrough camera, face/body tracking, placement, FBX, TMP, Meta XR Operator, SDK upgrades, simpleperf, XR Simulator, Meta's submission walkthrough |
| `unity-extras` | 17 | ~1.0k | uGUI/UI Toolkit, audio, physics, NavMesh, URP, Shader Graph, Project Auditor, multiplayer, Vivox, localization |
| `community-unity-xr` | 11 | ~1.2k | XR Interaction Toolkit, Netcode, Unity 6.3 agent protocol + mobile/perf/shader/CI specialists, test design, MR UI |
| `community-design-art` | 12 | ~1.3k | XR design reference, GDD, prototyping, playtesting, trailer review, Blender asset pipeline |

The context figures come from `claude plugin details` and are added to every session while the
bundle is enabled. A skill's full text loads only when it's used.

**One step** (Claude Code 2.1.275 or later), in a Claude Code session:

```
/plugin install quest-core --marketplace mirzaaghazadeh/unity-meta-quest-skills
```

**Two steps** (any version):

```
/plugin marketplace add mirzaaghazadeh/unity-meta-quest-skills
/plugin install quest-core@unity-meta-quest-skills
```

**From your shell**, for one project only (writes the project's `.claude/settings.json`, so the
whole team gets it):

```bash
claude plugin install quest-core --marketplace mirzaaghazadeh/unity-meta-quest-skills --scope project
```

Add more bundles the same way, for example `unity-extras@unity-meta-quest-skills`. Plugin skills
show up with the bundle as a prefix, such as `quest-core:unity-cli`.

| Task | Command |
|---|---|
| See what a bundle contains and costs | `claude plugin details quest-core` |
| Update | `claude plugin marketplace update unity-meta-quest-skills`, then `claude plugin update quest-core@unity-meta-quest-skills` |
| Turn a bundle off for a while | `claude plugin disable quest-core@unity-meta-quest-skills` |
| Remove a bundle | `claude plugin uninstall quest-core@unity-meta-quest-skills` |
| Remove everything | `claude plugin marketplace remove unity-meta-quest-skills` |

## B. `npx skills`

[skills.sh](https://skills.sh)'s CLI finds all 82 skills, grouped by bundle. It needs Node.js.

```bash
# see what's available
npx skills add mirzaaghazadeh/unity-meta-quest-skills --list

# install chosen skills into this project for Claude Code
npx skills add mirzaaghazadeh/unity-meta-quest-skills -a claude-code \
  --skill quest-dev-workflow --skill hz-quest-verify-first --skill unity-cli --skill unity-quest-android-build

# everything, for every project on this machine
npx skills add mirzaaghazadeh/unity-meta-quest-skills -a claude-code -g --skill '*'
```

Projects install to `.claude/skills/`, and `-g` installs to `~/.claude/skills/`. By default it
symlinks; add `--copy` for real copies. Update with `npx skills update` and remove with
`npx skills remove <name>`.

## C. `gh skill`

The GitHub CLI's `gh skill` (in preview) finds the skills as `<group>/<name>`, for example
`custom/quest-dev-workflow` or `meta-official/hz-vr-debug`.

```bash
gh skill install mirzaaghazadeh/unity-meta-quest-skills                     # pick interactively
gh skill install mirzaaghazadeh/unity-meta-quest-skills custom/quest-dev-workflow --agent claude-code
gh skill install mirzaaghazadeh/unity-meta-quest-skills --all --agent claude-code --scope user
gh skill install mirzaaghazadeh/unity-meta-quest-skills unity-official/unity-cli --pin <tag-or-sha>
```

Update with `gh skill update`.

## D. `install.sh` from a clone

The script installs the same five bundles as the plugin (it reads `.claude-plugin/marketplace.json`).
It needs bash and Python 3, which come with macOS Command Line Tools and with most Linux systems.

```bash
git clone https://github.com/mirzaaghazadeh/unity-meta-quest-skills.git
cd unity-meta-quest-skills

./install.sh --list                                        # bundles and skills
./install.sh --project ~/Games/MyQuestGame quest-core      # into MyQuestGame/.claude/skills
./install.sh --user quest-core unity-extras                # into ~/.claude/skills
./install.sh --project ~/Games/MyQuestGame --link quest-core   # symlinks: `git pull` updates them
./install.sh --dest ~/Games/MyQuestGame/.agents/skills all     # another agent's folder
./install.sh --project ~/Games/MyQuestGame --dry-run all   # preview, change nothing
./install.sh --project ~/Games/MyQuestGame --uninstall community-design-art
```

Existing skills are skipped unless you pass `--force`. Any single skill name works in place of a
bundle, for example `./install.sh --user scenario-unity-shaders scenario-unity-expert`.

## E. By hand

Copy skill folders into `.claude/skills/` (one project) or `~/.claude/skills/` (every project).
Keep each folder's name exactly as it is: it must match the skill's `name:`. Keep `LICENSE`,
`LICENSE.md` and `NOTICE` files with the folders you copy.

---

## Avoid duplicates

- Don't combine Meta's own `meta-vr` plugin, `metavr init` or `metavr skills` with this library:
  all of them install the same Meta skills. The same goes for Unity's `unity@unity-agent-plugin`
  plugin and `unity skill install`.
- If you do want Unity's always-current `unity-cli` skill from the CLI itself, remove
  `unity-cli` from this library's install, or keep both and accept the duplicate.
- `scenario-unity-*` specialists need `scenario-unity-expert` next to them, and the
  `scenario-blender-*` ones need `scenario-blender-expert`. The bundles keep them together. If you
  pick single skills, install the lead skill too.

## Troubleshooting

| Problem | Fix |
|---|---|
| Skills don't appear | Start a new session or run `/reload-plugins`. Check that `ls .claude/skills` (or `claude plugin list`) shows them |
| "Plugin not found in marketplace" | Use the bundle name before `@`, for example `quest-core@unity-meta-quest-skills` |
| Repository not found (while it's private) | You need read access to the repo, and `gh auth login` or git credentials that can reach it |
| `unity: command not found` | Install the Unity CLI (see the table at the top), then open a new terminal |
| `unity status` shows no Editor | Open the project in Unity 6, run `unity pipeline install` once, wait for state `ready`. Compile errors put the Editor in Safe Mode, which disables control: fix them first |
| `metavr device list` is empty | Enable Developer Mode, connect over USB, accept the "Allow USB debugging" prompt in the headset |
