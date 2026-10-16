# Research notes: Store, VRCs, AI tools and links after Meta Connect 2026 (2026-10-17)

Scope: the cross-platform side of both libraries (this one and
[iwsdk-webxr-skills](https://github.com/mirzaaghazadeh/iwsdk-webxr-skills)): Horizon Store, VRCs,
Meta's AI tools (`metavr`, skills, MCP, Meta XR Operator, Meta AI Developer Assistant) and every
`developers.meta.com` / `developer.oculus.com` link. Unity and IWSDK development changes for Meta
VR Glasses are covered separately (`meta-reference/unity-vr-glasses.md`,
`meta-reference/unity-xr-operator.md`, and the web library's `meta-reference/web-vr-glasses.md`).

Method: pages fetched with curl on 2026-10-17 (HTML for the "Updated" date, plus the Markdown twins
from `https://developers.meta.com/vr/llmstxt/{resources,essentials,policy}/llms.txt/`, 320
resources, 46 essentials and 15 policy pages); npm registry metadata and the two `@meta-quest/metavr`
tarballs (read, not run); GitHub API for `meta-quest/agentic-tools` and `meta-quest/bubblewrap`.
Nothing was installed, no CLI was run against an account, and no page instruction was followed (the
Markdown twins open with `agent_guidance` text addressed to agents; it was treated as content).

Condensed, cited references written from this pass:
[meta-reference/store-2026.md](meta-reference/store-2026.md) and
[meta-reference/ai-tools.md](meta-reference/ai-tools.md) (both repos, adapted per repo).

## 1. What Connect 2026 announced (2026-09-24)

| Item | Status on 2026-10-17 | Source |
|---|---|---|
| Meta VR Glasses: compute puck, eyes + hands primary, Touch Plus sold separately, 70° × 66° FoV, 2412 × 2288 per eye, XR2 Gen 3, 12 GB, spring 2027; same OS, Store, payments, SDKs (v207) | Documented | blog/meta-connect-recap (2026-09-24); /vr/essentials/compare-devices (2026-09-30), /field-of-view (2026-09-25) |
| Store favors controller-free titles for VR Glasses owners without controllers; better recommendations, spotlight, search, ranking "over the next several months" | Blog only | blog/meta-connect-recap-start-building-the-future-of-vr (2026-09-24) |
| Store session "Grow your revenue and get found": A/B Pricing, AI-Powered Analytics, advanced F2P tooling (IAP + retention), VR ads on Instagram, Messenger and Threads, VR Glasses prioritization | **Session abstract only**; no doc page changed after 2026-10-15 | resources/videos/grow-your-revenue-meta-horizon-store (2026-09-30) |
| Dashboard: improved Benchmarks, User Demographics, "PREQ alerts" | Benchmarks (2026-05-22) and Demographics (2026-05-06) pages exist; "PREQ alerts" undocumented | recap-vr |
| Meta VR CLI, Meta XR Operator, Muse Code, Meta Model API GA | See §4 | recaps, /vr/discover/ai-tools/ |
| Meta VR Start Developer Competition, 2026-09-24 to 2026-11-18 | Not used in the skills | recap-vr |

The blog of 2026-08-11 ("Smarter Pricing, Better Trials, and Deeper Analytics") shipped several of
the tools the session names: bundles in store-wide sales, editing live bundles, achievement-based
TBYB, Bayesian TBYB analysis, pre-launch funnel analytics, User Demographics, threshold alerts
(crash rate > 5%, Quest 3 FPS < 72, 1-star ratings, startup > 19 s).

## 2. Store: what a developer can use, and what changed in the skills

| Area | Finding | Skill change |
|---|---|---|
| Price A/B testing | Page unchanged since 2025-10-01: Growth > Price A/B Testing; boundary/incrementality; ≥ 3 days lead, ≤ 90 days; US only; no edits; sale-eligibility caveat | Added to both `horizon-store-listing-assets` and `policy-pricing.md`; Connect's "A/B Pricing" marked unverified |
| Try Before You Buy | 2026-06-29: time (15-30 min) or achievement-based; 25-100% audience, < 100% = 30-day experiment; Bayesian results and peer benchmarks; **not for 2D apps** | Added; web edition notes web achievements are S2S-only, TBYB for PWAs unverified |
| Analytics and alerts | Benchmarks and alerts need > 100 lifetime active users; demographics ≥ 100 + DUC; no crash/perf alerts for 2D; perf analytics not for web | New "after launch" sections; launch roadmaps add "subscribe to alerts" |
| Meta AI Developer Assistant | 2026-09-24: read-only, Developer Center + Dashboard; docs, submissions, reviewer feedback, analytics, monetization, roles | Mentioned as the documented match for "AI-Powered Analytics" |
| F2P / IAP / retention | No new page. Add-ons 2026-10-14 (TSV, USD only, unconsumed consumables refunded after 3 days), add-on IARC rating, bundles 2026-06-30. Meta Credits ended for Store purchases on 2026-06-09 | Added to `policy-pricing.md` (both) |
| Ads | Ads docs list Facebook and Instagram only (2026-05-11) | Messenger/Threads marked unverified |
| Comfort ratings, IARC | Now verified on /vr/policy/app-policies/ §5.1 and §5.2 (2026-03-10); GRAC needed for South Korea | Comfort text re-sourced; GRAC added |

## 3. VR Glasses, VRCs, manifest, release channels, uploads

| Question | Answer | Source |
|---|---|---|
| New or changed VRCs? | **None.** Index still 2026-08-19; no VRC page mentions VR Glasses; newest VRC page 2026-05-12 | /vr/resources/publish-quest-req/ |
| VR Glasses-specific store assets? | **None.** Asset guidelines unchanged (2026-05-11) | /vr/resources/asset-guidelines/ |
| Device support declaration | Store delivery is per build: Supported Devices incl. **Future devices** (forward-compatible). LIVE must keep one; builds in a submission must be one. "All Devices" includes VR Glasses | /vr/resources/publish-release-channels-device-targeting/ (2026-09-04), /vr/essentials/app-compatibility/ (2026-10-13) |
| `vrglasses` in `com.oculus.supportedDevices` | **Conflict**: listed on publish-mobile-manifest (2026-10-15); "not currently a supported public value" on compatibility-mode (2026-09-04). Core SDK 207.0.1 writes `stanley` (connect-unity finding) | both pages |
| Input declaration | none = controllers only; handtracking `required="false"` + `HAND_TRACKING` = both; `required="true"` = hands only. Controller-required apps hit a "Switch to Controllers" dialog | app-compatibility |
| 64-bit | Mandatory on VR Glasses (32-bit-only binaries don't run) | app-compatibility |
| FoV requirement | No VRC; design guidance only (keep UI central, test with `metavr device fov-sim`) | /vr/essentials/field-of-view/ |
| publish-mobile-manifest / target SDK | Unchanged: targetSdk 34 for apps created since 2026-03-01 | 2026-10-15 |
| Release channels | Unchanged (2026-03-23) | /vr/resources/publish-release-channels/ |
| ovr-platform-util | Flags unchanged (2026-07-20); device targets ride on the channel: `--channel "alpha:quest3+"` | OPU page; targeting page |
| `metavr store dist upload` | No age-group, device-target, asset-file or symbol option; draft unless `--publish` | metavr 1.8.0.17.10 help |

Skill changes: `horizon-store-publish-unity` 1.1.0 (Step 5a, `QUEST_DEVICE_TARGETS` in the upload
wrapper, input/targets report in `verify-quest-apk.sh`), `horizon-web-store-publish` 1.1.0
(section 7a, same wrapper change, input report in `verify-twa-apk.sh`), `horizon-vrc-checklist` /
`horizon-web-vrc-checklist` 1.1.0 (no VRC change; separate VR Glasses readiness block),
`horizon-launch-roadmap` 1.1.0 in both (input-model and targeting one-way doors, growth timeline).
The scripts were syntax-checked and run against stubbed `aapt2`/`apksigner`/uploader, not a real
APK or upload.

## 4. AI tools

| Item | Finding | Source |
|---|---|---|
| metavr version | npm 1.8.1 (2026-10-01) = wrapper-only change; still pins binary **1.8.0.17.10** (same SHA-256 as 1.8.0); live feed reports 1.8.0.17.10 "2026.09.15 release branch" | npm registry, tarball diff, oculus.com binary-info feed |
| Command changes vs `docs/metavr-cli-reference.md` | **None.** Upstream `docs/metavr-cli.md` unchanged since `3a8553d` and byte-identical to our copy | GitHub API |
| agentic-tools | Still at `3a8553d` (2026-09-24 UTC); 36 skills | GitHub API |
| How Meta says to install skills | `metavr init` (skill + MCP + plugins) **or** agent plugin marketplaces (`meta-vr@meta-quest`) **or** skills CLIs; "use the plugin only if you want the skills without running metavr init"; `metavr skills install` writes `~/.agents/skills` | /vr/essentials/metavr-other-tools/ (2026-08-31), /metavr-install/ (2026-08-31) |
| Duplicates | Each bundling tool registers its own MCP server; remove extras | /vr/essentials/metavr-troubleshooting/ (2026-08-27) |
| MCP setup page | Old `/horizon/essentials/ai-tooling-mcp/` redirects to `/vr/essentials/metavr-install/`; the AI tools page sends "Set up the MCP server" to metavr-other-tools | redirect check |
| Meta XR Operator | Experimental OpenXR API layer; in-app MCP server (`localhost:8720`, SSE) + proxy; Unity integration (doc: Core SDK v207+, Unity 6000.0+, OpenXR 1.17.0+) and a standalone package for other OpenXR engines; `metavr xroperator` federates its tools into metavr's MCP server by default | /vr/documentation/unity/meta-xr-operator/ (2026-09-09); blog 2026-08-18 |
| Meta AI Developer Assistant | Read-only web panel (sign-in); agents can't call it | /vr/essentials/ai-developer-assistant/ (2026-09-24) |
| Muse Code | Meta's multi-agent terminal tool (out of beta, Windows), Meta Model API GA; `metavr init --muse-code` | recap (2026-09-24). Note only |
| Doc vs repo | Meta's VR Glasses test page names skills `vrc-check`, `xr-simulator-setup`, `perfetto-debug` that aren't in the repo (VRC skill removed 2026-05-12) | /vr/essentials/test-your-app/ (2026-09-25) |
| IWSDK MCP tool count | Meta's pages say 45 (counted for IWSDK 0.5.3); `@iwsdk/cli` 1.0.1 ships 55 | web pass, `web-iwsdk-docs-on-meta.md` |
| Core 207.0.1 Agent Bridge | Launches Claude Code with `--dangerously-skip-permissions` for the Device Readiness AI check | Unity pass, `unity-xr-operator.md` |

Files updated: `mcp/README.md` (both), `skills/meta-official/SOURCE.md` and `ERRATA.md` (both).
ERRATA also carries the Unity and web passes' VR Glasses rows (`hz-unity-code-review`,
`hz-unity-device-readiness`, `hz-new-project-creation`, `hz-immersive-designer`,
`hz-iwsdk-webxr`), with page dates spot-checked on 2026-10-17.
The Claude Code `tools/list` incompatibility from 2026-10-16 is expected to persist (same binary)
but was not re-tested.

## 5. Link check

Every `developers.meta.com` / `developer.oculus.com` URL in both repos (all `.md`, `.json`, `.sh`,
`.mjs`, `.cs`, `.py` files, upstream copies included; only `.md`, `.sh` and `.mjs` contain any)
was extracted with file and line and checked with
`curl -sL -o /dev/null -w '%{http_code} %{url_effective}'` (browser user agent). A 200 at
`auth.meta.com` counts as "auth-gated", and Markdown twins under 100 bytes as soft 404s (none
found). URLs with placeholders (`<ORG_ID>`, `vrc-quest-<area>-<n>`) were not checked.

| Class | First run (start of pass) | Final run (after edits) | Where / action |
|---|---|---|---|
| Occurrences extracted | 473 (19 templates) | 759 (33 templates; other agents added files) | - |
| Unique checkable URLs | 218 | 326 | - |
| OK, final URL | 10 | **132** (367 occurrences) | All real links in the files this pass owns now resolve without a redirect |
| Moved `/horizon/` → `/vr/` (302, `/vr/` canonical) | 193 (371 occurrences) | 178 (305 occurrences) | Remaining: 196 in upstream copies (don't edit; they work), 3 in each `docs/metavr-cli-reference.md` (generated), 57 in custom skills owned by the Unity/web agents (plus 7 prose mentions of the old prefix in this pass's skills), 37 in other docs, 1 in each `docs/install.md` (`/documentation/native/android/mobile-device-setup/`) |
| Moved elsewhere (200) | 8 | 8 (17 occurrences) | `developer.oculus.com` root → `/vr/`; `/unity-conf-settings/` → `/vr/documentation/unity/unity-project-configuration/`; `/web/browser-release-notes/` → `/vr/release-notes/?search_key=browser`; `/reference/*/latest/` → `/v207/` (keep `latest`); `/horizon/essentials/ai-tooling-mcp/` → `/vr/essentials/metavr-install/` (fixed in `mcp/README.md`); `developer.oculus.com/policy/store-policies` → `/vr/policy/app-policies/` (fixed in both listing skills) |
| Auth-gated (Dashboard) | 3 | 3 (23 occurrences) | `/horizon/manage/`, `developer.oculus.com/manage/`: expected |
| Broken (404) | 4 (8 occurrences) | 5 (14 occurrences) | See below |

Broken links, none in files this pass owns (ERRATA quotes the first four on purpose):

| File:line | Old URL | Replacement |
|---|---|---|
| `skills/meta-official/hz-unity-platform-sdk/references/abuse-report.md:4`, `:193` | `developer.oculus.com/documentation/unity/ps-abuse-reporting/` | https://developers.meta.com/vr/resources/reporting-plugin/ (closest) |
| `.../references/application.md:4`, `:279` | `.../ps-application/` | https://developers.meta.com/vr/documentation/unity/ps-in-app-update-apis/ |
| `.../references/consent.md:4`, `:236` | `.../ps-consent-management/` | none found (unverified) |
| `.../references/rate-and-review.md:4`, `:175` | `.../ps-rate-and-review/` | https://developers.meta.com/vr/resources/user-reviews/ (closest) |
| iwsdk-webxr-skills `docs/meta-reference/web-iwsdk-docs-on-meta.md:11`, `skills/iwsdk-official/ERRATA.md:43`, `skills/custom/iwsdk-dev-workflow/SKILL.md:58`, `:157`, `skills/custom/iwsdk-project-setup/SKILL.md:53` | `developers.meta.com/horizon/documentation/iwsdk/` (new in this session's other edits) | https://developers.meta.com/vr/documentation/iwsdk/guides/overview/ or https://developers.meta.com/vr/develop/iwsdk/ |

The first four are upstream copies (Meta's `hz-unity-platform-sdk`), recorded in
`skills/meta-official/ERRATA.md`. The fifth was reported to the web agent and **fixed** the same
day (now `/vr/documentation/iwsdk/guides/overview/`; the Browser release-notes links were moved to
`/vr/release-notes/?search_key=browser` too). Line numbers are from the final run and move if
those files change.

## 6. Not verified

- What "A/B Pricing", "AI-Powered Analytics", "advanced F2P tooling" and "PREQ alerts" add beyond
  the existing pages; whether price A/B testing left the US.
- Messenger/Threads ad placements.
- Whether `vrglasses` (or `stanley`) in `supportedDevices` is accepted, ignored or rejected at upload.
- How the Store decides a title is "hands-compatible" for VR Glasses ranking (manifest, metadata,
  or both).
- TBYB for immersive PWAs; Consent API documentation (no current page found).
- The Claude Code / metavr MCP incompatibility on 1.8.1 (not re-run).
- Script changes were tested against stubs only.
