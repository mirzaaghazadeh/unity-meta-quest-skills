# Source: Meta VR Agentic Tools (official)

| Field | Value |
|---|---|
| Publisher | Meta Platforms, Inc. |
| Repository | https://github.com/meta-quest/agentic-tools |
| Commit | `3a8553d10a5a1bd1bf9beaaa950243fbbed3b9ea` (2026-09-23) |
| License | Apache-2.0 (copy in `LICENSE`) |
| Fetched | 2026-10-16 |
| Meta's page | https://developers.meta.com/horizon/discover/ai-tools/ |

The skill folders here are copied unchanged from `skills/` in the repo at the commit above.

## What was copied (26 of 36)

These are the skills that apply to a **Unity → Quest → Horizon Store** pipeline. All of them use the
`metavr` CLI (Meta VR CLI, formerly `hzdb`), and the Unity ones expect a live Editor driven
through the `unity-cli` skill (see `../unity-official/` or `../custom/`).

## What was left out (10), and why

| Skill | Why skipped |
|---|---|
| `hz-android-2d-porting` | Native Android 2D apps, not Unity |
| `hz-iwsdk-webxr` | WebXR / Immersive Web SDK |
| `hz-store-pwa` | Shipping a PWA/TWA web app to the store |
| `hz-spatial-sdk` | Kotlin Meta Spatial SDK |
| `hz-platform-sdk`, `hz-psdk-integration` | Platform SDK for **Kotlin/Android**. The Unity equivalent `hz-unity-platform-sdk` is included |
| `hz-metavrx-layout-sdk`, `hz-metavrx-ui-set` | Jetpack Compose / React Native multi-window apps |
| `hz-react-native-expo` | React Native |
| `portal` | Meta Portal devices |

To get them, browse the repo above. Only `hz-unity-platform-sdk/SKILL.md` mentions `hz-platform-sdk`, as a pointer
for Kotlin projects, so nothing included depends on a skipped skill.

## Updating

```bash
git clone --depth 1 https://github.com/meta-quest/agentic-tools.git /tmp/agentic-tools
diff -rq /tmp/agentic-tools/skills ./   # review what changed before copying over
```

Once installed, `metavr skills` can install and update the bundle for you.
