# Source: xr-ux-design-and-performance-budgeting

- **Repo:** https://github.com/the-vibey-project/vibey
- **Path:** `src/vibey_tools/skills/plugins/vr-ar-development/skills/xr-ux-design-and-performance-budgeting/`
- **Commit:** `0b2cfd3dc3e0fd3577d12c822d45cf7840c68d58` (2026-10-11)
- **License:** MIT (`LICENSE` copied from the repo root)
- **Stars at fetch:** 3
- **Fetched:** 2026-10-16

## Why it was picked

This is part 4 of the five-skill VR/AR reference. It covers 3D UX (text legibility, placement zones, diegetic UI, scale and affordance, never moving the camera without input) and XR performance budgeting (11.1 ms at 90 Hz for both eyes, sustained versus burst on mobile SoCs, overdraw and fill rate, profiling on the device with frame-time histograms rather than averages).

## Security review

I read the file. It is Markdown only.

## Caveats

- Some levers it lists, such as "GPU instancing", are generic. On Unity 6 URP, follow `scenario-labs__scenario-unity-performance` for the SRP Batcher and GPU Resident Drawer specifics, and check them against Quest's Vulkan/Adreno path.
- See `the-vibey-project__xr-perceptual-constraints-displays-and-tracking/SOURCE.md` for notes on the parent repo.
