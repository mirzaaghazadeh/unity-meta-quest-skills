# Meta Quest VRC reference (all active VRCs)

Source: https://developers.meta.com/horizon/resources/publish-quest-req/ (updated 2026-08-19).
The columns give the Store requirement level for **immersive** apps and **panel (2D)** apps:
R = Required, Rec = Recommended, N/A. The "Requirement" text summarizes Meta's wording. The
"Unity check / test" column is our guidance, not Meta's text. Detail pages live at
`https://developers.meta.com/horizon/resources/vrc-quest-<area>-<n>/` (for example
`vrc-quest-input-4`). The Content and Publishing pages use `vrc-content-<n>` and
`vrc-publishing-<n>`; that pattern is unverified, so search the index if a link fails.

## Packaging

| ID | Imm | 2D | Requirement | Unity check / test |
|---|---|---|---|---|
| VRC.Quest.Packaging.1 | R | R | Manifest meets release-build manifest requirements; checked on every upload | `verify-quest-apk.sh`; see `horizon-store-publish-unity` Step 1 |
| VRC.Quest.Packaging.2 | R | R | APK Signature Scheme v2 | Custom keystore; `apksigner verify --verbose` shows v2 true |
| VRC.Quest.Packaging.3 | R | R | No Android features unsupported on Quest | Remove phone, telephony, GPS and Google Play Services-dependent features; check `uses-feature` entries in the merged manifest |
| VRC.Quest.Packaging.4 | R | R | Supported SDK and engine version. OpenXR loader must come from the Khronos OpenXR SDK or Meta's proprietary loader (since 2024-04-30). Valid network security config | Unity ≥ Meta XR SDK minimum (6000.0.66f2 for v207); Unity OpenXR plugin; check the Test Status column after upload |
| VRC.Quest.Packaging.5 | R | R | APK < 1 GB; each OBB < 4 GB | Split Application Binary or asset files |
| VRC.Quest.Packaging.6 | R | R | 64-bit binaries | IL2CPP + ARM64 only |

## Audio

| ID | Imm | 2D | Requirement | Unity check / test |
|---|---|---|---|---|
| VRC.Quest.Audio.1 | Rec | Rec | Support 3D audio spatialization | Meta XR Audio SDK or Unity spatializer on 3D sources |

## Performance

| ID | Imm | 2D | Requirement | Unity check / test |
|---|---|---|---|---|
| VRC.Quest.Performance.1 | R | Rec | Render ≥ 60 fps. Interactive apps use 72/80/90/96/100/120 Hz (96/100/120 not on all devices); media apps may use 60 Hz where supported. AppSW may render at half rate (≥ 30 fps) with good motion vectors. Exempt: loading screens, fades, black screens, system overlays | OVR Metrics Tool over the content length or 45 min, whichever is shorter. Look for extended periods below 60 fps |
| VRC.Quest.Performance.2 | - | - | Retired | - |
| VRC.Quest.Performance.3 | R | Rec | Head-tracked graphics within 4 s of launch, or a VR loading indicator | Small boot scene plus System Splash Screen; time a cold start |
| VRC.Quest.Performance.4 | Rec | N/A | ≥ 85% render scaling for most of the experience (brief dips, such as boss fights, allowed) | OVR Metrics "Render Scale Percent" graph; dynamic resolution min ≥ 0.85 for most content |

## Functional

| ID | Imm | 2D | Requirement | Unity check / test |
|---|---|---|---|---|
| VRC.Quest.Functional.1 | R | R | Installs and runs without crashes, freezes or extended unresponsiveness | 45+ min soak; `metavr app launch --verify`; check the logcat crash buffer |
| VRC.Quest.Functional.2 | R | Rec | Single-player apps pause when Horizon OS asks: immersive views such as Change Boundary or Enter Passthrough, Power button, headset off past Auto Sleep (not Quest 3S, which has no proximity sensor). Non-interactive ambience may continue. Not required for multiplayer | `OnApplicationPause`, `InputFocusLost` → pause. Test: Universal Menu, passthrough from Quick Settings |
| VRC.Quest.Functional.3 | R | R | Never leave the user stuck | Every UI state has an exit; no soft-locks after focus loss |
| VRC.Quest.Functional.4 | R | R | Never lose user data | Save on pause; atomic writes to `Application.persistentDataPath` |
| VRC.Quest.Functional.5 | R | R | Respond to positional tracking as well as orientation | 6DoF rig |
| VRC.Quest.Functional.6 | R | R | Only Meta Quest headsets/controllers in title, Store assets, in-app (cross-platform players may show their own) | Use SDK controller models; inspect for 45 min |
| VRC.Quest.Functional.7 | Rec | Rec | Notify users when internet is unavailable but needed | `Application.internetReachability` check plus an in-VR message |
| VRC.Quest.Functional.8 | - | - | Retired | - |
| VRC.Quest.Functional.9 | R | N/A | Local tracking space: user can reset forward orientation (not applicable to Stage) | Don't override the system recenter; re-place world UI on recenter |
| VRC.Quest.Functional.10 | Rec | N/A | Avoid head-locked menus and UI | World-locked or lazy-follow canvases (`hz-unity-meta-quest-ui`) |
| VRC.Quest.Functional.11 | - | - | Retired | - |
| VRC.Quest.Functional.12 | R | R | Full functionality for multiple entitled users on the headset | Per-user saves; test primary and secondary accounts |
| VRC.Quest.Functional.13 | Rec | Rec | Default to the user's language, falling back to English | `Application.systemLanguage` or the Unity Localization selector |
| VRC.Quest.Functional.14 | R | Rec | Passthrough-capable apps show passthrough loading screens and launch in passthrough from MR Home | OVRManager System Splash Screen Background = Passthrough (Contextual) |

