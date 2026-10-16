# Unity implementation

## Package decision

Read both `Packages/manifest.json` and `Packages/packages-lock.json`. Check current Unity and Meta documentation before adding versions.

Core package identifiers commonly involved:

```text
com.unity.inputsystem
com.unity.xr.management
com.unity.xr.openxr
com.unity.xr.interaction.toolkit
com.unity.xr.hands
com.unity.ugui
com.unity.textmeshpro                 # older Unity lines
com.unity.render-pipelines.universal
com.meta.xr.sdk.core                 # Meta branch, when selected
com.meta.xr.mrutilitykit             # MRUK branch, when selected
```

Do not copy this list directly into a manifest. Availability and compatibility depend on the exact Unity editor and registry setup.

## Input ownership

Choose one UI event path per scene.

### XRI path

- `EventSystem`
- `XRUIInputModule`
- `TrackedDeviceGraphicRaycaster` on interactive canvases
- XRI controller and/or hand interactors

### Meta Interaction SDK path

- Meta pointable Canvas components
- Meta ray/poke interactors and the matching scene module
- the camera/interaction rig required by the selected Meta SDK release

Do not keep `StandaloneInputModule`, `InputSystemUIInputModule`, `XRUIInputModule`, and a Meta pointable module active together without an explicit, tested architecture.

## Canvas conventions

Pick a documented conversion between Canvas units and metres. A common starting convention is scale `0.001`, making one Canvas unit one millimetre. The convention is useful only when the whole prefab follows it.

- Parent Canvas owns the unit conversion.
- Children remain at local scale one.
- `sizeDelta` and anchors define size.
- Decorative `Graphic.raycastTarget` values stay false.
- Each prefab records intended physical width, height, and viewing distance.
- Avoid automatic font sizing for critical text until device behaviour is characterized.

## MRUK placement policy

Each room-aware prefab should define:

- required semantic labels;
- acceptable surface normal range;
- minimum free rectangle and edge clearance;
- distance and height limits;
- whether occlusion is required;
- collision/overlap rejection;
- fallback when no valid surface exists;
- persistence and relocalization behaviour.

Keep room queries separate from UI presentation. This makes missing permission and missing room data testable without reconstructing the UI.

## Unity Editor automation boundary

Before executing generated C#:

1. inspect available MCP tool names;
2. save the current scene if the user has authorized scene edits;
3. make the script idempotent;
4. avoid arbitrary filesystem, process, shell, network, or credential access;
5. limit edits to the resolved Unity project;
6. log every created, changed, or deleted Unity object;
7. compile and inspect the console;
8. capture a scene or Game View screenshot where possible.

Meta examples may mention `Unity_RunCommand` or `meta_add_*` calls. Those commands belong to a separate Unity extension and are not guaranteed by the Meta `metavr` MCP. Map the intent to the configured bridge only after checking its actual API.
