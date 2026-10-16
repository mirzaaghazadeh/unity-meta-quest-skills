---
name: submit-game-version
description: Build the ArcadeHoopsMR Unity project in release mode and submit it to the Meta Horizon Store. Use when the user says "Build & submit new game version", "submit a new game version", "build and submit", "release a new version", or similar release requests.
---

# Submit Game Version

Bumps the version, builds a signed release Quest APK, and uploads it to the Meta
Horizon Store **BETA** channel — **with the Unity Editor left open the whole time**.

The build runs *inside the running editor* over Unity MCP, so you never close Unity.
`ReleaseBuilder.BuildAndroidReleaseInEditor()` does the full signed build but never
calls `EditorApplication.Exit`, so the editor survives. It reads the keystore passwords
from `CI/release.local.env` itself (the open editor has no env vars set). The upload +
record step is `CI/upload_and_record.sh`, which writes `CI/LastSubmittedVersion.md`.

> Requires Unity to be **open** with this project. (There is intentionally no batchmode
> path — batchmode launches a second Unity and fails while the editor holds the project.)

## Steps

1. **Confirm the editor is open and idle.** `Unity_ManageEditor(GetState)` — if it
   responds, Unity is open. Bail/ask if `IsPlaying` or `IsCompiling` is true (don't build
   during Play mode or a compile). If Unity isn't open, ask the user to open the project.

2. **Save the open scene(s)** so the build captures the latest edits, via
   `Unity_RunCommand`:
   ```csharp
   using UnityEditor.SceneManagement;
   internal class CommandScript : IRunCommand
   {
       public void Execute(ExecutionResult result)
       {
           bool saved = EditorSceneManager.SaveOpenScenes();
           result.Log("SaveOpenScenes = " + saved);
       }
   }
   ```

3. **Trigger the build via `Unity_RunCommand`:**
   ```csharp
   using UnityEngine;
   internal class CommandScript : IRunCommand
   {
       public void Execute(ExecutionResult result)
       {
           ArcadeHoops.CI.ReleaseBuilder.BuildAndroidReleaseInEditor();
           result.Log("Invoked BuildAndroidReleaseInEditor");
       }
   }
   ```
   The build takes **several minutes** and blocks the editor's main thread. Expect one of:
   - The RunCommand returns with a `RELEASE_BUILD_RESULT version=... code=... apk=...`
     log line → success. (The MCP wrapper may report `UNEXPECTED_ERROR` because the build
     emits benign **warnings** — XR deprecation notices, icon-compression hints. Those are
     not failures; the presence of the `RELEASE_BUILD_RESULT` line means the build
     succeeded. A real failure logs `RELEASE_BUILD_FAILED ...` instead.)
   - The MCP call times out before the build finishes. That's fine — the build keeps
     running on the main thread and completes regardless; verify via the result file next.

4. **Verify the build landed** by reading `Builds/last_release_result.txt` (written only
   on success) and confirming the APK exists:
   ```bash
   cat Builds/last_release_result.txt
   ls -la "$(sed -n 's/^apk=//p' Builds/last_release_result.txt)"
   ```
   If the result file isn't there yet, the build may still be running — wait and re-check
   before assuming failure.

5. **Set the release notes** (shown to users on the build). Write a concise summary —
   **max 6–8 sentences** — of ONLY what is new/changed since the last submitted build into
   `CI/release_notes.md` (overwrite it). Track this as you make changes across the session;
   if nothing is new, leave it empty. `upload_and_record.sh` reads this file for the
   `--notes` flag, records it in `CI/LastSubmittedVersion.md`, and then **clears it** so the
   next build's notes only cover the next batch of changes. (A generic
   `ArcadeHoopsMR <ver> (code N)` is used only if the file is empty; you can also pass
   `--notes "..."` to override.)

6. **Upload + record:**
   ```bash
   CI/upload_and_record.sh
   ```
   With no args it reads `Builds/last_release_result.txt` for the version/code/APK and
   `CI/release_notes.md` for the notes, uploads to the BETA channel, writes
   `CI/LastSubmittedVersion.md`, and clears `release_notes.md`. Run it in the background
   (uploads take a minute or two) and relay its final summary (incl. the Meta Build ID).

## What a successful run does

1. Bumps the patch in `bundleVersion` (`beta-1.0.3` → `beta-1.0.4`) and increments
   `AndroidBundleVersionCode` (+1) in `ProjectSettings.asset`.
2. Builds a signed, non-development release APK into `Builds/` and writes
   `Builds/last_release_result.txt`.
3. Uploads it to the Meta Horizon Store BETA channel with `ovr-platform-util`, passing
   `CI/release_notes.md` as the user-facing `--notes` (or a generic fallback if empty).
4. Writes `CI/LastSubmittedVersion.md` (version, bundle code, channel, date, release notes,
   last 5 commits) and clears `CI/release_notes.md` for the next build.

## Preconditions

- Unity must be **open** with the project and **idle** (not in Play mode / not compiling).
- `CI/release.local.env` must exist (gitignored) with `KEYSTORE_PASS`, `KEYALIAS_PASS`,
  `META_APP_ID`, `META_APP_SECRET`. If it's missing, `upload_and_record.sh` prints setup
  instructions and `BuildAndroidReleaseInEditor` logs a `RELEASE_BUILD_FAILED` about the
  missing passwords — surface them and point the user at `CI/release.local.env.example`.

## On failure

- Build failure → look for the `RELEASE_BUILD_FAILED ...` line in the RunCommand logs /
  Unity console (`Unity.GetConsoleLogs`). Common causes: keystore password mismatch, no
  enabled scenes, or a compile/build error.
- Upload errors → wrong `META_APP_ID` / `META_APP_SECRET`, or network.
- Fix simple issues and retry; ask the user for credentials you don't have.

## After a successful submit

- Report the submitted version + bundle code, the BETA channel, and the Meta Build ID
  from the upload output.
- The version bump (in `ProjectSettings.asset`) and `CI/LastSubmittedVersion.md` are left
  **uncommitted** for the user's review — do not commit unless they ask.
