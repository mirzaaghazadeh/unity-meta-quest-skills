# Quest performance targets (Meta sources, fetched 2026-10-16)

Meta's pages don't fully agree. Each value carries its source and date. The store **gate** is
VRC.Quest.Performance.1: render at ≥ 60 fps at an allowed refresh rate, with no extended dips over
the content length or 45 minutes, measured with OVR Metrics Tool. Meta's perf guidance sets a
higher **target**: interactive apps should hit at least 72 FPS.

## Frame rate and refresh

| Item | Value | Source |
|---|---|---|
| VRC rendering floor | ≥ 60 fps (≥ 30 fps where AppSW is in use) | vrc-quest-performance-1 (2025-10-22) |
| Allowed interactive refresh rates | 72, 80, 90, 96, 100, 120 Hz (96, 100 and 120 not on all devices; 96/100 planned for Quest 2/3/3S in a future OS) | vrc-quest-performance-1 |
| Media apps | 60 Hz where supported | vrc-quest-performance-1, unity-perf |
| Interactive target | ≥ 72 FPS | unity-perf (2024-10-30) |
| Quest 3 target | 72 Hz (90 Hz recommended) | device-optimization-comparison (2026-05-11) |
| Quest 2 target | 72 Hz | device-optimization-comparison |
| Frame budget | 13.8 ms at 72 Hz; ~11.1 ms at 90 Hz; ~8.3 ms at 120 Hz | device-optimization-comparison; unity-set-disp-freq (search summary) |
| Runtime Optimizer "Quick Perf" target | 14.2 ms CPU/GPU frame time | unity-quest-runtime-optimizer (2026-07-31) |
| Render scale | ≥ 85% for most of the experience (Rec) | vrc-quest-performance-4 |
| Launch | Head-tracked graphics within 4 s, or a VR loading indicator | vrc-quest-performance-3 |

## Hardware (device-optimization-comparison, 2026-05-11; Quest 2 and Quest 3 only)

| | Quest 2 | Quest 3 |
|---|---|---|
| Chipset | Snapdragon XR2 Gen 1 | Snapdragon XR2 Gen 2 (~2.5x Quest 2 GPU) |
| RAM | 6 GB | 8 GB |
| Render resolution per eye | 1832 × 1920 | 2064 × 2208 |
| Refresh rates (this page) | 72, 80, 90, 120 Hz | 72, 80, 90, 120 Hz |
| Draw calls (this page) | < 100 | < 200 |
| Triangles per frame (this page) | < 750K | < 1.5M |
| Advice | More draw-call sensitive: batch and instance; lower render scale; lower-res textures; limit post-processing | More memory for textures; use runtime device detection to scale quality |

Quest 3S, Quest Pro and Meta VR Glasses are not on that page. Treat Quest 3S like Quest 3 for
draw calls and triangles (Meta groups them in unity-perf), but test it separately.

## Quest 3S and Meta VR Glasses (compare-devices 2026-09-30; render-scale 2026-09-01; memory-ram 2026-08-31; cpu-gpu-levels 2026-09-11)

| | Quest 3S | Meta VR Glasses (ships spring 2027) |
|---|---|---|
| Chipset / RAM | XR2 Gen 2 / 8 GB | XR2 Gen 3 / 12 GB |
| Panel per eye | 1832 × 1920 (20 PPD) | 2412 × 2288 micro-OLED (37 PPD) |
| Render target at scale 1.0 | 1680 × 1760 | 1728 × 1760 (simulator profile: 1680 × 1760) |
| FoV | 96° × 90° | 70° × 66° nominal |
| Refresh | 72–120 Hz (unity-set-disp-freq 2026-10-01) | 120 Hz on the spec page; simulator 90/120 |
| PSS memory kill limit | 5.75 GiB | 5.75 GiB |
| Performance levels | Up to GPU 5 with dynamic resolution; CPU/GPU trading | Design to CPU 4 / GPU 4; no GPU 5, dual-core or trading; CPU Boost 4 → 8 from a refilling budget |
| Draw calls / triangles | As Quest 3 (unity-perf) | **Not published.** Meta: a Quest 3 app holding its rate at nominal should match it; geometry-heavy tiled passes gain, full-screen passes don't (optimize-performance 2026-09-19) |

Test plan for VR Glasses (test-your-app 2026-09-25): profile with dynamic resolution off and fixed
foveation first, then on; run the busiest scenes for 45 minutes or the content length; thermals
need real hardware (glasses and puck are separate thermal domains).

## Draw calls and triangles by workload (unity-perf, 2024-10-30)

| Device | Busy simulation | Medium | Light | Triangles |
|---|---|---|---|---|
| Quest 2, Quest Pro | 80-200 | 200-300 | 400-600 | 750k-1m |
| Quest 3, Quest 3S | 200-300 | 400-600 | 700-1000 | 1.3m-1.8m |

The device comparison page is newer (2026-05-11) and gives tighter single numbers (< 100 / < 200
draw calls). Plan for the tighter numbers on content-heavy scenes and use the ranges for lighter
ones.

## Measuring

- **OVR Metrics Tool**: `metavr tools install ovrmetric`. Use Report Mode or the Performance HUD;
  it reports FPS, render scale, CPU/GPU levels and throttling. This is the tool Performance.1/.4
  reviewers use.
- **Perfetto**: `metavr perf capture` / `metavr perf analyze-trace` (Meta `hz-perfetto-debug`).
- **simpleperf** for CPU hotspots (Meta `hz-simpleperf-debug`).
- **Meta Quest Runtime Optimizer** (Asset Store, experimental): per-GameObject GPU cost and
  bottleneck classification. Needs Horizon OS v78+ and Unity 2022.3+.
- **RenderDoc Meta Fork**: `metavr tools install renderdoc`.
- Lock CPU/GPU levels before comparing captures (unity-perf).

Last verified: 2026-10-17 (Quest 3S / VR Glasses section); 2026-10-16 (the rest)
