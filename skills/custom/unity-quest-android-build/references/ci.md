# CI builds for Quest with the unity CLI

Part of **`unity-quest-android-build`**. Read the official `unity-cli` skill
(`references/diagnostics-maintenance.md` → "CI init") for the generator's full behaviour.

## Start from Unity's generator, not from scratch

```bash
unity ci init --target Android --dry-run            # print the workflow and review it
unity ci init --target Android                      # write .github/workflows/unity.yml
unity ci init --provider gitlab --target Android    # .gitlab-ci.yml instead
```

The generated workflow installs the CLI, restores the Editor download cache and `Library/`,
installs the Editor version from `ProjectSettings/ProjectVersion.txt` with the Android module, and
activates a license. It then runs `unity doctor --ci`, runs tests with a JUnit report, builds,
uploads artifacts, and **returns the license seat in a step that always runs**. It refers to
secrets by name only: `UNITY_SERVICE_ACCOUNT_ID`, `UNITY_SERVICE_ACCOUNT_SECRET`,
`UNITY_LICENSE_SERIAL`.

Two edits make it Quest-ready:

1. **Build step:** `--target Android` alone can't do a zero-code build. Unity's docs say non-desktop
   targets need `--profile` or `--execute-method`. Replace the build command with path A or B from
   `SKILL.md`, and add the Quest signing secrets.
2. **Artifact check:** add the Step 3 `aapt2` / `apksigner` checks after the build, failing the job
   when the APK isn't `arm64-v8a` only, target 34, v2-signed with the release key.

## Licensing caveats (from Unity's skill)

- Service-account auth covers Unity **services**, not the Editor license.
- `unity license activate --personal` is **rejected** for a service account. The generated step
  activates from a **serial** (Pro or Plus), with floating-server and offline `.ulf` alternatives
  commented out.
- Unity Personal users: building on a local machine signed in as yourself is the documented route.
  Running CI on a Personal license is not covered by the docs read for this skill. Check
  `unity license activate --help`.
- A resident Editor holds a seat until it exits. Always run `unity license return --yes` in an
  `always()` / `after_script` step.

## Secrets layout (GitHub Actions example names)

| Secret | Used by |
|---|---|
| `UNITY_SERVICE_ACCOUNT_ID`, `UNITY_SERVICE_ACCOUNT_SECRET` | CLI sign-in. The CLI reads them from the environment, so no login command is needed |
| `UNITY_LICENSE_SERIAL` | `unity license activate --serial` |
| `QUEST_KEYSTORE_B64` (path A) or a keystore file restored from a secret (path B) | Release signing |
| `QUEST_KEYSTORE_PASS`, `QUEST_KEY_ALIAS`, `QUEST_KEY_ALIAS_PASS` | Release signing |

## Shape of the Quest build job (sketch, adapt the generated file)

This is a sketch for orientation. Prefer the generated workflow's install, cache and license steps,
which match the CLI version that produced them.

```yaml
# .github/workflows/quest.yml  (sketch)
jobs:
  quest-apk:
    runs-on: ubuntu-24.04            # the CLI needs glibc >= 2.34 (Ubuntu 22.04+). Editor + Android needs ~10+ GB free
    env:
      UNITY_NON_INTERACTIVE: "1"
      UNITY_NO_BANNER: "1"
      UNITY_SERVICE_ACCOUNT_ID: ${{ secrets.UNITY_SERVICE_ACCOUNT_ID }}
      UNITY_SERVICE_ACCOUNT_SECRET: ${{ secrets.UNITY_SERVICE_ACCOUNT_SECRET }}
    steps:
      - uses: actions/checkout@v4
        with: { lfs: true }
      # ... CLI install, Editor + `android` module (--cm) install, Library cache, license activation:
      #     copy these steps from `unity ci init --target Android`
      - name: EditMode tests
        run: unity test . --mode EditMode --report-format junit --output test-results/editmode.xml --timeout 900
      - name: Build Quest APK (path A: build profile)
        env:
          QUEST_KEYSTORE_B64: ${{ secrets.QUEST_KEYSTORE_B64 }}
          QUEST_KEYSTORE_PASS: ${{ secrets.QUEST_KEYSTORE_PASS }}
          QUEST_KEY_ALIAS: ${{ secrets.QUEST_KEY_ALIAS }}
          QUEST_KEY_ALIAS_PASS: ${{ secrets.QUEST_KEY_ALIAS_PASS }}
        run: |
          unity build . \
            --profile "Assets/Settings/Build Profiles/MetaQuest.asset" \
            --output-path Builds/Quest/Game.apk \
            --android-export-type apk \
            --android-target-sdk-version 34 \
            --android-version-code "${{ github.run_number }}" \
            --android-keystore-base64 "$QUEST_KEYSTORE_B64" \
            --android-keystore-password "$QUEST_KEYSTORE_PASS" \
            --android-key-alias "$QUEST_KEY_ALIAS" \
            --android-key-alias-password "$QUEST_KEY_ALIAS_PASS" \
            --timeout 3600 --format github
      - name: Verify APK
        run: |   # locate aapt2/apksigner under the Editor's AndroidPlayer/SDK, then run the Step 3 checks; fail on mismatch
          echo "see SKILL.md Step 3"
      - uses: actions/upload-artifact@v4
        with: { name: quest-apk, path: Builds/Quest/*.apk }
      - name: Return license seat
        if: always()
        run: unity license return --yes
```

Notes:
- `--format github` turns failures into GitHub annotations. `--report-format junit` gives native
  test results, and the file is written even when tests fail.
- Retry only on exit `6` or `7` from `unity test`, never on `8` (tests failed). For `unity build`,
  `6` means read the log. Upload `data.logFile` as an artifact on failure.
- Cache `Library/` with `unity cache key` (deterministic key), as the generated workflow does.
- Point CI at a Unity Accelerator with `--accelerator host:port` or `UNITY_ACCELERATOR` to cut
  import time.

Sources: official `unity-cli` skill (`diagnostics-maintenance.md`, `build-run-test.md`,
`auth-license-cloud.md`) at Unity-Technologies/skills@9d28210; https://docs.unity.com/en-us/unity-cli/use-unity-cli.md
(compatibility: glibc 2.34+). Last verified: 2026-10-16. The YAML is an unexecuted sketch.
