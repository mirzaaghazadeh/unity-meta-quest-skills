# Source: submit-game-version

- **Repo:** https://github.com/dilmerv/ArcadeHoopsMR_Build_Tools
- **Path:** repo root (`SKILL.md`, `CI/`, `Assets/CI/Editor/ReleaseBuilder.cs`)
- **Commit:** `91bd92e095af530e30e73281f66e59f7cf57f754` (2026-08-30)
- **License:** MIT (`LICENSE` copied)
- **Stars at fetch:** 2. The author, Dilmer Valecillos, is a well-known XR educator (LearnXR).
- **Fetched:** 2026-10-16

## What is in this folder

Copied unchanged:
- `SKILL.md`
- `CI/upload_and_record.sh`
- `CI/release.local.env.example`
- `CI/release_notes.md` (empty upstream)
- `Assets/CI/Editor/ReleaseBuilder.cs`
- `LICENSE`

**Deliberately not copied:**
- `CI/release.local.env`, that project's local signing and store configuration. Use `CI/release.local.env.example` as the template and keep your own copy out of git.
- `CI/LastSubmittedVersion.md`, a record of that project's last submission.
- `.DS_Store` files.

## Why it was picked

It was the only community skill found that automates the Meta Horizon Store upload step from Claude Code. It shows a complete pattern:
1. Drive the open Unity editor over the Unity AI Assistant MCP (`Unity_RunCommand`).
2. Bump `bundleVersion` and `bundleVersionCode`.
3. Build a signed release APK without exiting the editor, reading keystore passwords from a gitignored env file.
4. Upload with Meta's `ovr-platform-util upload-quest-build` to the BETA channel with release notes.
5. Write a submission record and leave the version bump uncommitted for review.

## Security review

- I read the SKILL.md, the shell script, and the C# builder in full.
- The script sources a local env file, calls `ovr-platform-util` from Meta Quest Developer Hub or from PATH, and writes `CI/LastSubmittedVersion.md`. It makes no other network calls.
- The C# builder bumps the version, sets signing from the env file, switches to Android, builds with `BuildPipeline.BuildPlayer`, and writes `Builds/last_release_result.txt`.

## Caveats

- **Project-specific.** Names (`ArcadeHoops.CI.ReleaseBuilder`, `ArcadeHoopsMR-*.apk`, `beta-1.0.x` versioning) and the keystore path in the `.example` file are that project's. Treat this as a template: adapt the namespace, version scheme, channel and paths before use.
- It depends on the Unity AI Assistant MCP tool names (`Unity_RunCommand`, `Unity_ManageEditor`). As of Unity 6.3 that MCP server is marked deprecated in favour of the official Unity CLI (see `scenario-unity-expert`, `references/live-channels.md`). With the CLI, call `ReleaseBuilder.BuildAndroidReleaseInEditor` through `unity command eval` or a batch `-executeMethod` instead.
- It uploads to a **public store channel**. Run it only after explicit confirmation.
