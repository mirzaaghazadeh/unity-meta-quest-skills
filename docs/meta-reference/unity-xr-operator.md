# Meta XR Operator, Device Readiness Check and the AI Tools panel (Unity, condensed)

Checked 2026-10-17 against developers.meta.com and the Meta XR Core SDK 207.0.1 package source.
Our own summary. **Code** = read from the package (`com.meta.xr.sdk.core@207.0.1`), not from a
docs page. All three tools are labelled **experimental** by Meta: names, tools and flows can change.

## 1. Meta XR Operator

| Fact | Value | Src |
|---|---|---|
| What | An OpenXR API layer inside your running app that starts a local MCP server, so an agent can read session/tracking/frame state and drive input | OV |
| Ships in | Meta XR Core SDK ≥ v207 (optional, experimental); also a standalone bundle (207.0.1, 2026-10-13) for non-Unity OpenXR apps | OV, STD |
| Needs | Unity 6000.0.x+, Core SDK v207+, Unity OpenXR Plugin (`com.unity.xr.openxr`) **1.17.0+**, an MCP-capable agent | OV, GS |
| Runs with | Windows: Quest Link or XR Simulator. macOS (Apple Silicon): XR Simulator. Quest: on-device build | OV |
| Endpoint | In-app server on port **8720**: SSE at `http://localhost:8720/sse` (docs); streamable HTTP at `http://127.0.0.1:8720/mcp` (Meta's bundled skill). Port override env var `AGENTICXR_MCP_PORT` (Meta skill `hz-xr-simulator-control`) | OV, CN, Code |
| Recommended connection | Through the MCP proxy (keeps a tool list while the app is stopped, reconnects). Direct SSE is "advanced" | OV, CN |
| Tools live only while | The app is in Play mode or running and focused on a headset | GS, QU |
| Capabilities | Session/system/frame state; Unity scene graph and UI canvases; head and controller poses (smooth moves); controller buttons/axes; synthetic hands (pinch, poke, grab) and gaze-and-pinch; composited image capture; spatial entities (rooms, walls, furniture); spatial math; your own C# tools | OV |
| Custom tools | `MetaXROperatorExternalTool.RegisterAgenticTool(name, description, parameters, callback)` (namespace `Meta.XR`) from `Start()`, once per session, no unregister; duplicate names return `ErrorValidationFailure`; callbacks run on the main thread; prefix names with your app name | CT |

### Unity setup (docs flow)

1. Core SDK v207+, OpenXR Plugin ≥ 1.17.0, OpenXR enabled for desktop and Android (GS).
2. Meta XR SDK window (Window > Meta > Meta XR SDK) > **AI Tools**: pick the agent, **Install
   Meta VR CLI** / **Install XR Operator**, **Run Command** (registers the MCP server with the
   agent), **Activate XR Operator**, optionally **Install project-level skills** (GS).
3. Project Setup Tool: apply the **Meta XR Operator** rules (upgrades OpenXR Plugin, enables the
   **API Layers** feature, registers the layer). On macOS the layer is enabled through environment
   variables at Play time instead (GS).
4. Launch the agent from the project root, enter Play mode with XR Simulator, ask for the XR
   session state (GS).

Manual equivalents (GS): `metavr tools install xroperator` (or `update`), then
`metavr mcp install claude-code`. Note: for Claude Code that command only prints the
`claude mcp add` line unless `--execute` is added (seen in metavr 1.8.0; the Core SDK's panel
passes `--execute -y`, Code).

### What the AI Tools panel does for Claude Code (Code)

| Panel transport | What it registers | Server name |
|---|---|---|
| MCP Proxy (default) | `metavr mcp install claude-code --execute -y`: Operator tools are **federated into metavr's MCP server** | `metavr` |
| Direct SSE | `claude mcp add meta-xr-operator -t sse http://localhost:8720/sse` | `meta-xr-operator` |
| (standalone docs) | stdio server running `meta-xr-operator-mcp-proxy` (bundled in Core at `Editor/MetaXROperator/Tools~/Mac/` and `Tools~/Windows/`) | `meta-xr-operator` |

- metavr registers Claude Code at **local scope** (`~/.claude.json`, project entry) (Code).
- Skills install to `<project>/.claude/skills` for Claude Code (Code).
- The federated route inherits any metavr MCP problem. This repo saw metavr 1.8.0.17.10 tools fail
  to load in Claude Code 2.1.301 (`ttlMs`/`cacheScope`); see `mcp/README.md`. npm metavr 1.8.1
  (2026-10-01) only changed the wrapper and still ships binary 1.8.0.17.10 ([ai-tools.md](ai-tools.md)),
  so expect the same until re-tested.
- Using both the federated `metavr` entry and a separate `meta-xr-operator` entry exposes the
  Operator tools twice. Register one.

### On a Quest headset (QU)

- Build profile **Android (Meta Quest)**, Project Setup Tool fixes for Android, **Development
  Build on**. The layer is bundled into the APK automatically.
- Code (MetaXROperatorBuildProcessor): the Operator AAR is included only in Development builds and
  the API layer is temporarily disabled for release builds.
- AI Tools > **Set up headset** runs, in order: `adb forward tcp:8720 tcp:8720`;
  `adb shell setprop debug.oculus.experimentalEnabled 1` (full tool set on Horizon OS v205+);
  `adb shell setprop debug.meta_xr_operator.request_capture_permission 1`;
  `adb shell am broadcast -a com.oculus.vrpowermanager.prox_close` (stay awake off-face).
  **Undo setup** reverses them; nothing survives a reboot.
- Manual path also needs `android.permission.INTERNET` and
  `<uses-feature android:name="com.oculus.experimental.enabled" android:required="false"/>`.
- Host port 8720 binds once: an Editor/Simulator session blocks `adb forward tcp:8720 tcp:8720`;
  use e.g. `adb forward tcp:8721 tcp:8720`.
- `openxr_set_head_pose` is unavailable on a headset (head follows the device). Screen capture
  needs one-time in-headset consent. Restore the proximity sensor with
  `adb shell am broadcast -a com.oculus.vrpowermanager.automation_disable`.

### Meta's bundled Operator skills (in Core 207.0.1 `Editor/MetaXROperator/Skills/`, Oculus SDK License, not copied here)

`hz-meta-xr-operator` (core usage), `-coordinates`, `-grabbed-objects`, `-hand-tracking`,
`-interaction-grab`, `-interaction-poke`, `-unity-cli-workflow` (edit with the `unity` CLI,
verify with Operator), `-unity-meta-quest` (device gotchas), `-unity-test-mechanics` (bounded
3-attempt test loop). Gotchas they record (Code): enable a controller interaction profile on the
**Android** OpenXR tab or simulated clicks do nothing; OVRRaycaster rays sit about 50–55° below the
aim pose; simulated controllers suppress physical ones until `openxr_release_input_devices`; hold
inputs across frames.

## 2. Meta XR AI Tools / Agent Bridge (Device Readiness AI mode, Immersive Debugger assistant)

| Fact | Value | Src |
|---|---|---|
| Providers | Claude Code, OpenAI Codex, Gemini CLI, OpenCode (Muse Code via metavr) | DRC |
| Settings | Edit > Preferences > Meta XR > Agent Bridge: **Enable AI Agent Bridge**, Additional Arguments, Advanced > Verbose Logging | DRC |
| How Claude Code is launched | `claude -p --output-format stream-json --include-partial-messages --verbose --dangerously-skip-permissions` plus your Additional Arguments | Code (`Editor/AgentBridge/ClaudeCodeService.cs`) |

**Implication:** the AI-powered check and the in-Editor assistant run Claude Code in the project
with permission prompts **bypassed**. Prefer the Standard check, or copy the prompt from **Run it
yourself in any AI agent** into a normal Claude Code session where permissions apply.

## 3. Device Readiness Check (DRC)

- Requires Core v207+ (ISDK v205+ if you use ISDK hands). Window > Meta > Tools > Device Readiness
  Check. Scan is read-only; fixes apply only when you click them. **Export as Markdown** saves the
  report; the report survives script reloads, not closing the window.
- Badges: **Automated** (one-click project fix), **Manual**, **AI assisted**.
- Standard check (Code, `Editor/HandReadinessTool/`): two setup tasks (Hand Tracking Support not
  Controllers Only → fix sets Controllers And Hands; ISDK ≥ 205.0.0 recommended), hidden from the
  normal Project Setup Tool; plus a regex scan of `Assets/**/*.cs` for: OVRInput trigger / grip /
  thumbstick-click / B / Y button reads; `OVRInput.GetLocalController*` pose reads; thumbstick
  axes; `OVRGrabber`/`OVRGrabbable`. A/X reads are not flagged (pinch is auto-mapped). Rules can be
  refreshed from a Meta remote-content endpoint at Editor start; baked-in rules are the fallback.
- Its predecessor page, **Hands Optimizer** (Core v205+, 2026-09-09), describes the same flow.
- Meta's agent skill for the same audit, plus a head-locked-UI FoV pass: `hz-unity-device-readiness`
  (in this repo under `skills/meta-official/`).

## Source keys

Canonical `/vr/` paths; `/horizon/` paths 302-redirect to them (checked 2026-10-17).

| Key | URL (developers.meta.com) | Last updated |
|---|---|---|
| OV | /vr/documentation/unity/meta-xr-operator/ | 2026-09-09 |
| GS | /vr/documentation/unity/meta-xr-operator/getting-started/ | 2026-09-09 |
| QU | /vr/documentation/unity/meta-xr-operator/quest/ | 2026-09-10 |
| SI | /vr/documentation/unity/meta-xr-operator/xr-simulator/ | 2026-09-09 |
| CN | /vr/documentation/unity/meta-xr-operator/connecting-ai-agents/ | 2026-09-09 |
| CT | /vr/documentation/unity/meta-xr-operator/custom-tools/ | 2026-07-23 |
| STD | /vr/downloads/package/meta-xr-operator-standalone/ (207.0.1) | 2026-10-13 |
| DRC | /vr/documentation/unity/device-readiness/ | 2026-09-24 |
| HO | /vr/documentation/unity/hands-optimizer/ | 2026-09-09 |
| Code | `com.meta.xr.sdk.core` 207.0.1: `Editor/MetaXROperator/*`, `Editor/AgentBridge/ClaudeCodeService.cs`, `Editor/HandReadinessTool/*` | read 2026-10-17 |
