---
name: quest-dev-workflow
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.0.0"
description: >-
  Start-here map for making a Meta Quest VR game in Unity with Claude Code and
  shipping it on the Meta Horizon Store. Gives the end-to-end order (verify,
  set up, build the game, test on the headset, release build, VRC audit, store
  listing, upload, submit), names the skill that handles each step, and lists
  the hard rules (confirm before any store upload or submission, never commit
  keystores, target API 34). Use when starting or planning a Quest Unity
  project, when unsure which skill fits a task, when the user asks "what's
  next", "where do I start" or "how do I ship this", or at the start of any
  multi-step Quest task.
---

# Quest game workflow: start here

> Made by [Navid Mirzaaghazadeh](https://github.com/mirzaaghazadeh) · part of [unity-meta-quest-skills](https://github.com/mirzaaghazadeh/unity-meta-quest-skills) · MIT

This skill only routes. The detailed steps live in the skills it names. Load the named skill
before acting on a step.

- **Plugin installs prefix skill names**, for example `quest-core:unity-cli`. Match on the part
  after the colon.
- **If a named skill isn't installed**, say so and name the bundle that has it (see
  [Bundles](#bundles)). Don't improvise its content from memory.

## 1. Before anything Quest-specific

1. Load **`hz-quest-verify-first`**. Quest APIs, manifest rules and store requirements change
   often, so check claims against Meta's live docs (`metavr docs search "<topic>"`), not memory.
2. Check the tools, and ask before installing anything that's missing:
   ```bash
   unity --version && unity status     # Unity CLI; an open Editor should show state "ready"
   metavr --version && metavr device list
   ```
   Missing tools → **`unity-quest-dev-setup`** (Unity CLI, Editor, Android modules) and
   **`metavr-cli`** (Meta VR CLI).
3. Prefer driving the open Editor (**`unity-cli`**, **`unity-pipeline`**: `unity command
   run_script` / `eval`) over hand-editing `.unity`, `.prefab` or `.asset` YAML.

## 2. The path from idea to store

| # | Phase | Skills, in order | Done when |
|---|---|---|---|
| 0 | Plan | `game-gdd-author`, `prototype-fast`, `hz-immersive-designer`, `horizon-launch-roadmap` | Core loop proven in the headset. Package name and comfort target decided |
| 1 | Set up | `unity-quest-dev-setup` → `hz-new-project-creation` → `quest-unity-project-settings` → `hz-unity-meta-core-sdk` | Empty scene runs on the Quest; settings audit passes |
| 2 | Build the game | `hz-unity-meta-interaction-sdk` (or `unity-xr` for XR Interaction Toolkit: **pick one**), `hz-unity-meta-quest-ui`, `hz-unity-meta-mixed-reality-utility-kit`, `physics-3d-collision`, `unity-package-management` | Features work in the Editor and the XR Simulator |
| 3 | Iterate without a headset | `hz-xr-simulator-install-and-configure`, `hz-xr-simulator-control` | Simulator session reproduces the feature |
| 4 | Test | `unity-test-runner-cli` (+ `test-designing-guide`), `game-playtest` | EditMode/PlayMode tests green; playtest notes logged |
| 5 | Debug and performance | `hz-vr-debug`, `hz-unity-code-review`, `hz-perfetto-debug`, `hz-simpleperf-debug`, `scenario-unity-performance`, `project-auditor-fixes` | Holds the frame rate on the lowest target device |
| 6 | Release build | `unity-quest-android-build` (standard script: `templates/QuestBuild.cs`) | Signed ARM64 APK passes the APK checks |
| 7 | Store readiness | `horizon-vrc-checklist`, `hz-unity-platform-sdk` (entitlement, achievements, IAP) | Every applicable VRC passes on the device |
| 8 | Store listing | `horizon-store-listing-assets`, `review-game-trailer` | Art, copy, rating and privacy policy ready |
| 9 | Upload and submit | `horizon-store-publish-unity` (ALPHA/BETA first), then Dashboard submission (`hz-store-submit` for the walkthrough) | Build on a test channel, then submitted by the user |

## 3. Hard rules

- **Ask first** before you upload to any release channel, use `--publish`, touch the LIVE or
  production channel, submit for review, install a tool, or change Claude or MCP config. Store
  submission itself happens in the Developer Dashboard, done by the user.
- **Never commit or print secrets.** Keystore passwords and app secrets come from environment
  variables. Remind the user to back up the keystore: without it the app can never be updated.
- **Packaging baseline:** IL2CPP, ARM64 only, `targetSdkVersion` **34** set explicitly (Unity's
  Meta Quest profile defaults to 32), and a version code that always increases.
- **Conflicting values:** Meta's `hz-store-submit` has outdated store specs (see `ERRATA.md` in
  the repo). The `horizon-*` skills hold the values checked on 2026-10-16. When in doubt, run
  `metavr docs search`.
- **One of each:** one interaction stack (Meta Interaction SDK *or* XRI), one build script, one
  verify-first skill (`hz-quest-verify-first`).
- **Prove it on the device.** Editor Play mode is not proof. Test on the headset or in the
  simulator, and read the logs (`hz-vr-debug`).

## 4. Quick router

| The user wants to… | Use |
|---|---|
| install or fix the Unity CLI, Editors, Android modules | `unity-quest-dev-setup`, `unity-cli` |
| create a new Quest project | `hz-new-project-creation` |
| check or fix project settings | `quest-unity-project-settings` |
| add a camera rig, passthrough, hand tracking, anchors | `hz-unity-meta-core-sdk` |
| grab, poke, ray, teleport, locomotion | `hz-unity-meta-interaction-sdk` |
| place things in the real room (mixed reality) | `hz-unity-meta-mixed-reality-utility-kit`, `hz-unity-placement` |
| build VR menus and text | `hz-unity-meta-quest-ui`, `hz-unity-tmp-resources`, `unity-xr-mr-ui-director` |
| import a 3D model | `hz-unity-fbx-import`; Blender work: `scenario-blender-expert` |
| write shaders that work in both eyes | `scenario-unity-shaders` |
| add multiplayer or voice | `setup-multiplayer-services`, `unity-multiplayer`, `setup-vivox-voice-chat` |
| add achievements, leaderboards, IAP, entitlement | `hz-unity-platform-sdk` |
| fix comfort or motion sickness | `hz-immersive-designer`, `xr-platforms-audio-and-comfort` |
| find why the game stutters | `hz-perfetto-debug`, `hz-unity-code-review`, `scenario-unity-performance` |
| see headset logs, screenshots, crashes | `hz-vr-debug` |
| run tests | `unity-test-runner-cli` |
| build an APK | `unity-quest-android-build` |
| check store requirements | `horizon-vrc-checklist` |
| make store art, trailer, description, pricing | `horizon-store-listing-assets`, `review-game-trailer` |
| upload a build or manage channels | `horizon-store-publish-unity` |
| plan the launch date, Early Access, pre-orders | `horizon-launch-roadmap` |
| upgrade the Meta XR SDK | `hz-unity-sdk-upgrade`, `hz-api-upgrade` |

## Bundles

When the library is installed as Claude Code plugins, skills come in five bundles:
`quest-core` (this skill and the main path), `meta-quest-extras` (other Meta Unity
features and tools), `unity-extras` (other official Unity skills), `community-unity-xr`
and `community-design-art`. See the repository README for install commands.

Last verified: 2026-10-16
