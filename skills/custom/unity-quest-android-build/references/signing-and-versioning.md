# Signing and versioning a Quest release build

Part of **`unity-quest-android-build`**.

## The release keystore

- One keystore and one key alias for the app's **entire life**. Meta (`hz-store-submit`): "Once you
  upload a build with a specific signing key, all future builds must use the same key." Losing the
  key means a new app entry.
- **Ask the user** whether a keystore already exists before creating one. If it exists, ask for the
  path and alias, and have them put the passwords in env vars or a secret store. Never accept
  passwords pasted into the conversation or into a command literal.
- Keep it **outside the repository**, and make sure `.gitignore` covers `*.keystore` and `*.jks`.

### Create one (only if the user confirms there is none)

Use the `keytool` from the Editor's OpenJDK module. On macOS it is
`/Applications/Unity/Hub/Editor/<ver>/PlaybackEngines/AndroidPlayer/OpenJDK/bin/keytool` (checked on
6000.6.6f1). On other OSes run `find "<Editor dir>" -name keytool -type f`. The user should run it **themselves**, because it
prompts for passwords:

```bash
"$KEYTOOL" -genkeypair -v -keystore ~/secure/quest-release.keystore \
  -alias quest-release -keyalg RSA -keysize 2048 -validity 10000
```

You can also use Unity's **Keystore Manager** (Player Settings > Android > Publishing Settings).

Then list it without printing secrets: `"$KEYTOOL" -list -v -keystore <file> -alias <alias>`.
That prompts for the password. Record the SHA-256 fingerprint in the project notes.

### Supplying it to a build

| Build path | How the key gets in |
|---|---|
| `unity build --profile …` (path A) | `--android-keystore-base64 "$QUEST_KEYSTORE_B64" --android-keystore-password "$QUEST_KEYSTORE_PASS" --android-key-alias "$QUEST_KEY_ALIAS" --android-key-alias-password "$QUEST_KEY_ALIAS_PASS"`. The CLI writes a temp keystore and deletes it on exit or interrupt |
| `--execute-method QuestBuild.BuildRelease` (path B) | Env vars `QUEST_KEYSTORE_PATH`, `QUEST_KEYSTORE_PASS`, `QUEST_KEY_ALIAS`, `QUEST_KEY_ALIAS_PASS` |
| Live Editor `unity command build` (path C) | Whatever is set in Player Settings. Unity doesn't save keystore passwords to disk, so a new session asks again |

Base64 for path A, without a trailing newline or wrapping:

```bash
base64 < ~/secure/quest-release.keystore | tr -d '\n' > /tmp/ks.b64     # macOS / Linux
# store the contents as a CI secret, e.g. QUEST_KEYSTORE_B64, then delete the temp file
```

Unity's CLI docs warn that flag values "surface in the process list and can be echoed into CI logs".
Env vars only avoid hard-coding them, so also mask them in the CI system. `unity shell` masks
keystore flags in its history file.

### Persistence side effects

`PlayerSettings.Android.keystoreName` and `keyaliasName` are saved into
`ProjectSettings/ProjectSettings.asset` when the Editor saves settings. Path B sets the keystore to
the absolute path of the CI file, which can create noisy diffs. Discard those changes on CI agents
rather than committing them. Passwords aren't written to disk.

## Version code and version name

- `versionCode` (integer) must be **strictly higher** than every build uploaded to any release
  channel. `versionName` (`PlayerSettings.bundleVersion`) is the display string.
- Path A: `--android-version-code <N>`. Path B: `QUEST_VERSION_CODE=<N>`, plus optionally
  `QUEST_VERSION_NAME=1.2.0`.
- CI: use the CI run number (`${{ github.run_number }}`), or something derived from a tag, such as
  `major*10000 + minor*100 + patch`. The scheme must never go down.
- `unity build --versioning-strategy semantic|tag|custom` derives a version string from git
  (`custom` takes `--build-version`). It rejects a dirty working tree unless you pass
  `--allow-dirty-build`. How that string maps to the Android `versionCode` isn't documented, so set
  `--android-version-code` explicitly.

## Check before upload

```bash
"$APKSIGNER" verify --verbose --print-certs Builds/Quest/<App>.apk
#  "Verified using v2 scheme (APK Signature Scheme v2): true"
#  Signer certificate DN must be your release identity, not "CN=Android Debug"
#  The SHA-256 digest must match the fingerprint recorded for the release key
"$AAPT2" dump badging Builds/Quest/<App>.apk | grep -E "versionCode|versionName"
```

Sources: Unity CLI `build` flags (official `unity-cli` skill, `references/build-run-test.md`);
`PlayerSettings.Android` (https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/playersettings/android.md);
Meta `hz-store-submit` (meta-quest/agentic-tools). Last verified: 2026-10-16.
