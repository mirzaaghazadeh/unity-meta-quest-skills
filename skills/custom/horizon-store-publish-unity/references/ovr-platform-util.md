# ovr-platform-util reference (Meta Quest builds)

Condensed from Meta's "Oculus Platform Command Line Utility" page (updated 2026-07-20), "Target
app builds by device" (updated 2026-09-04) and "Asset Files" (updated 2024-09-06). Re-check flags
with `ovr-platform-util <command> --help` or
`metavr docs fetch https://developers.meta.com/vr/resources/publish-reference-platform-command-line-utility/`
because the tool updates itself (`self-update`). Unchanged as of 2026-10-17.

## Get the tool

- `metavr tools install platform-utils` (alias `ovr-platform-util`), the managed install.
- Windows: https://developers.meta.com/vr/downloads/package/platform-utils-cli-pc/
- macOS: https://developers.meta.com/vr/downloads/package/platform-utils-cli-osx/
- Linux (experimental): `https://www.oculus.com/download_app/?id=5159709737372459&access_token=OC%7C5159709737372459%7C`
- Run `ovr-platform-util version` to check it and `ovr-platform-util self-update` to update it.

## Authentication (pick one)

| Method | Flag | Where to get it |
|---|---|---|
| App secret | `-s`, `--app-secret` | Dashboard > app > Development > API ("App Credentials"). Admin only |
| User token | `-t`, `--token` | Dashboard > Account > Account settings > "Get Oculus platform utility token" > Generate token |
| Interactive | `get-access-token [--email] [--password]` | Prints a user token. Prompts for missing values |

All of these are passwords. Store them in a CI secret store and never log them.

## upload-quest-build

