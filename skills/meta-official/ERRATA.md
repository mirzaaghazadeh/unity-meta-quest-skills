# Errata: where Meta's official skills disagree with Meta's live docs

The skill folders here are kept **unchanged** from upstream (commit `3a8553d`). Some values in them
are older than Meta's own developer pages. Checked on 2026-10-16. Where the two disagree, trust the
live page and the custom skills in `../custom/`, which were written from those pages.

| Skill | What the skill says | What Meta's docs say now | Source |
|---|---|---|---|
| `hz-store-submit` | `targetSdkVersion >= 32`, `minSdkVersion >= 29` | targetSdk **32–34** for immersive apps, and **apps created since 2026-03-01 must target 34**. minSdk 29–34, with 32 recommended | [publish-mobile-manifest](https://developers.meta.com/horizon/resources/publish-mobile-manifest/) ✔ re-checked |
| `hz-store-submit` | Icon 1024×1024; hero 2560×1440; at least 3 screenshots; small landscape 1280×720 | Icon **512×512** (24-bit PNG, no transparency); hero **3000×900**; covers 2560×1440 / 1440×1440 / 1008×1440; **5** screenshots at 2560×1440 | [asset-guidelines](https://developers.meta.com/horizon/resources/asset-guidelines/) ✔ re-checked |
| `hz-store-submit` | Name 30, short description 150, long description 4000 characters | **40 / 500 / 1500** | [publish-app-metadata](https://developers.meta.com/horizon/resources/publish-app-metadata/) |
| `hz-store-submit` | Trailer 30–60 s | **30 s to 2 min**, MP4/H.264/AAC, 1080p–2K | [asset-guidelines](https://developers.meta.com/horizon/resources/asset-guidelines/) ✔ re-checked |
| `hz-store-submit` | 72 Hz minimum (90 Hz Quest 3); load under 15 s | VRC.Quest.Performance.1: render at **≥ 60 fps**. Performance.3: graphics within **4 s** or a loading indicator | [publish-quest-req](https://developers.meta.com/horizon/resources/publish-quest-req/) |
| `hz-store-submit`, `hz-unity-platform-sdk` | Paid apps must check entitlement and quit, or they are auto-rejected | VRC.Quest.Security.1 is now **Recommended**. On failure, exiting, showing an error or a limited demo are all allowed | [vrc-quest-security-1](https://developers.meta.com/horizon/resources/vrc-quest-security-1/) |
| `hz-unity-code-review` | Turn on multiview under XR Plug-in Management > **Oculus** | The Oculus XR plugin is deprecated. Use **OpenXR** (Meta XR feature group) | [research-meta.md](../../docs/research-meta.md) §5 |
| `hz-unity-meta-core-sdk` (references) | Unity 2022.3.15f1 or later | Meta XR SDK v203+ needs **Unity 6000.0.66f2 or later** | [research-meta.md](../../docs/research-meta.md) §5 |

Rows marked ✔ were checked again against the live page during review. The others come from the
research pass (`docs/research-meta.md`, sources listed there).

## Observed in a real project

These come from running the skills' advice in a Unity project, not from the docs: Unity 6000.6.6f1
on macOS (Apple Silicon), Meta XR Core SDK 207.0.1, Unity OpenXR Plugin 1.18.0, `metavr`
1.8.0.17.10, 2026-10-16.

| Skill | What the skill says | What happened | Evidence |
|---|---|---|---|
| `hz-xr-simulator-install-and-configure` (references/installation.md) | `com.meta.xr.simulator` is "required for simulator support"; install it if the simulator doesn't show up | With Core SDK **207**, adding `com.meta.xr.simulator` (latest on Meta's npm registry: **81.0.1**) **breaks compilation**: `error CS0122: 'Styles' / 'ToolDescriptor' / 'IUserInterfaceItem' is inaccessible due to its protection level` in the package's `Editor/Utils/ToolbarItem.cs` and `Editor/Updater.cs`. Core 207 ships the integration itself (`com.meta.xr.sdk.core/Editor/MetaXRSimulator/`, menu **Meta > Meta XR Simulator**). With Core ≥ 207, **don't add the separate package**; remove it if the project fails to compile | Editor.log of the batch import; removing the package fixed the build |
| `hz-xr-simulator-install-and-configure` | macOS: "extracting the archive" | `metavr tools install xrsim` (207.1, 447 MB) installs **`/Applications/MetaXRSimulator.app`**, which is exactly where Core 207's `MacOSXRSimInstallationDetector` looks first (then Spotlight). No `sudo`, no manual extraction | `metavr tools list`; Core source `Editor/MetaXRSimulator/MacOSXRSimInstallationDetector.cs` |
| `hz-xr-simulator-install-and-configure` | (Homebrew is an option Meta documents in `Oculus-VR/homebrew-repo`) | The Homebrew formula `oculus-vr/tap/meta-xr-simulator` was at **71.0.0** (vs 207.1 from `metavr`), needs `brew trust` for the third-party tap, and a `sudo …/post_installation_macos.sh` step. Prefer `metavr tools install xrsim` | `brew info meta-xr-simulator` |
| `metavr-cli` (SKILL.md "MCP" section) | Register the server with `metavr mcp install project` (or a per-client `metavr mcp install …`) and the agent gets metavr's tools | However it's registered, Claude Code **2.1.301** reports `Connected · tools fetch failed: Invalid result for tools/list … ttlMs … cacheScope`. metavr advertises MCP protocol `2026-07-28`, which Claude Code uses, but omits the `ttlMs`/`cacheScope` fields that version requires on list results (its `server/discover` reply has them). Also, `metavr mcp install claude-code` only **prints** the `claude mcp add` command unless you add `--execute`. Workaround: [`metavr_mcp_relay.py`](../custom/unity-quest-dev-setup/scripts/metavr_mcp_relay.py) (see `unity-quest-dev-setup` Step 8) | Captured JSON-RPC traffic; see `mcp/README.md` |

**What to do in practice:** when you install `hz-store-submit`, also install
`custom/horizon-store-listing-assets`, `custom/horizon-vrc-checklist` and
`custom/horizon-store-publish-unity`. Or patch the values above into your installed copy. When
Meta updates the repo, check whether these rows are fixed before re-applying anything.
