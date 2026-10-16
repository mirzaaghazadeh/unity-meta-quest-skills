# Errata: where Meta's official skills disagree with Meta's live docs

The skill folders here are kept **unchanged** from upstream (commit `3a8553d`, still the latest on
2026-10-17). Some values in them are older than Meta's own developer pages. Checked on 2026-10-16
and again on 2026-10-17 after Meta Connect 2026. Where the two disagree, trust the live page and
the custom skills in `../custom/`, which were written from those pages.

Meta's developer pages moved from `developers.meta.com/horizon/...` to `developers.meta.com/vr/...`
(HTTP 302, `/vr/` is canonical). The `/horizon/` and `developer.oculus.com` links in these skills
still redirect and work, except the four dead links listed below.

| Skill | What the skill says | What Meta's docs say now | Source |
|---|---|---|---|
| `hz-store-submit` | `targetSdkVersion >= 32`, `minSdkVersion >= 29` | targetSdk **32–34** for immersive apps, and **apps created since 2026-03-01 must target 34**. minSdk 29–34, with 32 recommended | [publish-mobile-manifest](https://developers.meta.com/vr/resources/publish-mobile-manifest/) (2026-10-15) ✔ re-checked |
| `hz-store-submit` | Icon 1024×1024; hero 2560×1440; at least 3 screenshots; small landscape 1280×720 | Icon **512×512** (24-bit PNG, no transparency); hero **3000×900**; covers 2560×1440 / 1440×1440 / 1008×1440; **5** screenshots at 2560×1440 | [asset-guidelines](https://developers.meta.com/vr/resources/asset-guidelines/) (2026-05-11) ✔ re-checked |
| `hz-store-submit` | Name 30, short description 150, long description 4000 characters | **40 / 500 / 1500** | [publish-app-metadata](https://developers.meta.com/vr/resources/publish-app-metadata/) (2025-08-20) |
| `hz-store-submit` | Trailer 30–60 s | **30 s to 2 min**, MP4/H.264/AAC, 1080p–2K | [asset-guidelines](https://developers.meta.com/vr/resources/asset-guidelines/) ✔ re-checked |
| `hz-store-submit` (SKILL.md, references/rejection-troubleshooting.md) | 72 Hz minimum on Quest 2, **90 Hz on Quest 3 and Meta VR Glasses**; load under 15 s | VRC.Quest.Performance.1: render at **≥ 60 fps** on every device (no VR Glasses-specific VRC). Performance.3: graphics within **4 s** or a loading indicator | [publish-quest-req](https://developers.meta.com/vr/resources/publish-quest-req/) (2026-08-19) ✔ re-checked 2026-10-17; [test-your-app](https://developers.meta.com/vr/essentials/test-your-app/) (2026-09-25) applies the same Performance.1 bar to VR Glasses |
| `hz-store-submit` (references/submission-checklist.md) | "App launches successfully on Meta VR Glasses, Quest 3, and Quest 3S" | VR Glasses hardware ships in spring 2027. Test with simulators and `metavr device fov-sim` on a Quest 3/3S; Store delivery to VR Glasses is set by **Future devices** in the build's Supported Devices | [test-your-app](https://developers.meta.com/vr/essentials/test-your-app/) (2026-09-25), [app-compatibility](https://developers.meta.com/vr/essentials/app-compatibility/) (2026-10-13) |
| `hz-store-submit` (upload section), `metavr-cli` | `metavr store dist upload` as the upload path | It has no age-group, device-target, asset-file or symbol options (metavr 1.8.0.17.10). For targeted release builds use `ovr-platform-util upload-quest-build --age-group ... --channel "alpha:quest3+"` | [target-builds-by-device](https://developers.meta.com/vr/resources/publish-release-channels-device-targeting/) (2026-09-04); `docs/metavr-cli-reference.md` |
| `hz-store-submit`, `hz-unity-platform-sdk` | Paid apps must check entitlement and quit, or they are auto-rejected | VRC.Quest.Security.1 is now **Recommended**. On failure, exiting, showing an error or a limited demo are all allowed | [vrc-quest-security-1](https://developers.meta.com/vr/resources/vrc-quest-security-1/) (2025-04-08) |
| `hz-unity-platform-sdk` (references/abuse-report.md, application.md, consent.md, rate-and-review.md) | "Documentation" links to `developer.oculus.com/documentation/unity/ps-abuse-reporting/`, `/ps-application/`, `/ps-consent-management/`, `/ps-rate-and-review/` | All four return **404**. Closest current pages: [User Reporting Plugin](https://developers.meta.com/vr/resources/reporting-plugin/), [In-app update APIs](https://developers.meta.com/vr/documentation/unity/ps-in-app-update-apis/) (Application API), [Customer reviews](https://developers.meta.com/vr/resources/user-reviews/); no current page found for the Consent API (unverified) | link check 2026-10-17 |
| `hz-unity-code-review` | Turn on multiview under XR Plug-in Management > **Oculus** | The Oculus XR plugin is deprecated. Use **OpenXR** (Meta XR feature group) | [research-meta.md](../../docs/research-meta.md) §5 |
| `hz-unity-meta-core-sdk` (references) | Unity 2022.3.15f1 or later | Meta XR SDK v203+ needs **Unity 6000.0.66f2 or later** | [research-meta.md](../../docs/research-meta.md) §5 |
| `hz-unity-code-review` (SKILL.md §2 table; references/performance-checklist.md Draw Calls, Triangle Count, Memory Budget) | Meta VR Glasses column copies Quest 3 (200–300 busy draw calls, 1M–2M triangles), while the checklist also says "up to 150" draw calls and "< 1M" triangles; ~2 GB total RAM | No VR Glasses draw-call or triangle budget is published. An app that holds its frame rate at nominal CPU 4 / GPU 4 on Quest 3 reaches it on VR Glasses; no GPU level 5, dual-core or CPU/GPU trading there. 12 GB RAM; PSS kill limit 5.75 GiB, as on Quest 3/3S | [optimize-performance](https://developers.meta.com/vr/documentation/unity/optimize-performance/) (2026-09-19), [memory-ram](https://developers.meta.com/vr/essentials/memory-ram/) (2026-08-31), [compare-devices](https://developers.meta.com/vr/essentials/compare-devices/) (2026-09-30) |
| `hz-unity-device-readiness` (SKILL.md Step 2, "UI Interaction" row) | Convert ray-based UI to poke (`PokeInteractable`) | On VR Glasses eye gaze targets and pinch selects. Unity route: ISDK 207 gaze (right-click Canvas > Interaction SDK > Add Gaze Interaction to Canvas, "Enable Ray Fallback" on); poke only for UI within reach | [unity-support-meta-vr-glasses](https://developers.meta.com/vr/documentation/unity/unity-support-meta-vr-glasses/) (2026-09-30), [unity-isdk-getting-started](https://developers.meta.com/vr/documentation/unity/unity-isdk-getting-started/) (2026-09-14) |
| `hz-unity-device-readiness` (FoV section) | Skip the FoV pass without both frusta | Meta publishes nominal 70° × 66° (perceived p90 58° × 54°); the XR Simulator VR Glasses profile uses 37° temporal / 37° nasal / 34° up / 34° down per eye. The per-side split of the visible 70 × 66 isn't published (Core 207.0.1's FoV mask defaults to 35/35, 25 up / 41 down, from code) | [field-of-view](https://developers.meta.com/vr/essentials/field-of-view/) (2026-09-25), [xrsim-getting-started](https://developers.meta.com/vr/documentation/unity/xrsim-getting-started/) (2026-09-03) |
| `hz-new-project-creation` (references/unity-project-manual.md "Target Devices") | Select Meta VR Glasses among the target devices | Unity OpenXR 1.18.0 / 1.19.0-pre.2's Meta Quest feature has no VR Glasses entry; Core 207.0.1 `OVRProjectConfig` has `DeviceType.VRGlasses`, but its manifest patch writes the internal key `stanley`. The compatibility-mode page says VR Glasses aren't a supported public `supportedDevices` value: ship through Dashboard **Future devices** | [compatibility-mode](https://developers.meta.com/vr/essentials/compatibility-mode/) (2026-09-04); package source `com.meta.xr.sdk.core` 207.0.1 `Editor/OVRManifestPreprocessor.cs`, `com.unity.xr.openxr` `MetaQuestFeature.cs` (read 2026-10-17 by the Unity pass) |
| `hz-immersive-designer` | Ray targets ≥ 48 mm (~2.8° at 1 m), poke/pinch 64 mm; no gaze rule; "Is haptic feedback used to confirm interactions?" | Gaze targets ≥ **3° of visual angle** (2 × d × tan 1.5°, ~5.2 cm at 1 m); keep hover subtle and never move or resize the target; give explicit close affordances. VR Glasses ship without controllers, so there are no haptics by default | [eyes-best-practices](https://developers.meta.com/vr/design/eyes-best-practices/) (2026-10-13), [compare-devices](https://developers.meta.com/vr/essentials/compare-devices/) (2026-09-30) |

Rows marked ✔ were checked again against the live page during review. The others come from the
research passes (`docs/research-meta.md`, `docs/research-connect-2026-platform.md`,
`docs/meta-reference/unity-vr-glasses.md`); the VR Glasses rows for `hz-unity-code-review`,
`hz-unity-device-readiness`, `hz-new-project-creation` and `hz-immersive-designer` come from the
2026-10-17 Unity pass, with their page dates spot-checked.

## Meta's pages vs. Meta's repo (2026-10-17)

- "Test your app for Meta VR Glasses" (2026-09-25) tells agents to use skills named `vrc-check`,
  `xr-simulator-setup` and `perfetto-debug`, and the VR Glasses start page (2026-10-13) names
  `new-project-creation`. The repo at `3a8553d` has `hz-perfetto-debug`,
  `hz-xr-simulator-install-and-configure` and `hz-new-project-creation`, and no VRC skill (it was
  removed on 2026-05-12). Use `../custom/horizon-vrc-checklist` for VRCs.
- `com.oculus.supportedDevices`: the release-manifest page (2026-10-15) lists `vrglasses`; the
  compatibility-mode page (2026-09-04) says VR Glasses are "not currently a supported public value"
  and to select **Future devices** instead. `hz-api-upgrade`'s example
  (`quest2|questpro|quest3|quest3s`) is therefore not clearly wrong. Rely on Future devices.

## Observed in a real project

These come from running the skills' advice in a Unity project, not from the docs: Unity 6000.6.6f1
on macOS (Apple Silicon), Meta XR Core SDK 207.0.1, Unity OpenXR Plugin 1.18.0, `metavr`
1.8.0.17.10, 2026-10-16.

| Skill | What the skill says | What happened | Evidence |
|---|---|---|---|
| `hz-xr-simulator-install-and-configure` (references/installation.md) | `com.meta.xr.simulator` is "required for simulator support"; install it if the simulator doesn't show up | With Core SDK **207**, adding `com.meta.xr.simulator` (latest on Meta's npm registry: **81.0.1**) **breaks compilation**: `error CS0122: 'Styles' / 'ToolDescriptor' / 'IUserInterfaceItem' is inaccessible due to its protection level` in the package's `Editor/Utils/ToolbarItem.cs` and `Editor/Updater.cs`. Core 207 ships the integration itself (`com.meta.xr.sdk.core/Editor/MetaXRSimulator/`, menu **Meta > Meta XR Simulator**). With Core ≥ 207, **don't add the separate package**; remove it if the project fails to compile | Editor.log of the batch import; removing the package fixed the build |
| `hz-xr-simulator-install-and-configure` | macOS: "extracting the archive" | `metavr tools install xrsim` (207.1, 447 MB) installs **`/Applications/MetaXRSimulator.app`**, which is exactly where Core 207's `MacOSXRSimInstallationDetector` looks first (then Spotlight). No `sudo`, no manual extraction | `metavr tools list`; Core source `Editor/MetaXRSimulator/MacOSXRSimInstallationDetector.cs` |
| `hz-xr-simulator-install-and-configure` | (Homebrew is an option Meta documents in `Oculus-VR/homebrew-repo`) | The Homebrew formula `oculus-vr/tap/meta-xr-simulator` was at **71.0.0** (vs 207.1 from `metavr`), needs `brew trust` for the third-party tap, and a `sudo …/post_installation_macos.sh` step. Prefer `metavr tools install xrsim` | `brew info meta-xr-simulator` |
| `metavr-cli` (SKILL.md "MCP" section) | Register the server with `metavr mcp install project` (or a per-client `metavr mcp install …`) and the agent gets metavr's tools | However it's registered, Claude Code **2.1.301** reports `Connected · tools fetch failed: Invalid result for tools/list … ttlMs … cacheScope`. metavr advertises MCP protocol `2026-07-28`, which Claude Code uses, but omits the `ttlMs`/`cacheScope` fields that version requires on list results (its `server/discover` reply has them). Also, `metavr mcp install claude-code` only **prints** the `claude mcp add` command unless you add `--execute`. Workaround: [`metavr_mcp_relay.py`](../custom/unity-quest-dev-setup/scripts/metavr_mcp_relay.py) (see `unity-quest-dev-setup` Step 8). npm `metavr` 1.8.1 (2026-10-01) still ships binary 1.8.0.17.10, so expect the same result (not re-tested) | Captured JSON-RPC traffic; see `mcp/README.md` |

**What to do in practice:** when you install `hz-store-submit`, also install
`custom/horizon-store-listing-assets`, `custom/horizon-vrc-checklist` and
`custom/horizon-store-publish-unity`. Or patch the values above into your installed copy. When
Meta updates the repo, check whether these rows are fixed before re-applying anything.