| Flag | Req. | Meaning |
|---|---|---|
| `--age-group` | yes | `TEENS_AND_ADULTS`, `MIXED_AGES` or `CHILDREN` |
| `-a`, `--app-id` | yes | App ID (Dashboard API tab) |
| `-s`/`--app-secret` or `-t`/`--token` | one | Auth |
| `--apk` | yes | APK path |
| `-c`, `--channel` | yes | Target channel. Case-insensitive. `store` means production |
| `--obb` | no | One OBB expansion file, up to 4 GB |
| `--assets-dir` | no | Directory of required/DLC asset files |
| `--asset-files-config` | no | JSON that marks files required, targets devices, or links DLC to IAP (spelled `--asset_files_config` elsewhere on Meta's pages) |
| `-n`, `--notes` | no | Release notes. Quote them and write newlines as `\n` |
| `--language-packs-dir` | no | Language packs |
| `--debug-symbols-dir` | no | Symbols dir |
| `--debug-symbols-pattern` | no | For example `*.sym.so` |
| `--draft` | no | Upload as a draft and publish later in the Dashboard |
| `--config` | no | JSON file of parameters |
| `--inherit-asset-files` | no | Reuse the previous build's asset files (default false) |
| `--exclude-addons` | no | Exclude add-ons (default false) |
| `--disable-progress-bar` | no | Use in CI |

Meta's own example:

```bash
ovr-platform-util upload-quest-build --age-group MIXED_AGES \
  --app-id 10001234 --token 1234abcd \
  --apk C:\RabbitHole.apk --channel alpha \
  --notes "Fixes the rabbit spawn bug."
```

The page lists `--age-group` as required but also says an upload without one goes to "draft"
status. Always pass it.

### Device targets on upload

Append device specifiers to the channel, joined with `|` (target-builds-by-device, 2026-09-04):

```bash
ovr-platform-util upload-quest-build --age-group TEENS_AND_ADULTS --app-id "$META_APP_ID" \
  --token "$META_UPLOAD_TOKEN" --apk app.apk --channel "alpha:quest3+"
```

| Specifier | Meaning |
|---|---|
| `quest2only`, `questproonly`, `quest3only` | That device only |
| `quest2+`, `questpro+`, `quest3+` | That device and newer. `quest3+` = Quest 3 family, Meta VR Glasses and future devices |
| `questpro-` | Quest Pro and older |

CLI (and MQDH) values override `com.meta.store.defaultDeviceTargets` in the manifest. In the
Dashboard the same choice is the build's **Supported Devices**, where **Future devices** makes a
build forward-compatible. LIVE must always keep one forward-compatible build, and builds in an
app submission must be forward-compatible. `metavr store dist upload` has no equivalent option.

## Other commands

| Command | Required | Optional | Use |
|---|---|---|---|
| `get-release-channel-data` | `--app-id` | `--token` | List channels: names, IDs, latest versions |
| `set-release-channel-build` | `--age-group`, `--app-id`, secret or token, `--destination-channel` | `--source-channel`, `--build-id`, `--build-dir` | Copy or move a build between channels |
| `upload-add-on` | `--app-id`, secret or token, `--file`, `--sku`, `--min-version-code` | `--metadata` | Upload a DLC/IAP asset file for an add-on SKU |
| `download-quest-build` | `-b`, `-d`, `-t` | `--list` | Download a build |
| `get-access-token` | none | `--email`, `--password` | Get a user token |
| `self-update`, `version` | none | none | Update or print the tool version |

```bash
ovr-platform-util set-release-channel-build --age-group TEENS_AND_ADULTS \
  --app-id 10001234 --app-secret 1234abcd \
  --source-channel beta --destination-channel rc
```

## OBB and asset files (DLC)

- **OBB**: one expansion file per mobile app, up to 4 GB. It downloads automatically at install.
  The Store renames it. Meta's page gives two conflicting patterns:
  `main.[package-name].[version-code].obb` and `main.[version-code].[package-name].obb`. Load the
  OBB through Unity's normal path and never hard-code its name.
- **More than one OBB, or large generic files**: upload them as **required asset files**
  through `--assets-dir` plus a config JSON. Each file can be up to 4 GB, and Meta recommends
  2 GB or less. The Unity Platform Tool page says Quest supports "up to 2 GB of asset files
  that can be used as DLC", which conflicts. Confirm the current limit with `metavr docs search
  "asset files size limit"`.
- **Keep asset file names the same across builds** so patch updates stay small.
- Per-device files go through `supportedDevices` (Meta's example):

```json
{
  "scene_12_Q2.obb":     { "required": true, "supportedDevices": ["quest2"] },
  "scene_12_Q3.obb":     { "required": true, "supportedDevices": ["quest3", "quest3s"] },
  "scene_12_shared.obb": { "required": true, "supportedDevices": ["quest2", "questpro", "quest3", "quest3s"] }
}
```

  The manifest page (2026-10-15) lists the identifiers `quest2`, `questpro`, `quest3`, `quest3s`
  and `vrglasses`. Whether asset-file `supportedDevices` accepts `vrglasses` is unverified: the
  compatibility-mode page (2026-09-04) says VR Glasses aren't a supported public manifest value
  yet, and Meta XR Core SDK 207.0.1 writes the internal key `stanley` (per this library's
  `docs/meta-reference/unity-vr-glasses.md`).
- DLC linked to IAP: the config JSON associates DLC files with IAP SKUs set up in the Dashboard.
  The exact schema is not on the asset-files page. Look it up with
  `metavr docs search "asset files config IAP sku DLC"`. Runtime download code lives in
  `hz-unity-platform-sdk` (`references/asset-file.md`).
- Local testing of required files: install the APK, then push the files into the app's OBB
  directory with adb (Meta's page describes `adb shell pm install -g` followed by `adb push`).

## Gotchas

- Every upload must meet release packaging requirements, alpha included. The manifest and SDK
  checks run right after upload, and errors show in Manage Builds > Test Status.
- Release channel user limit: 200 by default, and you can request up to 2,500.
- `--notes` is user-visible. Don't put internal ticket text in it.
- Uploading to `store` puts the build in production. On an already-live app, entitled users get
  the update. Ask the user first.
