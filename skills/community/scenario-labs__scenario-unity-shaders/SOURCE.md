# Source: scenario-unity-shaders

- **Repo:** https://github.com/scenario-labs/skills
- **Path:** `skills/game-engines/unity/scenario-unity-shaders/`
- **Commit:** `49dd26e47371163ae0020c7f5627d0993626bfc2` (2026-10-15)
- **License:** MIT, Copyright 2026 Scenario Inc. (`LICENSE` copied)
- **Stars at fetch:** 1032
- **Fetched:** 2026-10-16

## Why it was picked

It covers Unity 6.3 URP 17.3 shader work:
- hand-written URP HLSL as the primary route, with Shader Graph driven through exposed properties and Custom Function `.hlsl`;
- every pass agreeing (ShadowCaster, DepthOnly, DepthNormals);
- the 6.3 `_CLUSTER_LIGHT_LOOP` and Forward+ light-loop trap;
- variant budgets counted after stripping (`shader_feature` vs `multi_compile` vs `dynamic_branch`);
- PSO warm-up proven cold versus warm, and Render Graph renderer-feature shape.

**Its shader templates include the single-pass-instanced stereo macros** (`UNITY_VERTEX_OUTPUT_STEREO`, `UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO`, `UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX`). Without those, custom shaders render in one eye only on Quest. That is why this was chosen over `adevra/unity-shader-agent-skills`.

## Security review

- I read the SKILL.md. I grepped references, the 10 C# files, the 9 shader, HLSL and compute files, and `ut_shaders.py`.
- **subprocess:** only to launch a development player for the PSO trace.
- No network calls.

## Caveats

- **Measured on Metal (M5 Max).** Re-measure on Quest (Vulkan/Adreno). Half precision matters on Adreno, unlike on desktop where it compiles to float. Profile on the device.
- PSO `.graphicsstate` collections are per graphics API. Trace one on Quest Vulkan.
- `GraphicsStateCollection` is experimental in 6.3.
- It needs `scenario-labs__scenario-unity-expert` installed as a sibling folder named `scenario-unity-expert`.
