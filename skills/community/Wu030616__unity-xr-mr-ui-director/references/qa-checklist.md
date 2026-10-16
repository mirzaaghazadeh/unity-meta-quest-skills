# Quest 3 MR UI QA checklist

## Static project gate

- [ ] Installed Unity editor matches `ProjectVersion.txt`.
- [ ] Package versions are locked and compatible.
- [ ] Android/Quest build target is intentional.
- [ ] Exactly one active EventSystem path exists in each tested scene.
- [ ] Canvas raycaster matches the selected interaction stack.
- [ ] TMP/SDF font assets resolve without missing references.
- [ ] No decorative graphic receives raycasts.
- [ ] Physical panel and target dimensions are documented.

## Interaction gate

- [ ] Controller ray can aim, hover, activate, cancel, and recover.
- [ ] Hand ray works at the intended distance.
- [ ] Direct poke works only inside its intended near range.
- [ ] Hover and press feedback remain visible under hand occlusion.
- [ ] Destructive actions have confirmation or undo.
- [ ] Tracking loss produces a visible recoverable state.
- [ ] One-handed and seated paths cover core tasks where promised.

## Mixed-reality gate

- [ ] Scene permission granted path tested.
- [ ] Permission denied path tested.
- [ ] No room data path tested.
- [ ] Valid and invalid wall/table/floor candidates tested.
- [ ] Doorways, narrow fragments, clutter, and overlaps are rejected.
- [ ] Relocalization and anchor restoration tested.
- [ ] Bright, dark, reflective, cluttered, and low-contrast backgrounds tested.
- [ ] Virtual/real occlusion behaviour matches the design.

## Readability and comfort gate

- [ ] Tested on Quest 3, not only Game View or simulator.
- [ ] Body and head motion do not cause a rigid HUD to chase the eyes.
- [ ] Primary text is readable at the documented distance.
- [ ] Critical information stays near the intended view zone.
- [ ] State is not encoded by colour alone.
- [ ] Reduced-motion and recenter controls work where applicable.

## Performance gate

- [ ] Device refresh-rate target is documented and current guidance verified.
- [ ] Frame time tested on device through representative interaction.
- [ ] Canvas rebuild spikes measured.
- [ ] Overdraw/transparency reviewed in passthrough scenes.
- [ ] CPU, GPU, thermal, and memory behaviour recorded.
- [ ] Screenshot, log, and trace evidence paths preserved.

## Sign-off

Record failures plainly. A simulator pass does not satisfy the Quest 3 gate. A single clean room does not satisfy the MR background gate.