## Security

| ID | Imm | 2D | Requirement | Unity check / test |
|---|---|---|---|---|
| VRC.Quest.Security.1 | Rec | Rec | Entitlement check within 10 s of launch; on failure exit, show an error, or enter a limited demo. "Apps are not required to perform a platform entitlement check." | `Core.AsyncInitialize` + `Entitlements.GetIsViewerEntitled` in the boot scene (`hz-unity-platform-sdk`). Test: a non-entitled account, ADB install, launch from Library |
| VRC.Quest.Security.2 | R | R | Minimum permissions; none prohibited; review-required ones justified; denying a runtime prompt must not crash or hang | `verify-quest-apk.sh` permission scan; deny each prompt on device |

## Tracking

| ID | Imm | 2D | Requirement | Unity check / test |
|---|---|---|---|---|
| VRC.Quest.Tracking.1 | R | N/A | Metadata play modes are honored. Sitting: fully playable seated, no standing, no reaching to the floor, no turning > 90°. Standing and roomscale: fully usable in a 6.5' × 6.5' area | Reachability tuning; height calibration; snap/smooth turn |
| VRC.Quest.Tracking.2 | R | R | Each declared input modality supports targeting, selection, locomotion and all gameplay alone from launch | Play the whole game per modality |

## Input

| ID | Imm | 2D | Requirement | Unity check / test |
|---|---|---|---|---|
| VRC.Quest.Input.1 | Rec | Rec | In-game menu opens with the gamepad menu button or the left Touch menu button | Map `OVRInput.Button.Start` (left menu) to pause/menu. Verify the API name with `metavr docs api-details OVRInput` |
| VRC.Quest.Input.2 | Rec | N/A | Pick up objects with grip, not trigger | ISDK grab uses grip by default; check custom grab code |
| VRC.Quest.Input.3 | Rec | N/A | Hands and controllers match real position and orientation as closely as possible | Use SDK anchors and offsets; no hand-tuned offsets |
| VRC.Quest.Input.4 | R | N/A | Focus-aware: keep rendering, hide hands and controllers (or rest them at the sides), ignore input when unfocused; resume on refocus | `OVRManager.InputFocusLost/Acquired`, `OVRManager.hasInputFocus` |
| VRC.Quest.Input.5 | Rec | N/A | Hand-tracked hands render and animate correctly | OVRHand/ISDK hand visuals; test both hands |
| VRC.Quest.Input.6 | - | - | Retired | - |
| VRC.Quest.Input.7 | R | R | Correctly handle switching between controllers and hands | Auto-switch in settings; pick up and put down controllers repeatedly |
| VRC.Quest.Input.8 | R | R | Hand-tracking system gesture reserved; it must not trigger app actions | Don't bind actions to the system gesture |

## Asset (see `horizon-store-listing-assets` for dimensions)

| ID | Imm | 2D | Requirement |
|---|---|---|---|
| VRC.Quest.Asset.1 | R | R | Logo on a transparent background |
| VRC.Quest.Asset.2 | R | R | Cover art (Landscape, Square, Portrait, Hero, Spatialized tile) has clear branding with no extra text, taglines or banners |
| VRC.Quest.Asset.3 | Rec | Rec | No text in the top or bottom 20% of cover art |
| VRC.Quest.Asset.4 | Rec | Rec | Hero art has centered branding and/or title |
| VRC.Quest.Asset.5 | R | R | Screenshots represent the app, with no extra logos, text or icons |
| VRC.Quest.Asset.6 | R | R | Description, screenshots and videos show no other VR platforms' headsets, controllers or logos |
| VRC.Quest.Asset.7 | R | R | Trailer ≤ 2 minutes |
| VRC.Quest.Asset.8 | Rec | Rec | Artwork text ≥ 24 pt |
| VRC.Quest.Asset.9 | R | R | Immersive Image Layer images (Left, Right, Logo) on transparent backgrounds |
| VRC.Quest.Asset.10 | Rec | N/A | Media showing Quest Pro-exclusive features carries "Captured on Meta Quest Pro." |

