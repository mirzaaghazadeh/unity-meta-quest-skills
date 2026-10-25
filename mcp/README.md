# MCP servers for this stack (not installed)

Two MCP servers matter for Unity → Quest → Horizon Store work. Both are free and run locally.
Nothing in this folder is active: `mcp.json.example` is a template to copy into a game project as
`.mcp.json` when you decide to use it.

| Server | What it gives Claude | Command | Needs |
|---|---|---|---|
| **metavr** (Meta VR CLI, formerly `hzdb`) | 40+ tools: device list/connect, install/launch apps, logcat, screenshots, files, **Meta docs search**, Perfetto traces, store upload, XR Simulator | `metavr mcp server` (or `npx -y metavr mcp server`) | Node 16+ for npx, or the standalone binary. A Quest in Developer Mode for device tools |
| **unity** (Unity CLI) | The open Editor's commands: scenes, GameObjects, components, prefabs, scripts, builds, tests, settings, packages, console, `eval` | `unity mcp --project-path <abs path>` | Unity CLI, Unity 6.0+, and the project's `com.unity.pipeline` package (`unity pipeline install`) |

## Do you even need MCP?

In Claude Code, the skills already call both tools directly through Bash: `metavr ...` and
`unity command ...`. Unity says direct CLI calls are faster and use fewer tokens than its MCP mode.
The skills work without either MCP server. Add MCP only if you want the tools to show up as
native tools, or you use a client without a shell.

## Generate the config instead of hand-writing it

Both CLIs can print or write their own entry. Use `--dry-run` first:

```bash
unity mcp configure claude-code --project-path "$PWD" --dry-run   # prints the entry, writes nothing
metavr mcp install claude-code                                     # prints the `claude mcp add` command; --execute runs it
metavr mcp install project                                         # project-local config instead
```

## Known issue: metavr tools don't load in Claude Code (2026-10-16)

**Symptom.** `claude mcp get metavr` shows `Status: ! Connected · tools fetch failed` with
`Invalid result for tools/list: … "path": ["ttlMs"] … "path": ["cacheScope"] …`.

**Cause.** Seen with metavr 1.8.0.17.10 and Claude Code 2.1.301. Claude Code talks the MCP
`2026-07-28` protocol (`server/discover`, no `initialize`). metavr lists `2026-07-28` as supported
and includes `ttlMs`/`cacheScope` in its `server/discover` reply, but leaves them out of
`tools/list` results, which that protocol version requires. With older protocol versions metavr's
replies are valid, which is why other clients don't see it.

**Workaround.** Route the server through
[`metavr_mcp_relay.py`](../skills/custom/unity-quest-dev-setup/scripts/metavr_mcp_relay.py) (it ships
with the `unity-quest-dev-setup` skill), a 60-line standard-library script that adds the two fields
(no caching) and passes everything else through:

```bash
claude mcp remove metavr 2>/dev/null
claude mcp add metavr -- python3 -I /ABS/PATH/skills/custom/unity-quest-dev-setup/scripts/metavr_mcp_relay.py \
  ~/.metavr/bin/metavr mcp server --update
claude mcp get metavr        # expect: ✔ Connected
```

Claude Code loads new MCP tools only when a session starts: restart it (`claude --continue` keeps
the conversation). Check again after each metavr or Claude Code update, and drop the relay once the
direct command connects. The `npx -y metavr mcp server` form from `mcp.json.example` can be wrapped
the same way.

**Re-tested 2026-10-25: no longer seen.** With Claude Code 2.1.310, the same binary 1.8.0.17.10
(npm `metavr` 1.8.1 changed only the download wrapper) registered directly as
`~/.metavr/bin/metavr mcp server --update` connected, and its tools loaded after
`/reload-plugins`. The `meta-vr` plugin's `npx -y metavr mcp server` failed once with `tools fetch
failed — request _meta is missing or has malformed required fields`, then connected on the next
check. Try the direct command first, and keep the relay for older Claude Code versions.

## Meta XR Operator

Meta XR Operator (experimental, in Meta XR Core SDK v207+) runs an MCP server inside the running
Unity app and an MCP proxy for the agent. `metavr xroperator enable` (the default) federates the
proxy's tools into metavr's own MCP server, so with metavr registered you don't need a second
Operator entry. Check with `metavr xroperator status`. Unity setup: the Unity skills and
`docs/meta-reference/unity-xr-operator.md`; cross-engine summary: `docs/meta-reference/ai-tools.md` §5.

## Avoid duplicates

- Each Meta route registers its **own** metavr server under its own name: `metavr init`, the
  `meta-vr` plugin, `metavr mcp install`, the Meta Horizon extension for VS Code (which also
  registers it in Cursor), and MQDH's separate MCP server. Keep one entry (`claude mcp list`) and
  remove the rest (Meta's troubleshooting page, 2026-08-27).
- Meta now says `metavr init` "already installs these skills and configures the MCP server", and to
  use the plugin only without it (2026-08-31). Both install the same skills that are already in
  this library's `skills/meta-official/`, as does `metavr skills install` (into `~/.agents/skills`).
  Use one source of Meta's skills, or Claude sees two copies of each.
- Unity's `unity@unity-agent-plugin` likewise duplicates `skills/unity-official/`.
- Don't run Unity's older AI Assistant MCP (`com.unity.ai.assistant`, now deprecated) next to `unity mcp`.

## Sources

- Meta: https://developers.meta.com/vr/essentials/metavr-install/ (2026-08-31; the old
  `/horizon/essentials/ai-tooling-mcp/` page now redirects here),
  https://developers.meta.com/vr/essentials/metavr-other-tools/ (2026-08-31),
  https://developers.meta.com/vr/essentials/metavr-troubleshooting/ (2026-08-27),
  https://developers.meta.com/vr/documentation/unity/meta-xr-operator/ (2026-09-09), and the
  `meta-quest/agentic-tools` README (`.mcp.json`)
- Unity: `docs/research-unity.md` §1 and §5 (Unity CLI docs, https://docs.unity.com/en-us/unity-cli/llms.txt)
