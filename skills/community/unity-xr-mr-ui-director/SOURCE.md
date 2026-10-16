# Source: unity-xr-mr-ui-director

- **Repo:** https://github.com/Wu030616/quest3-unity-codex-skills
- **Path:** `skills/unity-xr-mr-ui-director/`
- **Commit:** `d7d6dce11acc6b5ca469327ea5b3f50f60921b6f` (2026-07-14)
- **License:** Apache-2.0 (`LICENSE`, `NOTICE`, `THIRD_PARTY_NOTICES.md` copied from the repo root)
- **Stars at fetch:** 0
- **Fetched:** 2026-10-16

## What is in this folder

- `SKILL.md`: unchanged from upstream.
- `references/`, `scripts/audit_unity_xr_ui.py`, `agents/openai.yaml`: unchanged.
- `UPSTREAM-SOURCE.md`: this is the upstream file `SOURCE.md`, renamed so this provenance file could use the name `SOURCE.md`. Its content is unchanged.

## Why it was picked

The only community skill found that targets Quest 3 spatial UI in Unity end to end. It covers the spatial UI brief in metres and visual angle, choosing one input stack (XRI or Meta Interaction SDK, never both), world-space Canvas rules, MRUK room-aware placement, and a QA ladder that ends on a real Quest 3 device. It ships a standard-library Python auditor that only reads files. The auditor reports the editor version, XR packages, and duplicate EventSystem or UI input modules in scenes and prefabs.

## Security review

- I read the SKILL.md and every reference file.
- I read the auditor script. It imports only the standard library and reads files under the project path. It has no network, no subprocess, and no writes.
- The skill tells the agent not to install `metavr`, not to assume bridge tools exist, and not to mutate devices without authorization.

## Caveats

- It was written for Codex (`$skill` invocation and `agents/openai.yaml`). The SKILL.md body works in Claude Code as is.
- It routes platform claims through `quest3-verify-first`, which is collected next to it.
- Some text is adapted from Meta's official `hz-unity-meta-quest-ui` (Apache-2.0, with attribution in NOTICE). It does not replace Meta's official skill.
- Single author, new repo, no stars yet. The values in `references/quest3-spatial-ui-standards.md` are starting points to check on a device, not Meta requirements.
