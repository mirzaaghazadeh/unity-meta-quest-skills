# Source: scenario-unity-mobile

- **Repo:** https://github.com/scenario-labs/skills
- **Path:** `skills/game-engines/unity/scenario-unity-mobile/`
- **Commit:** `49dd26e47371163ae0020c7f5627d0993626bfc2` (2026-10-15)
- **License:** MIT, Copyright 2026 Scenario Inc. (`LICENSE` copied)
- **Stars at fetch:** 1032
- **Fetched:** 2026-10-16

## Why it was picked

Quest is an Android, Vulkan, tile-based mobile GPU platform. This is the strongest Unity 6.3 mobile skill found. It covers:
- sustained frame budgets (×0.65 for thermals) judged on p95 and max frame time;
- ASTC import rules enforced by an `AssetPostprocessor`;
- mobile URP settings (Depth Priming off on TBDR, Native RenderPass, Depth and Opaque textures off unless sampled);
- one URP asset per quality tier;
- IL2CPP ARM64 and Vulkan-first builds;
- reproducible signing from environment variables, with keystore passwords never on disk;
- AAB checks with bundletool;
- ANR and startup hygiene.

Each step comes with C# AgentKit jobs and a `ut_mobile.py` toolkit.

## Security review

- I read the SKILL.md. I grepped all references, the 17 C# files and `ut_mobile.py`.
- **subprocess targets:** Unity, `java -jar bundletool`, and Unity's bundled JDK `keytool` and `jarsigner`. `keytool -list` passes the store password as a command-line argument, so it is briefly visible in `ps`. That is minor, but worth knowing on shared machines.
- **Keystore:** passwords come only from environment variables (`MobileBuild.cs`), and the procedures generate them in memory.
- **No network calls.** The code-scan regex that mentions `UnityWebRequest` only lints user code.

## Caveats

- **Phone-oriented.** It covers Google Play AAB, iOS Xcode export, IAP 5, LevelPlay ads, touch HUD and safe areas, none of which apply to Quest. **Quest builds are APKs uploaded with `ovr-platform-util`** (see `dilmerv__submit-game-version` and the official Meta skills), not Play AABs. Quest also needs OpenXR and Meta settings this skill does not cover. Use it for the texture, URP, thermal, IL2CPP and signing parts.
- It needs `scenario-labs__scenario-unity-expert` installed as a sibling folder named `scenario-unity-expert`.
- It was verified in the editor on macOS. Device steps (soak, thermal) are listed as hand-overs, not run.
