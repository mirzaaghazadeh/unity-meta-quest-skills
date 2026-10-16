# Source: quest3-verify-first

- **Repo:** https://github.com/Wu030616/quest3-unity-codex-skills
- **Path:** `skills/quest3-verify-first/`
- **Commit:** `d7d6dce11acc6b5ca469327ea5b3f50f60921b6f` (2026-07-14)
- **License:** Apache-2.0 (`LICENSE`, `NOTICE`, `THIRD_PARTY_NOTICES.md` copied from the repo root)
- **Stars at fetch:** 0
- **Fetched:** 2026-10-16

## What is in this folder

- `SKILL.md`, `references/primary-sources.md`, `agents/openai.yaml`: unchanged from upstream.
- `UPSTREAM-SOURCE.md`: this is the upstream file `SOURCE.md`, renamed. Its content is unchanged.

## Why it was picked

It is the dependency of `unity-xr-mr-ui-director`. It is a short gate: before any Quest-specific claim or change, resolve the real project (ProjectVersion, manifest, lock file), check the installed editors, sort the available tools into categories, verify each unstable claim against a primary source, and write a project decision record. Its stop conditions keep the agent from mutating a project when compatibility cannot be shown.

## Security review

- I read all files. There are no scripts.
- The skill explicitly forbids `npx -y metavr`, implicit MCP enablement, and device or upload actions without authorization.

## Caveats

- It overlaps with Meta's official `hz-quest-verify-first`, which is collected separately and was the inspiration. If you use Meta's official skill and its `metavr` tooling, you can skip this one. It is kept because the UI director names it, and because it is the "safe edition": no telemetry and no auto-install.
- Single author, new repo.
