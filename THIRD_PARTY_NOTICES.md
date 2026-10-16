# Third-party notices

This repository mixes original work and third-party work. The root [`LICENSE`](LICENSE) (MIT)
covers **only the original work**. Everything listed under "Third-party work" keeps its own license,
which is stored next to it, and those licenses take precedence for those files.

## Original work: MIT, © 2026 Navid Mirzaaghazadeh

- `skills/custom/**`: all 8 custom skills and their scripts and templates
- `README.md`, `THIRD_PARTY_NOTICES.md`, `CONTRIBUTING.md`, `mcp/**`
- `docs/research-meta.md`, `docs/research-unity.md`, `docs/community-candidates.md`. These are
  research notes. They summarise and briefly quote public Meta and Unity documentation, with a
  link to each source.
- The `SOURCE.md` provenance file in each third-party folder, plus `skills/meta-official/ERRATA.md`

## Third-party work

| Path | Author | License | License file |
|---|---|---|---|
| `skills/meta-official/*` (26 skills) | Meta Platforms, Inc. ([meta-quest/agentic-tools](https://github.com/meta-quest/agentic-tools) @ `3a8553d`) | Apache-2.0 | `skills/meta-official/LICENSE` |
| `docs/metavr-cli-reference.md` | Meta Platforms, Inc. (same repo, `docs/metavr-cli.md`, unchanged) | Apache-2.0 | `skills/meta-official/LICENSE` |
| `skills/unity-official/*` (20 skills) | Unity Technologies ([Unity-Technologies/skills](https://github.com/Unity-Technologies/skills) @ `9d28210`) | Unity Companion License v1.4 | `LICENSE.md` in each skill folder; full text in `skills/unity-official/UNITY-COMPANION-LICENSE.md` |
| `skills/community/AlterLab-IEU__*` (2) | Cem Ipek, AlterLab Creative Technologies Laboratory | MIT | `LICENSE` in each folder |
| `skills/community/dilmerv__submit-game-version` | Dilmer Valecillos | MIT | `LICENSE` |
| `skills/community/gamedev-skills__prototype-fast` | Abhishek Barali and the awesome-gamedev-agent-skills contributors | Apache-2.0 | `LICENSE`, `NOTICE` |
| `skills/community/GarrettPetersen__review-game-trailer` | GarrettPetersen | MIT | `LICENSE` |
| `skills/community/Nice-Wolf-Studio__*` (2) | Nice-Wolf-Studio | MIT | `LICENSE` in each folder |
| `skills/community/nowsprinting__test-designing-guide` | nowsprinting | Unlicense (public domain) | `LICENSE` |
| `skills/community/scenario-labs__*` (8) | Scenario Inc. | MIT | `LICENSE` in each folder |
| `skills/community/the-vibey-project__*` (5) | Adam Matthew Steinberger | MIT | `LICENSE` in each folder |
| `skills/community/Wu030616__*` (2) | Wu030616 | Apache-2.0 | `LICENSE`, `NOTICE`, `THIRD_PARTY_NOTICES.md` in each folder |

Each third-party folder also has a `SOURCE.md` recording the exact upstream repository, commit,
fetch date and the review notes for that copy.

### Modifications

Third-party files are copied **unchanged**. The only additions are files placed next to them:
`SOURCE.md`, `skills/meta-official/ERRATA.md`, and a copy of Unity's `LICENSE.md` in each Unity
skill folder (as section 5 of the Unity Companion License asks). Folder names under
`skills/community/` carry an `owner__` prefix to show where each one came from. The skill files
themselves are not renamed.

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
