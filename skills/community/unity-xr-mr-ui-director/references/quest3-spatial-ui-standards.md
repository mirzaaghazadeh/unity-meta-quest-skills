# Quest 3 spatial UI standards

These are starting points for prototypes. Validate on the intended user population and current Quest 3 runtime.

## Geometry and readability

- Start primary panels around 1–2 m from the eyes and slightly below neutral gaze.
- Avoid persistent interactive content closer than 0.5 m.
- Express typography and target size by visual angle. A 1-degree target is about 17.5 mm at 1 m and 34.9 mm at 2 m.
- For direct touch around 45 cm, begin near a 32 mm square interactive face. Increase it for low dexterity, motion, or uncertain tracking.
- Keep primary content near the central field of view. Require deliberate head movement for secondary content, not routine controls.
- Curve very wide panels or split them so edge content does not sit at a markedly different focal distance.

## Visual treatment

- Put text on a stable surface. Passthrough changes contrast continuously.
- Use SDF text. Validate weight, atlas quality, and mip behaviour on device.
- Maintain at least WCAG AA-like contrast as a screen-space baseline, then test against real bright and cluttered rooms.
- Do not encode state by colour alone. Add shape, icon, text, motion, audio, or haptic feedback.
- Keep translucent layers limited. Transparency weakens readability and adds overdraw on mobile XR hardware.
- Use depth for hierarchy sparingly. Keep text and its control surface at nearly the same depth.

## Required control states

Every interactive control needs the applicable subset of:

```text
idle
aim or gaze acquired
hover or proximity
pressed
activated or confirmed
disabled
busy
error
tracking lost
```

The transition from hover to activation must remain visible when the hand or controller partially occludes the target.

## Comfort and access

- Provide seated and standing placement when the task allows it.
- Keep core actions inside a comfortable chest-to-eye working zone.
- Offer one-handed completion for core flows where practical.
- Avoid rigid head-locked panels. Use delayed body-follow behaviour with recentering when a following menu is necessary.
- Provide reduced-motion and alternative input paths.
- Pair spatial audio cues with visual equivalents.

## Frame and interaction budgets

- Verify the current Quest requirements before choosing a release refresh rate.
- Treat 90 Hz as a useful Quest 3 design target when the experience and current platform guidance support it.
- Budget in milliseconds. Inspect CPU, GPU, main-thread, render-thread, Canvas rebuild, fill-rate, and thermal behaviour.
- Disable hidden UI and decorative raycast targets. Split static from frequently changing canvases.

## Sources to verify during active work

- Meta Quest developer documentation: <https://developers.meta.com/horizon/develop/>
- Unity XR Interaction Toolkit UI setup example for 3.2; select the installed package version before implementation: <https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/manual/ui-setup.html>
- Unity Meta OpenXR MR sample: <https://github.com/Unity-Technologies/mr-example-meta-openxr>
- Meta MRUK sample: <https://github.com/oculus-samples/Unity-MRUtilityKitSample>
- Microsoft mixed-reality button research: <https://learn.microsoft.com/en-us/windows/mixed-reality/design/button>

Quest SDKs and store requirements change. Use `quest3-verify-first` before turning any value or API name into a project requirement.
