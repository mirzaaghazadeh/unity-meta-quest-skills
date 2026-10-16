# AGENTS.md

Guidance for AI coding agents (Claude Code, Codex, Cursor, Copilot and others) working with this
repository. People should start at [README.md](README.md).

This repo is a **skill library**: 80 Agent Skills for building Meta Quest VR games in Unity with
the Unity CLI and shipping them on the Meta Horizon Store. It holds no app code.

## Layout

```
skills/meta-official/<skill>/    26  Meta's skills, copied unchanged (Apache-2.0) + ERRATA.md
skills/unity-official/<skill>/   20  Unity's skills, copied unchanged (Unity Companion License)
skills/custom/<skill>/           11  Original skills by Navid Mirzaaghazadeh (MIT)
skills/community/<skill>/        23  Third-party skills, copied unchanged (MIT/Apache/Unlicense)
.claude-plugin/marketplace.json      Five plugin bundles; the single source of truth for bundles
install.sh                           Installs bundles or skills by reading marketplace.json
docs/                                install.md, usage.md, research notes, CLI reference
mcp/                                 Optional MCP config for `metavr` and `unity`
```

Every folder name equals its skill's `name:`. Every third-party folder has a `SOURCE.md`
(upstream repo, commit, license, review notes).

## If a user asks you to install skills from this repo

1. **Ask where** (one game project or every project) and **which bundle**. Default to
   `quest-core` in the game project.
2. **Pick one method.** Use the Claude Code plugin when you are Claude Code:
   `claude plugin install quest-core --marketplace mirzaaghazadeh/unity-meta-quest-skills --scope project`.
   Otherwise use `./install.sh --project <dir> <bundle>` from a clone, or `npx skills add` /
   `gh skill install` (see [docs/install.md](docs/install.md)).
3. **Check for duplicates first.** Look for Meta's `meta-vr` plugin, Unity's
   `unity@unity-agent-plugin` plugin and existing `.claude/skills/` folders with the same names.
   Tell the user instead of installing a second copy.
4. **Confirm** before changing user-level Claude settings (`--scope user`, `~/.claude/skills`).
5. Tell the user to start a new session or run `/reload-plugins`, then suggest:
   "Use quest-dev-workflow to plan my Quest game."

## If you are using these skills in a game project

- Start with `quest-dev-workflow` (the router) and `hz-quest-verify-first` (verify Quest facts
  against live docs with `metavr docs search`).
- Drive the open Editor with the `unity-cli` / `unity-pipeline` skills (`unity status`,
  `unity command run_script`) rather than hand-editing `.unity`, `.prefab` or `.asset` files.
- **Ask before:** any store upload or `--publish`, anything touching the LIVE/production channel,
  submitting for review, installing tools, or editing Claude/MCP config.
- **Never** print, log or commit keystore passwords or app secrets.
- Prefer the `horizon-*` skills' values over Meta's `hz-store-submit` where they differ (see
  `skills/meta-official/ERRATA.md`).

## If you are changing this repository

- **Don't edit** files under `skills/meta-official/`, `skills/unity-official/` or third-party
  folders in `skills/community/`. Refresh them from upstream as described in their `SOURCE.md`.
  Record disagreements with live docs in `skills/meta-official/ERRATA.md`.
- **Custom skills** (`skills/custom/`): keep `name:` equal to the folder name, keep the
  `license: MIT` and `metadata.author` fields and the "Made by" line, cite a Meta or Unity URL for
  every number or flag, mark anything unconfirmed as unverified, and update `Last verified:`.
- **New or moved skills:** add the path to exactly one bundle in
  `.claude-plugin/marketplace.json`, add a row to the README catalog, and for third-party skills
  add `LICENSE`/`NOTICE`, a `SOURCE.md` and a row in `THIRD_PARTY_NOTICES.md`. Follow
  [CONTRIBUTING.md](CONTRIBUTING.md) for the review rules.
- **Check before committing:**
  ```bash
  claude plugin validate . --strict      # marketplace and skill frontmatter
  ./install.sh --list                    # every bundle resolves
  ./install.sh --project /tmp/skilltest --dry-run all
  ```
- Never commit `.env` files, keystores (`*.keystore`, `*.jks`) or credentials. `.gitignore`
  blocks the common ones.
