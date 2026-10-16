# Source: test-designing-guide

- **Repo:** https://github.com/nowsprinting/unity-coding-skills
- **Path:** `skills/test-designing-guide/` (`SKILL.md` + `assets/output-format.md`, `assets/output-format-ja.md`)
- **Commit:** `94c16e2f27eed04e5fddacbff982a2e31c99c084` (2026-10-09)
- **License:** Unlicense (public domain; `LICENSE` copied)
- **Stars at fetch:** 20. The author, Koji Hasegawa (nowsprinting), maintains the widely used Unity Test Helper and UI Test Helper packages.
- **Fetched:** 2026-10-16

## Why it was picked

It is a rigorous, Unity-specific method for designing tests before writing them:
- **Layers:** Editor tests, then unit tests (Play Mode for runtime code), then integration tests on scenes and prefabs, then visual verification by screenshot analysis, with manual tests last.
- **Techniques:** equivalence partitioning ("one test method = one partition = one outcome"), boundary values, state transition and decision tables, and game-specific error guessing (button mashing, input during loading, tunnelling, PRNG bias).
- **Plus:** guidance on stubbing PRNGs, and a fixed output table.

Use it with the official `unity test` CLI.

## Security review

I read all files. They are Markdown only.

## Caveats

- `user-invocable: false`. Upstream it is loaded by the plugin's `plan-feature`, `fix-bug` and `test-designer` subagent, which were not collected because they depend on the JetBrains Rider MCP server. It works on its own when invoked by name or description.
- Visual-verification and manual-test layers matter a lot in VR. Headset feel and comfort always end up in manual tests.
