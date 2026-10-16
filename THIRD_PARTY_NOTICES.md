# Third-party notices

This repository mixes original work and third-party work. The root [`LICENSE`](LICENSE) (MIT)
covers **only the original work**. Everything listed under "Third-party work" keeps its own license,
which is stored next to it, and those licenses take precedence for those files.

## Original work: MIT, © 2026 Navid Mirzaaghazadeh

- `skills/custom/**`: all 9 original skills with their scripts and templates. Each `SKILL.md`
  carries `license: MIT`, `metadata.author` and a "Made by" line.
- `README.md`, `AGENTS.md`, `CLAUDE.md`, `CONTRIBUTING.md`, `THIRD_PARTY_NOTICES.md`, `install.sh`,
  `.claude-plugin/marketplace.json`, `mcp/**`
- `docs/install.md`, `docs/usage.md`, `docs/research-meta.md`, `docs/research-unity.md`,
  `docs/community-candidates.md`. The research notes summarise and briefly quote public Meta and
  Unity documentation, with a link to each source.
- The `SOURCE.md` provenance file in each third-party folder, plus `skills/meta-official/ERRATA.md`

## Third-party work

| Path | Author | License | License file |
|---|---|---|---|
| `skills/meta-official/*` (26 skills) | Meta Platforms, Inc. ([meta-quest/agentic-tools](https://github.com/meta-quest/agentic-tools) @ `3a8553d`) | Apache-2.0 | `skills/meta-official/LICENSE` |
| `docs/metavr-cli-reference.md` | Meta Platforms, Inc. (same repo, `docs/metavr-cli.md`, unchanged) | Apache-2.0 | `skills/meta-official/LICENSE` |
| `skills/unity-official/*` (20 skills) | Unity Technologies ([Unity-Technologies/skills](https://github.com/Unity-Technologies/skills) @ `9d28210`) | Unity Companion License v1.4 | `LICENSE.md` in each skill folder; full text in `skills/unity-official/UNITY-COMPANION-LICENSE.md` |
| `skills/community/game-gdd-author`, `game-playtest` | Cem Ipek, AlterLab Creative Technologies Laboratory ([AlterLab-IEU/AlterLab_GameForge](https://github.com/AlterLab-IEU/AlterLab_GameForge)) | MIT | `LICENSE` in each folder |
| `skills/community/submit-game-version` | Dilmer Valecillos ([dilmerv/ArcadeHoopsMR_Build_Tools](https://github.com/dilmerv/ArcadeHoopsMR_Build_Tools)) | MIT | `LICENSE` |
| `skills/community/prototype-fast` | Abhishek Barali and the awesome-gamedev-agent-skills contributors ([gamedev-skills/awesome-gamedev-agent-skills](https://github.com/gamedev-skills/awesome-gamedev-agent-skills)) | Apache-2.0 | `LICENSE`, `NOTICE` |
| `skills/community/review-game-trailer` | GarrettPetersen | MIT | `LICENSE` |
| `skills/community/unity-xr`, `unity-multiplayer` | Nice-Wolf-Studio | MIT | `LICENSE` in each folder |
| `skills/community/test-designing-guide` | nowsprinting | Unlicense (public domain) | `LICENSE` |
| `skills/community/scenario-unity-*` (5), `scenario-blender-*` (3) | Scenario Inc. ([scenario-labs/skills](https://github.com/scenario-labs/skills)) | MIT | `LICENSE` in each folder |
| `skills/community/xr-*` (5) | Adam Matthew Steinberger ([the-vibey-project/vibey](https://github.com/the-vibey-project/vibey)) | MIT | `LICENSE` in each folder |
| `skills/community/quest3-verify-first`, `unity-xr-mr-ui-director` | Wu030616 ([Wu030616/quest3-unity-codex-skills](https://github.com/Wu030616/quest3-unity-codex-skills)) | Apache-2.0 | `LICENSE`, `NOTICE`, `THIRD_PARTY_NOTICES.md` in each folder |

Each third-party folder also has a `SOURCE.md` recording the exact upstream repository, path,
commit, fetch date and review notes for that copy.

### Modifications

Third-party files are copied **unchanged**. The only additions are files placed next to them:
`SOURCE.md`, `skills/meta-official/ERRATA.md`, and a copy of Unity's `LICENSE.md` in each Unity
skill folder (as section 5 of the Unity Companion License asks). Each folder is named after the
skill's own `name:`, which can differ from the upstream folder path recorded in `SOURCE.md`.

### About the Unity Companion License

The Unity skills may be used and redistributed **in connection with authoring or distributing
content under a valid Unity Engine license**. Building Unity games is the purpose of this library,
so that fits. They may not be used for competitive analysis or to build a competing product. Keep
the `LICENSE.md` notices with the files if you copy them. If you need different terms, get the
skills straight from [Unity-Technologies/skills](https://github.com/Unity-Technologies/skills).

## Trademarks

Unity is a trademark of Unity Technologies. Meta, Meta Quest, Meta Horizon and Horizon OS are
trademarks of Meta Platforms, Inc. Blender is a trademark of the Blender Foundation. These names
are used here only to describe compatibility. **This project is not affiliated with, endorsed by
or sponsored by Unity Technologies, Meta Platforms, Inc. or any other company named here.**
