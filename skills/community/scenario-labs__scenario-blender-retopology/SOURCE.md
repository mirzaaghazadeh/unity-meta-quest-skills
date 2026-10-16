# Source: scenario-blender-retopology

- **Repo:** https://github.com/scenario-labs/skills
- **Path:** `skills/dcc/blender/scenario-blender-retopology/`
- **Commit:** `49dd26e47371163ae0020c7f5627d0993626bfc2` (2026-10-15)
- **License:** MIT, Copyright 2026 Scenario Inc. (`LICENSE` copied)
- **Stars at fetch:** 1032
- **Fetched:** 2026-10-16

## Why it was picked

Quest games need tight triangle budgets. This skill turns dense sculpts, scans or **AI-generated meshes** into clean low-poly topology:
- a game low poly is the silhouette plus the loops needed for animation;
- auto-remesh (QuadriFlow, voxel) is fine for static assets, while deforming characters need planned loops;
- pole placement rules, a Shrinkwrap cage fitted on the subdivided surface, and topology audits.

The `bx_retopo.py` toolkit is tested on Blender 5.2.1 and every claim is sourced.

## Security review

- I read the SKILL.md. I grepped the references and `bx_retopo.py`.
- It uses bpy, bmesh and the standard library.
- No subprocess and no network.

## Caveats

- It needs `scenario-labs__scenario-blender-expert` installed as a sibling folder named `scenario-blender-expert`.
- The default game budget is "< 10,000 tris per character". Set your own Quest budget per scene.
