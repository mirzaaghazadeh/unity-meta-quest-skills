# Source: xr-rendering-input-and-spatial-understanding

- **Repo:** https://github.com/the-vibey-project/vibey
- **Path:** `src/vibey_tools/skills/plugins/vr-ar-development/skills/xr-rendering-input-and-spatial-understanding/`
- **Commit:** `0b2cfd3dc3e0fd3577d12c822d45cf7840c68d58` (2026-10-11)
- **License:** MIT (`LICENSE` copied from the repo root)
- **Stars at fetch:** 3
- **Fetched:** 2026-10-16

## Why it was picked

This is part 2 of the five-skill VR/AR reference. It covers stereo rendering (single-pass/multiview), reprojection (ATW/ASW as a safety net, not a budget), fixed and eye-tracked foveation, MSAA versus TAA in XR, and TBDR rules. It also covers input (controllers, hands, gaze, gorilla arm) and the locomotion comfort table: snap turn as the default, and vignetted smooth movement offered as an option rather than imposed. Anchors and occlusion are included for MR.

## Security review

I read the file. It is Markdown only.

## Caveats

- Engine-neutral. Unity- and Quest-specific settings (URP render scale, FFR level, Application SpaceWarp) live in the official Meta skills and `scenario-labs__scenario-unity-mobile`.
- See `the-vibey-project__xr-perceptual-constraints-displays-and-tracking/SOURCE.md` for notes on the parent repo.
