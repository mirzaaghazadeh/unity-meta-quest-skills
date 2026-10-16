# Source and local changes

Prepared as a portable Agent Skill on 2026-07-14.

Primary upstream references:

- `hz-unity-meta-quest-ui` from Meta Quest Agentic Tools v1.2.2, commit `8cb3ef460d2f60bb85dddba22797f5dd1d4adcbe`, Apache-2.0: <https://github.com/meta-quest/agentic-tools>
- Unity XR Interaction Toolkit documentation and examples
- Unity Meta OpenXR MR example
- Meta MRUK example
- Microsoft mixed-reality interaction research

Local changes:

- combines design, Unity project audit, interaction-stack selection, MRUK placement, and Quest 3 QA into one self-contained Skill;
- defaults to OpenXR/XRI-compatible architecture;
- removes assumptions that `Unity_RunCommand` or `meta_add_*` tools exist;
- does not install or invoke `metavr`;
- does not copy Meta hooks, telemetry, device mutation tools, automatic scene saves, asset deletion recipes, or busy-wait TMP installation;
- adds a read-only Python project auditor.

Change notice: this skill is a safety-focused rewrite with a new workflow, new references, and a new read-only Python auditor. It does not redistribute Meta hooks, binaries, telemetry, or device-mutation tools. The upstream repository is referenced for attribution and provenance only.
