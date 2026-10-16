# Source and local changes

Prepared as a portable Agent Skill on 2026-07-14.

The verification concept was informed by `hz-quest-verify-first` from Meta Quest Agentic Tools v1.2.2, commit `8cb3ef460d2f60bb85dddba22797f5dd1d4adcbe`:

<https://github.com/meta-quest/agentic-tools>

Change notice: this Skill is a safety-focused rewrite. It uses live primary-source verification and project inspection. It does not copy the upstream `allowed-tools` grant, execute `npx`, install `metavr`, register telemetry hooks, or expose ADB/device mutation commands.

The original repository's Markdown is Apache-2.0 and carries a Meta Platforms, Inc. copyright notice. The npm `metavr` package uses a different Meta SDK license and is intentionally not installed or redistributed by this Skill.
