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
metavr mcp install claude-code                                     # writes it, so run only when you want it
metavr mcp install project                                         # project-local config instead
```

## Avoid duplicates

- Meta warns that each install route (`metavr init`, the `meta-vr` plugin, `metavr mcp install`)
  registers its **own** metavr server. Pick one.
- The `meta-vr` Claude Code plugin and Unity's `unity@unity-agent-plugin` also install the same
  skills that are already in this library. If you install the plugins, don't also copy the matching
  folders from `skills/`, or Claude sees two copies of each skill.
- Don't run Unity's older AI Assistant MCP (`com.unity.ai.assistant`, now deprecated) next to `unity mcp`.

## Sources

- Meta: https://developers.meta.com/horizon/essentials/ai-tooling-mcp/ and the `meta-quest/agentic-tools` README (`.mcp.json`)
- Unity: `docs/research-unity.md` §1 and §5 (Unity CLI docs, https://docs.unity.com/en-us/unity-cli/llms.txt)
