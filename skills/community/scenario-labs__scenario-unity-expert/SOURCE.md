# Source: scenario-unity-expert

- **Repo:** https://github.com/scenario-labs/skills (ported from edemaistre/unity-expert-skills v0.1)
- **Path:** `skills/game-engines/unity/scenario-unity-expert/`
- **Commit:** `49dd26e47371163ae0020c7f5627d0993626bfc2` (2026-10-15)
- **License:** MIT, Copyright 2026 Scenario Inc. (`LICENSE` copied from the repo root)
- **Stars at fetch:** 1032
- **Fetched:** 2026-10-16

## Why it was picked

This is the lead "technical director and agent protocol" skill for Unity 6.3 LTS (6000.3) on macOS, which is this user's stack. It is the best agent-operations skill found for Unity:
- **Channel choice:** one-shot batch `-executeMethod` jobs, a resident headless editor with a file-based bridge, the user's GUI editor, or the **official Unity CLI with `com.unity.pipeline`** (`unity command`, `eval`, `status`, `test`). It covers the CLI's Safe Mode, sandbox and AMBIGUOUS_EDITOR failure modes.
- **Habits:** measure-then-look quality gates, and a list of Unity 6.3 traps.
- **Toolkit:** the shared Python toolkit (`ut_env`, `ut_run`, `ut_live`, `ut_review`, `ut_stat`) and the C# AgentKit that the other `scenario-unity-*` skills import.

The author reports every procedure was run live in 6000.3.21f1, with blind-graded gains of 63% to 97% on written briefs.

## Security review

- I read the SKILL.md and every reference.
- I skimmed all 5 Python modules and 7 C# files for network, process and deletion calls.
- **Python subprocess calls:** the Unity editor, `ps`, `cp -cR` (APFS clone of a base project), `xcodebuild -version`, and the `unity` CLI. There are no HTTP calls and no downloads.
- **Home directory:** it only reads the Unity license file (`~/Library/Unity/licenses/...`) and looks for the CLI at `~/.unity/bin/unity`.
- **`AgentBridge.cs`:** an `[InitializeOnLoad]` editor class that polls `<project>/Library/AgentKit/bridge/inbox/*.json` and runs the static methods named there. It opens no socket and runs as the local user. Anything that can write to your project's `Library/` folder can trigger editor methods while the bridge is installed. It is acceptable for local use. Remove `Assets/Editor/AgentKit/` from shipped or shared projects if that matters to you.
- `ut_live.eval` writes temporary C# into `Assets/Editor/AgentEval/` and archives it to `Library/`.
- `references/live-channels.md` documents Unity's official CLI installer (`curl ... public-cdn.cloud.unity3d.com ... | bash`), but the skill does not run it. It deliberately avoided editing shell rc files.

## Caveats

- **Install folder name matters.** The specialist skills find this one as a sibling folder named exactly `scenario-unity-expert`. When installing, drop the `scenario-labs__` prefix.
- **Tested on macOS desktop players, not Quest.** The numbers are Metal/Apple Silicon. For Quest (Android, Vulkan, Adreno), use `scenario-labs__scenario-unity-mobile`, measure in a development build on the headset, and follow Meta's performance guidance.
- `install_agentkit()` copies C# into `<project>/Assets/Editor/AgentKit/`. That is a project change, so commit or review it.
- `base_project()` and some references point to the author's machine (`tests/projects/...`, `archive/...`). Those paths are provenance only and are not shipped.
- It pins CLI 1.0.0-beta.11 and `com.unity.pipeline` 0.7.0-exp.1 observations. Re-probe your installed CLI.
- It overlaps with Unity's official `unity-cli` skill (collected separately). This skill builds on that one rather than replacing it.
