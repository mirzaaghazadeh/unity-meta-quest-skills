---
name: quest3-verify-first
description: Verifies current Meta Quest 3, Horizon OS, Meta XR, OpenXR, Unity package, permission, and device claims against primary sources and the user's actual local project before code or configuration changes. Use for every Quest 3 Unity, MR, Movement SDK, MRUK, passthrough, hand tracking, body tracking, A2E, Android manifest, build, or store task.
license: Apache-2.0
compatibility: Requires access to the target project and live primary documentation. Unity, device, browser, and MCP capabilities are detected at runtime.
metadata:
  version: "0.1.0"
---

# Quest 3 Verify First — Safe Edition

Quest and Unity packages change quickly. Verify before editing. This edition does not auto-download or execute `metavr`, does not install npm packages, and does not assume a Quest device or Unity Editor bridge is connected.

## Verification gate

Before making a Quest-specific claim or change:

1. Resolve the actual Unity project from `ProjectSettings/ProjectVersion.txt`.
2. Read `Packages/manifest.json` and `Packages/packages-lock.json` when present.
3. List installed Unity editors. Report an exact-version mismatch.
4. Inspect the MCP/tool list. Distinguish:
   - Blender Editor control;
   - Unity Editor control;
   - Quest device/ADB control;
   - documentation or web access.
5. Search current primary documentation for each unstable claim. Prefer Meta, Unity, Khronos, Android, or the package's official repository.
6. Open the supporting page. Verify the exact package ID, version range, API signature, permission, device support, and publication date where relevant.
7. Cite the page beside the resulting recommendation.

Do not treat a search snippet, cached blog, generated answer, or example project as the current compatibility matrix.

## Hardware and inference boundary

Do not infer a tracking capability from the product family, a sample name, or another headset. For the exact target device and runtime, verify the official support table, required permission, input source, and SDK version. Keep these signal paths separate:

- measured hardware sensor data;
- microphone-derived expression inference such as Audio-to-Expression;
- camera- or model-derived software inference;
- synthetic gaze, blink, idle, or fallback animation.

Label the input source in the implementation and test record. Do not describe inference or synthetic animation as measured eye, face, or body tracking.

## Tool safety

- Never run `npx -y metavr`, install `metavr`, or enable its MCP implicitly.
- If a separately audited `metavr` installation exists, query its version and consent/telemetry state before use.
- Never assume examples named `Unity_RunCommand`, `Unity_ImportExternalModel`, or `meta_add_*` are connected tools. Inspect the actual Unity bridge first.
- Generated Unity C# runs with Editor permissions. Keep it bounded to the resolved project, review it, and log every mutation.
- Before device commands, confirm that the requested install, launch, capture, or diagnostic action is authorized. List connected devices and target an explicit serial when more than one exists.
- Do not install, uninstall, clear, reboot, delete files, change permissions, or upload a build unless the user request authorizes that action.

## Project decision record

For package or platform work, return a small record:

```text
Unity project:
Required/installed editor:
Target device:
Render pipeline:
XR provider:
Interaction stack:
Meta packages and exact versions:
Claim verified:
Primary source and date checked:
Local evidence:
Mutation proposed/performed:
Remaining uncertainty:
```

## Stop conditions

Stop before mutation when:

- the required Unity editor is absent;
- package compatibility cannot be established from primary sources;
- the target Unity project is ambiguous;
- a proposed package change would reserialize an existing user project without a safe copy or version control;
- the task requires a Quest device and none is connected;
- a tool example cannot be mapped to an actually connected bridge.

Documentation-only planning may continue. Label untested steps plainly.

## Sources

Use [primary sources](references/primary-sources.md) as the starting index. Browse live for current versions and requirements.