## Ads (only if the app shows ads)

| ID | Imm | 2D | Requirement |
|---|---|---|---|
| VRC.Quest.Ads.1 | R | R | Meet all advertising policy requirements |
| VRC.Quest.Ads.2 | R | R | "Contains Ads" label on the product details page |
| VRC.Quest.Ads.3 | R | R | Ads not stereoscopic, head-tracked or immersive |
| VRC.Quest.Ads.4 | R | R | Interfering ads have a clear dismissal |
| VRC.Quest.Ads.5 | R | R | Interfering ads don't appear after each of several consecutive user actions |
| VRC.Quest.Ads.6 | R | R | Ads don't impair device functionality |
| VRC.Quest.Ads.7 | R | R | Ads don't cause inadvertent clicks (no mimicking OS notifications or app UI) |

## Accessibility (all Recommended)

| ID | Imm | 2D | Recommendation |
|---|---|---|---|
| VRC.Quest.Accessibility.1 | Rec | Rec | Playable without audio (subtitles, visual cues) |
| VRC.Quest.Accessibility.2 | Rec | Rec | Text and progression-critical controls clearly legible |
| VRC.Quest.Accessibility.3 | Rec | Rec | Direction through visual, audio and/or haptic feedback |
| VRC.Quest.Accessibility.4 | Rec | Rec | One-handed or one-controller option |
| VRC.Quest.Accessibility.5 | Rec | Rec | Adjustable display settings such as brightness and contrast |
| VRC.Quest.Accessibility.6 | Rec | Rec | Color-blind options or non-color cues |
| VRC.Quest.Accessibility.7 | Rec | Rec | Rotate the view without moving the head or neck (snap/smooth turn) |
| VRC.Quest.Accessibility.8 | Rec | Rec | Multiple locomotion styles |
| VRC.Quest.Accessibility.9 | Rec | N/A | Sitting/standing apps offer fixed-position interaction |

## Streaming (only for streaming apps)

| ID | Store | Requirement |
|---|---|---|
| VRC.Quest.Streaming.1 | Rec | Handle connectivity issues gracefully |
| VRC.Quest.Streaming.2 | R | Stereoscopic, head-tracked or immersive streaming only from a local PC the customer physically accesses, unless Meta approves otherwise |
| VRC.Quest.Streaming.3 | R | Virtual or cloud streaming displays connectivity notices |
| VRC.Quest.Streaming.4 | R | Virtual or cloud streaming not directed at children under 13 |

## Privacy Policy

| ID | Imm | 2D | Requirement |
|---|---|---|---|
| VRC.Quest.Privacy.1 | R | R | Privacy Policy URL links to a policy managed by the app's team |
| VRC.Quest.Privacy.2 | R | R | Explains what user data is collected |
| VRC.Quest.Privacy.3 | R | R | Explains how user data is used |
| VRC.Quest.Privacy.4 | R | R | Explains how users can request deletion |
| VRC.Quest.Privacy.5 | R | R | Team and app pass data protection checks |

## Content

| ID | Imm | 2D | Requirement |
|---|---|---|---|
| VRC.Content.1 | R | R | Meet all content guidelines |
| VRC.Content.2 | R | R | App metadata matches in-app content |
| VRC.Content.3 | R | R | UGC apps provide a way to report Code of Conduct violations |
| VRC.Content.4 | Rec | Rec | UGC apps let users immediately hide undesired content |

## Publishing

| ID | Imm | 2D | Requirement |
|---|---|---|---|
| VRC.Publishing.1 | R | R | App website URL links directly to a valid page |
| VRC.Publishing.2 | R | R | External Support Link (if present) goes to a valid support page |
| VRC.Publishing.3 | R | R | Terms of Service URL (if present) goes to a valid TOS page |
| VRC.Publishing.4 | R | R | App name meets content guidelines |
| VRC.Publishing.5 | R | R | Short description meets content guidelines |
| VRC.Publishing.6 | R | R | Long description meets content guidelines |
| VRC.Publishing.7 | R | R | Search keywords relevant and within guidelines |
| VRC.Publishing.8 | R | R | Any Meta brand use in metadata follows the Brand Guidelines |

Last verified: 2026-10-16
