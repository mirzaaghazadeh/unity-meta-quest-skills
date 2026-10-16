# Source: scenario-unity-performance

- **Repo:** https://github.com/scenario-labs/skills
- **Path:** `skills/game-engines/unity/scenario-unity-performance/`
- **Commit:** `49dd26e47371163ae0020c7f5627d0993626bfc2` (2026-10-15)
- **License:** MIT, Copyright 2026 Scenario Inc. (`LICENSE` copied)
- **Stars at fetch:** 1032
- **Fetched:** 2026-10-16

## Why it was picked

It treats performance as a measurement discipline:
- budget in milliseconds, name the bound thread or GPU first, decide on numbers from a development player rather than the editor;
- GC allocation counted in bytes and calls, with 0 B per gameplay frame as the target;
- Unity 6.3 URP draw-call reality: SRP Batcher plus GPU Resident Drawer, with the conditions under which GRD silently does nothing;
- PSO and shader warm-up, and attributing every spike to a ProfilerMarker;
- allocation tests in CI.

Every claim is sourced and timestamped (Unity profiling e-books, Unity Profiler team talks). It ships C# probes and audits plus `ut_perf.py`.

## Security review

- I read the SKILL.md. I grepped references, the 9 C# files and `ut_perf.py`.
- **subprocess targets:** Unity players and `xcrun xctrace` (Metal traces).
- **`PerfShaders.cs`:** deletes and recreates only its own generated ShaderVariantCollection asset, and reads optional `AGENTKIT_SHADER_*` environment variables.
- No network calls.

## Caveats

- **Desktop/macOS measurements** (Metal System Trace). On Quest, swap in on-device tools: OVR Metrics Tool, Perfetto, RenderDoc for Quest, and Snapdragon Profiler. Meta's official skills cover these.
- **Check desktop-leaning advice for Quest.** GPU Resident Drawer, Forward+ and Split graphics jobs need confirming on Vulkan/Adreno with single-pass instanced stereo before you adopt them.
- It needs `scenario-unity-expert` installed next to it, in the same parent folder.
