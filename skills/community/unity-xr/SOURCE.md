# Source: unity-xr

- **Repo:** https://github.com/Nice-Wolf-Studio/unity-claude-skills
- **Path:** `skills/unity-xr/` (`SKILL.md` + `references/xr-toolkit.md`)
- **Commit:** `a149d3e995420432f52b0899092daa4b069f441f` (2026-09-15)
- **License:** MIT (`LICENSE` copied)
- **Stars at fetch:** 40
- **Fetched:** 2026-10-16

## Why it was picked

It is a Unity 6.3 LTS-based guide to the XR stack: XR Plug-in Management, OpenXR features and interaction profiles, XR Interaction Toolkit 3.3 interactors and interactables, the locomotion architecture (Locomotion Mediator, XR Body Transformer), haptics, tracking origin, the XR Interaction Simulator, and an anti-pattern list. It is the deepest engine-specific XRI skill found, and it is a companion to the Meta-specific official skills.

## Security review

I read all files. They are Markdown only, with no scripts and no instructions to run anything.

## Caveats

- It is generic Unity XR, not Meta-specific. For Meta XR SDK, MRUK, Passthrough and Interaction SDK, use the official Meta skills.
- The hand-tracking section shows the legacy `InputDevice`/`CommonUsages.handData` API. For new work, prefer the XR Hands package (`com.unity.xr.hands`, `XRHandSubsystem`).
- It is derived from documentation, not tested on a device. Check package versions against your installed XRI and OpenXR versions.
- Upstream plugin naming: the parent repo installs as a plugin (`/unity:unity-xr`). Here it is a standalone skill.
