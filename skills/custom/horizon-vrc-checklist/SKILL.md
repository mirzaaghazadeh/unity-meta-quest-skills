---
name: horizon-vrc-checklist
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.2.0"
description: >-
  Pre-submission audit of a Unity Meta Quest game against Meta's current
  Virtual Reality Checks (VRCs) for the Horizon Store, by VRC ID
  (VRC.Quest.Packaging/Performance/Functional/Security/Tracking/Input/Asset/
  Privacy, VRC.Content, VRC.Publishing). Covers what to check in Unity code and
  settings, how to test each on a headset with metavr, adb and OVR Metrics Tool,
  and an evidence table showing pass/fail per VRC. Use when the user asks "will
  this pass review", "run the VRC checklist", "check store requirements", "why
  was my app rejected (VRC...)", "frame rate requirement", "focus awareness",
  "pause when headset removed", "entitlement check required?", "is my game
  ready for Meta VR Glasses", or before Submit for Review. Takes precedence over
  the generic VRC notes in Meta's hz-store-submit, which are partly out of date.
---

# Horizon Store VRC audit for Unity projects

> Made by [Navid Mirzaaghazadeh](https://github.com/mirzaaghazadeh) · part of [unity-meta-quest-skills](https://github.com/mirzaaghazadeh/unity-meta-quest-skills) · MIT

Meta's VRC guide (last updated 2026-08-19, unchanged on 2026-10-17) lists 84 VRC IDs, of which
80 are active. The four retired ones are Performance.2, Functional.8, Functional.11 and Input.6.
Connect 2026 added **no VRC for Meta VR Glasses**; the readiness checks for that device are in
the section "Meta VR Glasses readiness" below and are not review requirements.
The full table, with a Unity check and a device test for every VRC, is in
[references/vrc-reference.md](references/vrc-reference.md). Unity code patterns are in
[references/unity-implementation.md](references/unity-implementation.md).

**The individual VRC pages are authoritative.** Meta warns that its downloadable test plans
(.xls/.csv) are "slightly out of date". Meta's "Common VRC failures" page (2026-05-01) and
Meta's own `hz-store-submit` / `hz-unity-platform-sdk` skills still carry older wording.
Where they disagree, the VRC page wins:

| Topic | Current VRC page says | Older text says |
|---|---|---|
| VRC.Quest.Security.1 entitlement | **Recommended**: "Apps are not required to perform a platform entitlement check." Check within 10 s; on failure exit, show an error, **or** enter a limited demo (page 2025-04-08) | "must", required, must quit |
| VRC.Quest.Performance.1 | Render at **≥ 60 fps** at a refresh rate of 72/80/90/96/100/120 Hz. AppSW may halve the rate (≥ 30 fps). 45-minute OVR Metrics check (page 2025-10-22) | "72 fps"; 90 Hz on Quest 3 |
| VRC.Quest.Input.6 (hide low-confidence hands) | **Retired** | listed as active |

Still implement the entitlement check for paid games. It's cheap anti-piracy (`hz-unity-platform-sdk`).
A game with no check won't fail Security.1, though.

## Workflow

1. **Classify the app.** These answers decide which VRCs apply. Record them at the top of the report.
   - Immersive or panel (2D). 2D apps have a different required set, shown in the 2D column of the reference.
   - Input modalities declared in metadata: controllers, hands, or both (Tracking.2, Input.7).
   - Play area: sitting, standing or roomscale (Tracking.1).
   - Single-player or multiplayer (Functional.2 applies only to single-player).
   - Local or Stage tracking space (Functional.9 applies to Local).
   - Launches in passthrough or MR (Functional.14). Ads (Ads.1-7). UGC (Content.3/4). Cloud streaming (Streaming.*).
   - Whether the app uses Platform SDK features, which triggers the Data Use Checkup and Privacy.5.
   - Target devices, including whether the build will be marked **Future devices** (Meta VR Glasses).
2. **Static audit of the project**, with no headset (see the table below). Search the code with
   Grep for the APIs named, inspect Player and XR settings, and run
   `horizon-store-publish-unity/scripts/verify-quest-apk.sh` on a release APK. That covers
   Packaging.1-6 and Security.2.
3. **Device tests.** Use a release-signed build on each target headset. Run `metavr device list`
   first (see `hz-quest-verify-first`). Install OVR Metrics Tool with `metavr tools install ovrmetric`.
4. **Report.** Write one row per applicable VRC: `ID | Req/Rec | Status (PASS/FAIL/RISK/N/A) | Evidence | Fix`.
   List FAILs on Required VRCs first. Don't mark PASS without evidence such as a log line, a
   screenshot, an OVR Metrics capture or a code reference.
5. Before stating any requirement, re-check the VRC page (`metavr docs fetch https://developers.meta.com/vr/resources/vrc-quest-<area>-<n>/`)
   if it is older than the "Last verified" date below.

## Required VRCs Unity games fail most, and what to check

| VRC | Requirement (Store, immersive) | Check in Unity | Test on device |
|---|---|---|---|
| Packaging.1/.4 | Release manifest rules; supported SDK/engine. Checked automatically on upload | `verify-quest-apk.sh`: targetSdk 32-34 (34 for new apps), minSdk 29-34, VR category, headtracking, supportedDevices, debuggable off, installLocation auto, excludeFromRecents. Unity at or above Meta XR SDK minimum (6000.0.66f2 for v207) | Upload to ALPHA and read Manage Builds > Test Status |
| Packaging.2/.5/.6 | APK Sig v2; APK < 1 GB, OBB < 4 GB; 64-bit | Custom keystore, ARM64 only, IL2CPP, Split Application Binary if big | `apksigner verify --verbose` |
| Performance.1 | ≥ 60 fps rendering, no extended dips (30 with AppSW) | Target refresh rate set (`OVRPlugin.systemDisplayFrequency`), FFR, MSAA, draw-call budgets (`quest-unity-project-settings`) | OVR Metrics Tool for the content length or 45 min, whichever is shorter. Look for sustained dips below 60. Loading screens and fades are exempt |
| Performance.3 | Head-tracked graphics within 4 s of launch, or a VR loading indicator | Lightweight boot scene; System Splash Screen set in OVRManager > Quest Features > General | `metavr app launch <pkg> --cold-start --measure-launch-time`, then count seconds in the headset |
| Functional.1 | No crashes, freezes or long hangs | Async loading off the main thread; null-safe first-run save data | Play 45 min+; `metavr log --level E`; `metavr app launch --verify` |
| Functional.2 | Single-player pauses when the OS asks | `OnApplicationPause(true)` stops simulation and input; Universal Menu → `OVRManager.InputFocusLost` pauses gameplay | Open the Universal Menu, enter passthrough from Quick Settings, press Power. All interactive elements must stop |
| Functional.3/.4 | Never stuck; never lose user data | Every menu has a way back; save on pause/quit (`OnApplicationPause`), not only on clean exit | Force-stop mid-session (`metavr app stop <pkg>`), relaunch, check progress |
| Functional.5 | Responds to position as well as orientation | 6DoF rig (OVRCameraRig, tracking origin not stationary-only) | Lean and step, and confirm the view translates |
| Functional.6 | Only Meta Quest headsets/controllers in app and assets | No generic or other-brand controller models; use the SDK controller models | Play 45 min and inspect the store media |
| Functional.9 | Local tracking space: user can reset forward orientation | Don't block system recenter; re-anchor UI after recenter | Turn 180°, long-press the recenter button, and confirm forward resets |
| Functional.12 | Full function for multiple entitled users | No cached user ID or entitlement across accounts; per-user save paths keyed by platform user ID | Two entitled accounts, primary and secondary; play as each |
| Functional.14 | Passthrough-capable apps: passthrough splash, launch in passthrough from MR Home | OVRManager System Splash Screen Background = Passthrough (Contextual), set automatically when Passthrough Support ≠ None | Enable passthrough in MR Home, launch, watch the splash |
| Security.2 | Minimum permissions; none prohibited | Merged manifest; strip extras with `tools:node="remove"` | Deny each runtime prompt; the app must not crash or hang |
| Tracking.1 | Meets the declared sitting/standing/roomscale mode | Standing/roomscale: everything usable in 6.5' × 6.5'. Sitting: no standing, no reaching to the floor, no turning > 90° | Play several levels in each declared mode |
| Tracking.2 | Each declared input mode works alone from launch | Hands-only declared → menus, locomotion and gameplay all work with hands only. The merged manifest declares input too (`oculus.software.handtracking` required=false = both, true = hands only); `verify-quest-apk.sh` prints it | Play through with each modality |
| Input.4 | Focus-aware: keep rendering, hide hands/controllers, ignore input when unfocused | `OVRManager.InputFocusLost` / `InputFocusAcquired` or `OVRManager.hasInputFocus` | Open the Universal Menu: the scene still renders, hands are hidden, in-app input is ignored |
| Input.7 | Correct controller ↔ hands switching | Hand and controller visuals and interactors swap cleanly (ISDK handles this, see `hz-unity-meta-interaction-sdk`) | Enable auto-switch in device settings; repeatedly pick up and put down the controllers |
| Input.8 | System gesture reserved | No app action bound to the palm-pinch Meta/menu gesture | Do the system gesture and confirm no in-app reaction |
| Asset.1/2/5/6/7/9 | Logo transparent; cover art branding only; real screenshots without extra text; no other platforms; trailer ≤ 2 min; immersive image layers transparent | `horizon-store-listing-assets` | Asset review |
| Privacy.1-5 | Own privacy policy URL covering collection, use and deletion; pass data-protection checks | `horizon-store-listing-assets` | Open the URL |
| Content.1-3, Publishing.1-8 | Content guidelines; metadata matches the app; UGC report flow; valid URLs; name/description/keyword rules | `horizon-store-listing-assets` | Review the metadata |

Recommended but valuable: Security.1 (entitlement), Performance.4 (≥ 85% render scale for most
of the game), Input.1 (menu button opens the in-game menu), Input.2 (grip picks up objects,
not trigger), Functional.7 (tell the user when internet is missing), Functional.10 (avoid
head-locked UI), Functional.13 (default to the system language, falling back to English),
Audio.1 (spatial audio), and Accessibility.1-9.

## Meta VR Glasses readiness (not VRCs, but check them before marking Future devices)

Meta VR Glasses ship in spring 2027 without controllers; the Store shows hand-capable titles first
to owners without controllers. Report these as a separate block, never as VRC FAILs:

| Check | Pass condition | Source |
|---|---|---|
| 64-bit binary | No `armeabi-v7a`-only libraries (32-bit-only binaries don't run at all) | app-compatibility (2026-10-13) |
| Input path | Controllers-required apps install but hit a **Switch to Controllers** dialog. For reach, every required flow works with hands (gaze + pinch via ISDK Gaze Interaction) | app-compatibility; test-your-app (2026-09-25) |
| Field of view | Key UI readable inside 70° × 66°: `metavr device fov-sim enable` on a Quest 3/3S/Pro, `--perceived` for 58° × 54°, `disable` after | field-of-view (2026-09-25) |
| No hover-only actions | Gaze is private, hover never fires | Connect recap (2026-09-24) |
| Store targeting | The build's Supported Devices include Future devices (see `horizon-store-publish-unity` Step 5a) | target-builds-by-device (2026-09-04) |
| Performance | Same VRC.Quest.Performance.1 bar; thermals and the compute puck are device-only checks | test-your-app |

Unity implementation (Gaze Interaction, Device Readiness Check, simulator profile) belongs to
`meta-vr-glasses-unity` and `hz-unity-device-readiness`.

## Device test session (copy and adapt)

```bash
metavr device list
metavr app install --replace Builds/Quest/app.apk
metavr adb logcat --clear
metavr app launch com.company.game --cold-start --wait-for-idle --measure-launch-time --verify   # Performance.3, Functional.1
# Manual in headset: Universal Menu open/close (Input.4, Functional.2), passthrough via Quick Settings,
# Power off/on, recenter after a 180° turn (Functional.9), controller<->hands switching (Input.7)
metavr log --tag Unity --level W
metavr adb logcat --buffer crash
metavr capture screenshot -o evidence/focus-lost.png
```

For performance evidence, run an OVR Metrics Tool session over the content length or 45 minutes
and save its log or screenshot. For trace-level analysis use `hz-perfetto-debug`.

## Sources

Pages moved from `developers.meta.com/horizon/` to `/vr/` (old links redirect).

- https://developers.meta.com/vr/resources/publish-quest-req/ (VRC index, updated 2026-08-19)
- https://developers.meta.com/vr/resources/vrc-quest-performance-1/ (2025-10-22), -performance-3/ (2024-07-31), -performance-4/ (2024-07-31)
- https://developers.meta.com/vr/resources/vrc-quest-functional-2/ (rev. 2025-09-05), -functional-6/ (2026-03-30), -functional-9/, -functional-12/, -functional-14/ (2024-07-31)
- https://developers.meta.com/vr/resources/vrc-quest-security-1/ (2025-04-08), -security-2/ (2026-03-30)
- https://developers.meta.com/vr/resources/vrc-quest-input-4/ (2025-09-26), -input-7/ (2024-07-31)
- https://developers.meta.com/vr/resources/vrc-quest-tracking-1/ (2025-05-15), -tracking-2/ (2025-12-16)
- https://developers.meta.com/vr/resources/vrc-quest-packaging-1/, -packaging-4/, -packaging-5/
- https://developers.meta.com/vr/resources/publish-common-vrc-failures/ (2026-05-01)
- https://developers.meta.com/vr/documentation/unity/unity-lifecycle/ (2026-10-07)
- https://developers.meta.com/vr/documentation/unity/unity-splash-screen/ (2026-10-09)
- https://developers.meta.com/vr/essentials/app-compatibility/ (2026-10-13), /test-your-app/ (2026-09-25), /field-of-view/ (2026-09-25)
- https://developers.meta.com/vr/resources/publish-release-channels-device-targeting/ (2026-09-04)

Last verified: 2026-10-17 (the recenter timing in references/unity-implementation.md: observed on a
Quest 3S and in the core SDK 207 source, 2026-10-25)
