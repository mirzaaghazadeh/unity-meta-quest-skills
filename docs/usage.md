# Using the skills

You don't call skills by hand. Describe what you want, and Claude picks the matching skill from its
description. Naming a skill ("use horizon-vrc-checklist") makes the choice explicit. Plugin installs
also let you type it as a slash command, for example `/quest-core:quest-dev-workflow`.

## First session

Open Claude Code in your Unity project folder (or an empty folder for a new game) and start with:

> Use quest-dev-workflow to plan my Quest game. It's a [one-line pitch]. Check my machine first.

The `quest-dev-workflow` skill lays out the path below, and `hz-quest-verify-first` makes Claude
check Quest-specific facts against Meta's live docs instead of its memory.

## The path, as prompts

Copy the prompts and adapt them. Each row says which skill does the work.

| # | Phase | Example prompt | Skills that pick it up |
|---|---|---|---|
| 0 | Plan | "Help me write a one-page GDD for a seated VR puzzle game, then a 1-hour prototype plan to test if the core mechanic is fun." | `game-gdd-author`, `prototype-fast`, `hz-immersive-designer` |
| 0 | Launch plan | "What decisions can't I undo before my first store upload? Draft a launch timeline for a March release." | `horizon-launch-roadmap` |
| 1 | Machine | "Check that my Mac can build for Quest: Unity CLI, a Unity 6 Editor with Android modules, metavr. Ask before installing anything." | `unity-quest-dev-setup`, `metavr-cli` |
| 1 | Project | "Create a new Unity 6 Quest project called LanternPuzzle with URP and the Meta XR SDK." | `hz-new-project-creation`, `unity-cli` |
| 1 | Settings | "Audit this project's Quest settings and fix anything that would fail the store." | `quest-unity-project-settings`, `hz-unity-meta-core-sdk` |
| 2 | Blockout | "Block out the level from a JSON file with gaps of 2–5 m, validate it, and render screenshots plus the Quest 3 eye view." | `quest-level-blockout` |
| 2 | Art | "Find free CC0 models for the enemies and props, download what can be scripted, and keep CREDITS.md." | `quest-asset-sourcing` |
| 2 | Gameplay | "Add hand-tracked grabbing and teleport locomotion with snap turn." | `hz-unity-meta-interaction-sdk` |
| 2 | Mixed reality | "Spawn the lanterns on real tables in the room using scene data." | `hz-unity-meta-mixed-reality-utility-kit`, `hz-unity-placement` |
| 2 | UI | "Make a world-space main menu that's readable at 1.5 m and works with hands and controllers." | `hz-unity-meta-quest-ui`, `hz-unity-tmp-resources` |
| 3 | No headset | "Set up the Meta XR Simulator so I can test grabbing without the Quest." | `hz-xr-simulator-install-and-configure`, `hz-xr-simulator-control` |
| 4 | Tests | "Add EditMode tests for the puzzle logic and run them from the CLI." | `unity-test-runner-cli`, `test-designing-guide` |
| 5 | On device | "Install a dev build on my Quest, launch it and show me any errors in the log." | `unity-quest-android-build`, `hz-vr-debug` |
| 5 | VR Glasses | "Make my game work on Meta VR Glasses without controllers, scan it for problems and test it in the simulator." | `meta-vr-glasses-unity`, `hz-unity-device-readiness` |
| 5 | Agent playtest | "Set up Meta XR Operator so you can play my game in the simulator and check the menu works." | `meta-xr-operator-unity` |
| 5 | Performance | "It stutters when the lanterns light up. Capture a trace on the headset and tell me why." | `hz-perfetto-debug`, `hz-unity-code-review`, `scenario-unity-performance` |
| 6 | Release build | "Make a signed release APK with target API 34 and check it." | `unity-quest-android-build` |
| 7 | Store checks | "Run the VRC audit on this project and list what fails, by VRC ID." | `horizon-vrc-checklist` |
| 7 | Platform | "Add an entitlement check and three achievements." | `hz-unity-platform-sdk` |
| 8 | Listing | "Check my store art in ./store-assets against Meta's specs and draft the store description." | `horizon-store-listing-assets` |
| 9 | Upload | "Upload build 1.0.3 to the ALPHA channel with these release notes." | `horizon-store-publish-unity` |

## What stays your job

Claude asks before anything that's hard to undo or public. Some steps only you can do:

- **Developer Dashboard**: create the organization and app, fill in the listing, answer the age
  rating (IARC) and Data Use Checkup questionnaires, and press **Submit for Review**. The CLI
  uploads builds but doesn't submit.
- **Keystore**: create it once, back it up somewhere safe, and keep the passwords in environment
  variables or a password manager. If you lose the keystore you can never update the app.
- **Approvals**: uploads to release channels, `--publish`, the production channel, installing tools,
  and changes to Claude or MCP configuration all need your yes.
- **The headset**: put it on. Comfort, readability and real frame rate only show in the device.

## Tips

- **Keep the Editor open** with `com.unity.pipeline` installed. Claude then edits scenes through
  `unity command` instead of hand-editing YAML, and can read the console.
- **Compile errors block everything.** An Editor with compile errors starts in Safe Mode, where
  Claude can't control it. Ask Claude to fix the errors first.
- **Keep the context small.** Enable `quest-core` for daily work and add other bundles when a
  phase needs them (`claude plugin enable` / `disable`).
- **Pick one interaction stack**: Meta Interaction SDK (`hz-unity-meta-interaction-sdk`) or XR
  Interaction Toolkit (`unity-xr`), not both.
- **When values disagree**, the `horizon-*` skills hold Meta's specs checked on 2026-10-16, and
  `skills/meta-official/ERRATA.md` lists where Meta's own skills are behind. Ask Claude to check
  with `metavr docs search` when in doubt.
- **Optional MCP**: the skills call `unity` and `metavr` directly. If you want them as MCP tools
  too, see `mcp/README.md`.
