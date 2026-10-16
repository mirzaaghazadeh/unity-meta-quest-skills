# Meta Horizon Store after Connect 2026: condensed reference (Unity edition)

Checked 2026-10-17. This is a summary in our own words, not a copy of Meta's pages. Every row
names its source as `[key date]`; `date` is the page's own "Updated" date, and the keys resolve to
full URLs in [Sources](#sources). "Session only" means the fact comes from a Connect session
abstract and no doc page described it yet. Re-check anything older than a few months with
`metavr docs fetch <url>` or the Markdown twin
`https://developers.meta.com/horizon/llmstxt/resources/<page>.md`.

Since late 2026 every `developers.meta.com/horizon/...` page redirects (HTTP 302) to
`developers.meta.com/vr/...`, and the pages name the `/vr/` URL as canonical. Old links still work.

## 1. What Connect 2026 announced for the Store, and what is documented

Connect keynote and recaps: 2026-09-24. Store session "Grow your revenue and get found":
posted 2026-09-30.

| Announcement | Documented? | What a developer can do today |
|---|---|---|
| Meta VR Glasses (ships spring 2027; same OS, Store, payments and SDKs as Quest; v207 SDKs) | Yes [recap-vr 2026-09-24; glasses-start 2026-10-13] | Target it now with **Future devices** (section 2); test in simulators and with `metavr device fov-sim` on a Quest 3/3S |
| Store favors controller-free titles for VR Glasses owners without controllers; controller-required titles appear once controllers are paired | Blog only [recap-vr 2026-09-24; recap 2026-09-24; glasses-start 2026-10-13] | Make every required flow work with hands (and gaze); declare hands in the manifest only when it works |
| Better recommendations, spotlight promotions, search and ranking (some live, rest "over the next several months") | Blog only [recap-vr 2026-09-24] | Nothing to configure. Keep metadata accurate ([app-discovery 2025-06-03]) |
| A/B Pricing | Session only. The existing Price A/B Testing page was not updated [price-ab 2025-10-01] | Use the existing tool (section 3). Whether it now runs outside the US is **unverified** |
| AI-Powered Analytics | Session only. Closest doc: Meta AI Developer Assistant answers analytics questions in the Dashboard [ai-assistant 2026-09-24] | Ask the assistant (read-only) for metrics, benchmarks and funnel data |
| Advanced F2P tooling (IAP and retention) | Session only. No new doc page after 2026-10-14 | Use add-ons, bundles, targeted promo codes, subscriptions, TBYB (section 5). Specific new tools **unverified** |
| VR ad placement expanded to Instagram, Messenger, Threads | Session only. Ads docs list Facebook and Instagram [app-ads 2026-05-11] | Messenger/Threads placement **unverified** |
| "Meta VR Glasses Prioritization" for next-gen titles | Session only | Same as the controller-free row above |
| Dashboard: improved Benchmarks, User Demographics, "PREQ alerts" | Blog [recap-vr 2026-09-24]; Benchmarks [benchmarks 2026-05-22], Demographics [demographics 2026-05-06] | Section 4. "PREQ alerts" has no doc page (**unverified**; possibly the threshold alerts from August) |

Shipped before Connect and missing from our skills until now (blog [pricing-blog 2026-08-11]):
bundles in store-wide sales, editing live bundles without review, achievement-based TBYB, Bayesian
TBYB analysis, pre-launch funnel analytics, User Demographics, automatic quality alerts.

## 2. Meta VR Glasses: what a Store build needs

| Topic | Rule | Source |
|---|---|---|
| Binary | **64-bit required.** A binary with only 32-bit native libraries cannot run on VR Glasses (it still runs on Quest). In Unity: IL2CPP + ARM64 | [app-compat 2026-10-13] |
| Store delivery | Production build's **Supported Devices** must include **Future devices** plus every current Quest device that should get it. After launch, an approved Production build that targets Future devices reaches VR Glasses with no new APK | [app-compat 2026-10-13; targeting 2026-09-04] |
| "All Devices" | A build with no targeting ("All Devices") includes VR Glasses. Targeting controls delivery only, not compatibility | [targeting 2026-09-04] |
| Forward-compatible rule | LIVE must always hold at least one build that includes Future devices; every build in an app submission must be forward-compatible | [targeting 2026-09-04] |
| Input declaration (merged manifest) | No hand-tracking entries = controllers only. `oculus.software.handtracking required="false"` + `com.oculus.permission.HAND_TRACKING` = controllers and hands. `required="true"` = hands only. Don't declare hands before the hand path works | [app-compat 2026-10-13] |
| Controller-required immersive app | Installs, but a **Switch to Controllers** dialog blocks entry until controllers are active (controllers are sold separately) | [app-compat 2026-10-13; compare 2026-09-30] |
| `com.oculus.supportedDevices` | The release-manifest page lists `vrglasses` beside `quest2|questpro|quest3|quest3s`. **Conflict:** the compatibility-mode page says VR Glasses "are not currently a supported public value" and to use Future devices instead. Rely on Future devices; treat the manifest value as unverified | [manifest 2026-10-15] vs [compat-mode 2026-09-04] |
| Compatibility mode | Horizon OS runs third-party immersive Store apps on VR Glasses in compatibility mode: device-model APIs report an older Quest, and refresh rates are limited to ones both support. Check capabilities, not `Build.MODEL` | [compat-mode 2026-09-04] |
| Field of view | Nominal 70° × 66° (Quest 3: 110° × 96°); render extent 74° × 68°; 2412 × 2288 px per eye. Keep key UI near the center; no hover events (gaze is private) | [fov 2026-09-25; recap 2026-09-24] |
| Simulate FoV on a Quest 3/3S/Pro | `metavr device fov-sim enable` (70 × 66), `--perceived` (58 × 54), `disable`; resets on reboot | [fov 2026-09-25] |
| Platform services | Same Store app = same entitlement, add-ons and services. Cloud Backup restores on install only, is not save sync, 100 MB per backup | [platform-svc 2026-08-25] |
| VRCs | **No VRC was added or changed for VR Glasses.** VRC index still 2026-08-19; no VRC page mentions VR Glasses | [vrc 2026-08-19] |
| Store art | **No new asset type for VR Glasses.** Asset guidelines unchanged since 2026-05-11 | [assets 2026-05-11] |
| Metadata labels | Meta advises against presenting controller/hand support as Store categories, because Store labels and visibility rules can differ from tested behavior. Keep "Supported Controllers" truthful | [app-compat 2026-10-13; metadata 2025-08-20] |
| Testing before hardware | Simulators for input/layout; installation from a channel, thermals, compositor and Cloud Backup are device-only checks | [test-app 2026-09-25] |

## 3. Device targeting (applies to Quest and VR Glasses)

| Mechanism | How | Source |
|---|---|---|
| Dashboard | Build detail page > Supported Devices (Quest 2, Pro, 3, 3S, Future devices). Removing a device from a live build rolls that device back to the previous build | [targeting 2026-09-04] |
| Manifest default | `<meta-data android:name="com.meta.store.defaultDeviceTargets" android:value="quest3+"/>`. Specifiers: `quest2only`, `questproonly`, `quest3only`, `quest2+`, `questpro+`, `quest3+`, `questpro-`, joined with `|`. `quest3+` = Quest 3 family, VR Glasses and future devices | [targeting 2026-09-04] |
| ovr-platform-util | Channel suffix: `--channel "alpha:quest3+"` (or `"alpha:quest2only|questproonly"`). CLI and MQDH values override the manifest | [targeting 2026-09-04] |
| `metavr store dist upload` | Has no device-targeting or age-group option (only `--app-id --channel --apk [--obb] [--notes] [--publish]`) | metavr 1.8.0.17.10 `--markdown-help` (`docs/metavr-cli-reference.md`) |
| Stop selling on a device | New app submission > App Metadata > Specs > **Purchase-Blocked Devices**, then review | [targeting 2026-09-04] |

## 4. Pricing experiments, trials and analytics

| Tool | Key rules | Source |
|---|---|---|
| Price A/B testing | Dashboard > Growth > Price A/B Testing > Create Pricing Experiment. Types: boundary (one price above, one below) or incrementality. Start ≥ 3 days after setup; runs up to 90 days; starts/ends 11:00 PT. **US only.** No edits after submit (cancel and resubmit). Results by email. Ending within 30 days of a sale can cost sale eligibility. Needs App Submission, Manage App, View App permissions | [price-ab 2025-10-01] |
| Creative A/B testing | PDP asset tests (cover art etc.) in the Dashboard | [creative-ab 2024-12-16] |
| Try Before You Buy | Growth > Try before you buy. Time-based (15-30 min in 5-min steps) or achievement-based (needs at least one "simple" achievement; optional fallback time). Audience 25/50/75/100%; below 100% it is a 30-day experiment. No IAP/DLC during a trial. **Not available for 2D apps.** Results combine your data with prior experiments (probability of conversion lift) plus weekly peer benchmarks | [tbyb 2026-06-29] |
| Benchmarks | > 100 lifetime active users; segments under 15 apps hidden; 7-day Sun-Sat windows; funnel, IAP/DLC revenue per active user, retention, crash rate | [benchmarks 2026-05-22] |
| User Demographics | Analytics > User Demographics: age, country/region, language, play-habit heatmap. ≥ 100 lifetime active users and DUC compliance | [demographics 2026-05-06] |
| Alerts | Analytics > Alerting. > 100 lifetime active users. Universal alerts go to org admins. No crash/performance alerts for 2D apps; not on MQDH or web. Threshold alerts (subscribe once): crash rate > 5%, Quest 3 average FPS < 72, every 1-star rating, startup > 19 s; chart annotations mark releases and OS updates | [alerts 2025-10-01; pricing-blog 2026-08-11] |
| Pre-launch funnel | Views, wishlists and pre-orders while in Coming Soon / pre-order | [pricing-blog 2026-08-11; funnel 2026-02-18] |
| Meta AI Developer Assistant | Read-only panel in the Developer Center and Dashboard (sign-in). Answers from docs, known issues, submissions and reviewer feedback, analytics (incl. peer benchmark percentiles), monetization (add-ons, bundles, subscriptions, offer groups, promo codes, sales, trials, gift cards, revenue, refunds), org roles. Drafts a support ticket you submit yourself | [ai-assistant 2026-09-24] |

## 5. F2P, IAP and retention tooling

| Tool | Key rules | Source |
|---|---|---|
| Add-ons | Monetization > Add-ons; bulk TSV upload (SKU, name, description, currency **USD only**, amount, durable/consumable). A **consumable not consumed within 3 days is refunded automatically**. Payment info must be on file first | [add-ons-work 2026-10-14] |
| Add-on review and rating | Store-visible add-ons need review: budget ≥ 7 days. An add-on can carry a different IARC rating; a higher rating than the base app requires a paid add-on | [add-ons-setup 2026-09-28] |
| Bundles | Can join store-wide sales through promo groups; per-country base pricing; live bundles editable without review (descriptions and assets still reviewed); scheduled changes go live 3 days later | [bundles 2026-06-30; pricing-blog 2026-08-11] |
| Sales, promo codes | Unchanged since 2026-04-29 (see `horizon-store-listing-assets`) | [sales 2026-04-29] |
| Meta Credits | **Ended for Store purchases on 2026-06-09**; still used in Horizon Worlds | [credits 2026-06-09] |
| F2P guidance | Transition guide and model guides exist but predate Connect | [f2p-transition 2025-02-19; f2p-models 2025-08-21; iap-tips 2025-07-21] |

## 6. Unchanged since our 2026-10-16 pass

VRC index [vrc 2026-08-19]; release manifest, including targetSdk 34 for apps created since
2026-03-01 [manifest 2026-10-15]; asset guidelines [assets 2026-05-11]; metadata limits
[metadata 2025-08-20]; ovr-platform-util flags [opu 2026-07-20]; release channels
[channels 2026-03-23]; submission [submit 2026-05-08]; pricing bands [pricing 2026-03-30];
pre-launch windows [prelaunch 2026-08-06]; Data Use Checkup [duc 2026-10-14].

## 7. Open questions (unverified)

- Whether `vrglasses` in `com.oculus.supportedDevices` is accepted or ignored at upload (conflict in section 2).
- Whether Price A/B testing left the US, and what the "advanced F2P tooling" and "AI-Powered Analytics" are beyond the docs above.
- Messenger/Threads ad placements; "PREQ alerts".
- The VR Glasses Store label or shelf for hand-tracked titles: how the Store decides "hands-compatible" (manifest declaration, metadata, or both) is not documented.

## Sources

Dates are each page's "Updated" date (blog and video pages: the post date), checked 2026-10-17.

| Key | Page |
|---|---|
| recap | https://developers.meta.com/blog/meta-connect-recap/ (2026-09-24) |
| recap-vr | https://developers.meta.com/blog/meta-connect-recap-start-building-the-future-of-vr/ (2026-09-24) |
| session | https://developers.meta.com/resources/videos/grow-your-revenue-meta-horizon-store/ (2026-09-30) |
| pricing-blog | https://developers.meta.com/vr/blog/smarter-pricing-better-trials-deeper-analytics-for-VR-developers/ (2026-08-11) |
| glasses-start | https://developers.meta.com/vr/essentials/get-started-with-glasses/ (2026-10-13) |
| app-compat | https://developers.meta.com/vr/essentials/app-compatibility/ (2026-10-13) |
| compat-mode | https://developers.meta.com/vr/essentials/compatibility-mode/ (2026-09-04) |
| fov | https://developers.meta.com/vr/essentials/field-of-view/ (2026-09-25) |
| test-app | https://developers.meta.com/vr/essentials/test-your-app/ (2026-09-25) |
| platform-svc | https://developers.meta.com/vr/essentials/platform-services/ (2026-08-25) |
| compare | https://developers.meta.com/vr/essentials/compare-devices/ (2026-09-30) |
| ai-assistant | https://developers.meta.com/vr/essentials/ai-developer-assistant/ (2026-09-24) |
| targeting | https://developers.meta.com/vr/resources/publish-release-channels-device-targeting/ (2026-09-04) |
| manifest | https://developers.meta.com/vr/resources/publish-mobile-manifest/ (2026-10-15) |
| vrc | https://developers.meta.com/vr/resources/publish-quest-req/ (2026-08-19) |
| assets | https://developers.meta.com/vr/resources/asset-guidelines/ (2026-05-11) |
| metadata | https://developers.meta.com/vr/resources/publish-app-metadata/ (2025-08-20) |
| opu | https://developers.meta.com/vr/resources/publish-reference-platform-command-line-utility/ (2026-07-20) |
| channels | https://developers.meta.com/vr/resources/publish-release-channels/ (2026-03-23) |
| submit | https://developers.meta.com/vr/resources/publish-submit/ (2026-05-08) |
| pricing | https://developers.meta.com/vr/resources/monetization-pricing/ (2026-03-30) |
| prelaunch | https://developers.meta.com/vr/resources/pre-launch-listings/ (2026-08-06) |
| duc | https://developers.meta.com/vr/resources/publish-data-use/ (2026-10-14) |
| price-ab | https://developers.meta.com/vr/resources/monetization-price-ab-testing/ (2025-10-01) |
| creative-ab | https://developers.meta.com/vr/resources/ab-testing-instructions/ (2024-12-16) |
| tbyb | https://developers.meta.com/vr/resources/try-before-you-buy/ (2026-06-29) |
| benchmarks | https://developers.meta.com/vr/resources/publish-metrics-benchmarks/ (2026-05-22) |
| demographics | https://developers.meta.com/vr/resources/user-demographics/ (2026-05-06) |
| alerts | https://developers.meta.com/vr/resources/publish-analytics-alerts/ (2025-10-01) |
| funnel | https://developers.meta.com/vr/resources/publish-funnel-analytics/ (2026-02-18) |
| add-ons-work | https://developers.meta.com/vr/resources/working-with-add-ons/ (2026-10-14) |
| add-ons-setup | https://developers.meta.com/vr/resources/add-ons-setup/ (2026-09-28) |
| bundles | https://developers.meta.com/vr/resources/monetization-bundles/ (2026-06-30) |
| sales | https://developers.meta.com/vr/resources/monetization-sales-promotions/ (2026-04-29) |
| credits | https://developers.meta.com/vr/resources/monetization-meta-credits-open-beta/ (2026-06-09) |
| f2p-transition | https://developers.meta.com/vr/resources/monetization-f2p-transition/ (2025-02-19) |
| f2p-models | https://developers.meta.com/vr/resources/monetization-free-to-play-models/ (2025-08-21) |
| iap-tips | https://developers.meta.com/vr/resources/monetization-tips-iap/ (2025-07-21) |
| app-ads | https://developers.meta.com/vr/resources/app-ads/ (2026-05-11) |
| app-discovery | https://developers.meta.com/vr/resources/app-discovery/ (2025-06-03) |
