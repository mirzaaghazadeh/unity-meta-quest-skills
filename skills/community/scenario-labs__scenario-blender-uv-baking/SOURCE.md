# Source: scenario-blender-uv-baking

- **Repo:** https://github.com/scenario-labs/skills
- **Path:** `skills/dcc/blender/scenario-blender-uv-baking/`
- **Commit:** `49dd26e47371163ae0020c7f5627d0993626bfc2` (2026-10-15)
- **License:** MIT, Copyright 2026 Scenario Inc. (`LICENSE` copied)
- **Stars at fetch:** 1032
- **Fetched:** 2026-10-16

## Why it was picked

It covers game-ready UVs and high-to-low bakes in Blender 5.2:
- seams follow the texturing method, and every hard edge is a seam;
- the unwrap method is chosen by measurement;
- texel density is set before texture size, with target px/cm per camera type;
- pixel-true padding of res/128;
- projection distance is measured with adaptive extrusion rather than guessed;
- bakes are judged in pixels (inverted or flat texels) and under a raking light;
- unique UVs where baked lighting differs, and the OpenGL normal convention, which matches Unity.

The `bx_uvbake.py` toolkit computes every gate.

## Security review

- I read the SKILL.md. I grepped the references and `bx_uvbake.py`.
- It uses bpy, bmesh, numpy and the standard library, and writes bake images.
- No subprocess and no network.

## Caveats

- It needs `scenario-labs__scenario-blender-expert` installed as a sibling folder named `scenario-blender-expert`.
- `numpy` must be available in Blender's Python. Recent Blender builds bundle it.
