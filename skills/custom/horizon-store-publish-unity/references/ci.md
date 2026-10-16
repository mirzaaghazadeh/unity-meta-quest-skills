# CI for Unity to Horizon Store uploads

Run release builds on a machine that has a licensed Unity Editor matching
`ProjectSettings/ProjectVersion.txt` with Android Build Support, OpenJDK and Android SDK & NDK.
Per Meta's Unity requirements page (2026-08-11), standalone Quest builds run on Windows 10+ 64-bit
or macOS ("limited features"). Meta XR SDK v207 needs Unity 6000.0.66f2 or later.

## Pipeline order

1. **Checkout**, LFS included if used. Restore the `Library/` cache keyed on the Unity version,
   `Packages/packages-lock.json` and `ProjectSettings/`.
2. **License**: follow the `unity-cli` skill (`unity auth login --client-id ... --secret-from-stdin`,
   `unity license activate`, `unity license return --yes` at the end).
3. **Version code**: `QUEST_VERSION_CODE=$((BASE + CI_RUN_NUMBER))`. `BASE` must sit above the
   highest code already uploaded. Read that once with
   `ovr-platform-util get-release-channel-data --app-id ... --token ...`.
4. **Keystore**: decode it from a secret (base64) to a temp file outside the workspace. Export
   `QUEST_KEYSTORE_PATH`, `QUEST_KEYSTORE_PASS`, `QUEST_KEY_ALIAS`, `QUEST_KEY_ALIAS_PASS`.
   Delete the file in an always-run cleanup step.
5. **Optional settings gate**: run the Project Setup Tool report in batch mode
   (`-executeMethod OVRProjectSetupCLI.GenerateProjectSetupReport -reportFile <path> -buildTarget Android`)
   and/or `QuestProjectSettingsAudit.Audit` from `quest-unity-project-settings`. Fail the job on
   Required issues.
6. **Build**: `unity build <proj> --target Android --execute-method QuestReleaseBuild.Build --output-path Builds/Quest/app.apk --timeout 3600`.
7. **Verify**: `scripts/verify-quest-apk.sh Builds/Quest/app.apk`. A non-zero exit fails the job.
8. **Upload**: `scripts/upload-quest-build.sh Builds/Quest/app.apk alpha`, with `META_APP_ID`,
   `META_UPLOAD_TOKEN` and `META_AGE_GROUP` from secrets and `RELEASE_NOTES` from the changelog.
   Never upload to `store` from CI without a manual approval gate.
9. **Archive**: the APK, the IL2CPP symbols (`*.sym.so`), the Unity CLI provenance manifest, and
   the uploaded Build ID.

## GitHub Actions skeleton (fill in for your runner)

Install the Unity CLI and the Editor per the `unity-cli` skill. Runner setup and action
versions are left to you on purpose. Pin whatever you choose.

```yaml
name: quest-alpha
on:
  workflow_dispatch:
  push:
    tags: ["v*"]
jobs:
  build-upload:
    runs-on: [self-hosted, unity]        # machine with Unity + Android modules
    environment: quest-release           # holds the secrets; add required reviewers for store uploads
    env:
      QUEST_VERSION_CODE: ${{ github.run_number }}   # add your BASE offset if needed
      META_APP_ID: ${{ secrets.META_APP_ID }}
      META_UPLOAD_TOKEN: ${{ secrets.META_UPLOAD_TOKEN }}
      META_AGE_GROUP: TEENS_AND_ADULTS
    steps:
      - uses: actions/checkout@v4
      - name: Keystore
        run: |
          echo "${{ secrets.QUEST_KEYSTORE_B64 }}" | base64 --decode > "$RUNNER_TEMP/release.keystore"
          echo "QUEST_KEYSTORE_PATH=$RUNNER_TEMP/release.keystore" >> "$GITHUB_ENV"
      - name: Build
        env:
          QUEST_KEYSTORE_PASS: ${{ secrets.QUEST_KEYSTORE_PASS }}
          QUEST_KEY_ALIAS: ${{ secrets.QUEST_KEY_ALIAS }}
          QUEST_KEY_ALIAS_PASS: ${{ secrets.QUEST_KEY_ALIAS_PASS }}
        run: unity build . --target Android --execute-method QuestReleaseBuild.Build --output-path Builds/Quest/app.apk --timeout 3600 --format github
      - name: Verify
        run: bash tools/quest/verify-quest-apk.sh Builds/Quest/app.apk
      - name: Upload to ALPHA
        env:
          RELEASE_NOTES: "CI build ${{ github.run_number }} (${{ github.sha }})"
        run: bash tools/quest/upload-quest-build.sh Builds/Quest/app.apk alpha
      - name: Cleanup keystore
        if: always()
        run: rm -f "$RUNNER_TEMP/release.keystore"
```

(`tools/quest/` is wherever you copy this skill's `scripts/` inside the repo.)

## Notes

- Keep the Unity Editor patch version fixed in CI. An Editor upgrade can change the Android
  Gradle/SDK toolchain and the resolved target SDK.
- If several branches upload, give each one its own version-code range, or let only one branch
  upload.
- Debug symbols: IL2CPP Android builds produce `libil2cpp.sym.so`. Upload them with the build
  (`--debug-symbols-dir`) so the Dashboard can symbolicate crash reports.
