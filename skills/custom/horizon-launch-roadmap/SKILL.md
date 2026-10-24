---
name: horizon-launch-roadmap
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.2.0"
description: >-
  Plan and sequence a Unity Meta Quest game launch on the Meta Horizon Store:
  which skill to use at each phase, the irreversible decisions (package name,
  signing key, Early Access, pre-order and Coming Soon windows, Data Use Checkup
  auto-approval, Meta VR Glasses input and targeting), and a dated timeline
  counting back from launch day (Coming Soon up to 360 days, pre-orders up to
  90 days, submit at least 2 weeks ahead, release date at least 14 days out),
  plus post-launch growth tools (alerts, price A/B tests, Try Before You Buy).
  Use when the user asks "how do I publish my Quest game", "what's the release
  process", "plan my launch", "when should I submit", "can I do early access /
  pre-orders", "should I support Meta VR Glasses", or starts a new Quest game
  meant for the store.
---

# Horizon Store launch roadmap for a Unity Quest game

> Made by [Navid Mirzaaghazadeh](https://github.com/mirzaaghazadeh) · part of [unity-meta-quest-skills](https://github.com/mirzaaghazadeh/unity-meta-quest-skills) · MIT

This is a routing and planning skill. Each phase points to the skill that does the work. Since
2024-07-31 every Quest app ships through the Meta Horizon Store; App Lab is merged
(distribution-options). Review has two stages: technical (the VRCs), then content
("completeness, polish, and value", per publish-app-review, 2026-07-07). Press coverage of the
2024 App Lab merge said subjective-quality rejections would end. Meta's current page still
describes a content review, so plan for one.

## Decide early: one-way doors

| Decision | Why it's hard to undo | Source |
|---|---|---|
| **Package name** | Must be unique and is the app's identity on the Store | publish-mobile-manifest |
| **Signing keystore** | Every update must use the same key. Back it up in two places | hz-store-submit (Meta skill) |
| **Version code scheme** | Only goes up, across every channel and draft | publish-mobile-manifest; unity-prepare-for-publish |
| **Early Access** | Can be enabled only for **newly published** apps. Disabling is **irreversible** | early-access (2025-12-17) |
| **Coming Soon / pre-orders** | Can be created **only during the initial submission**. Pre-order price can't change. You can't move the release date to within 2 weeks of today | pre-launch-listings (2026-08-06) |
| **Data Use Checkup** | Auto-approval ends at the first review submission or a raised channel limit. Test every Platform SDK feature before that | publish-data-use (2026-10-14); hz-store-submit |
| **Age group** | Mixed Ages requires the Get Age Category API within 30 days. Children apps lose all Platform SDK features | age-groups (2026-04-02) |
| **Org verification** | Without it you can't publish, update or recertify | publish-organization-verification |
| **Input model (Meta VR Glasses)** | VR Glasses (spring 2027) ship without controllers, and the Store shows hand-capable titles first to people without them. Retrofitting hands late is expensive; decide at design time whether every flow works with hands (and gaze) | app-compatibility (2026-10-13); Connect recap (2026-09-24) |
| **Device targeting** | LIVE must always keep a forward-compatible build (**Future devices**), and every build in a submission must be one. A launched device can't be dropped from all builds; you can only hide the app with Purchase-Blocked Devices | target-builds-by-device (2026-09-04) |

## Phases and skills

| Phase | Do | Skills |
|---|---|---|
| 0. Setup | Unity ≥ 6000.0.66f2, Meta XR SDK, OpenXR, URP; Dashboard org + app; verify org | `hz-quest-verify-first`, `unity-cli`, `hz-new-project-creation`, `hz-unity-meta-core-sdk`, `quest-unity-project-settings` |
| 1. Build the game | Interaction, UI, comfort, MR features; hands and gaze if you target VR Glasses | `hz-unity-meta-interaction-sdk`, `hz-unity-meta-quest-ui`, `hz-immersive-designer`, `hz-unity-meta-mixed-reality-utility-kit`, `meta-vr-glasses-unity` |
| 2. Platform features | Entitlement, achievements, IAP, leaderboards, presence; create add-ons (Monetization > Add-ons; publishing one needs the DUC) | `hz-unity-platform-sdk` (fill in DUC before submitting; steps in `horizon-store-listing-assets` §4) |
| 3. Internal and external testing | Signed builds to ALPHA/BETA (200 users default, up to 2,500); test users | `horizon-store-publish-unity`, `metavr-cli`, `hz-vr-debug` |
| 4. Performance and compliance | OVR Metrics 45-min runs; VRC audit; fix FAILs | `horizon-vrc-checklist`, `hz-perfetto-debug`, `hz-unity-code-review` |
| 5. Listing | Art, metadata, IARC, age group, privacy policy, price | `horizon-store-listing-assets` |
| 6. Submit | A **draft build in the Production (LIVE) channel** attached on the Build tab (only those can be attached), age group self-certified, sharing preferences confirmed, then Submission tab > Submit for Review (the user's click) | `horizon-store-publish-unity` Step 7, `hz-store-submit` |
| 7. Release and live ops | Release now or schedule; subscribe to analytics alerts; sales, bundles, promo codes, keys; price A/B tests (US only) and Try Before You Buy once there is data; updates via production; yearly DUC recertification | `horizon-store-listing-assets` (references/policy-pricing.md), `horizon-store-publish-unity` |

## Timeline, counting back from launch day L

| When | Milestone | Rule |
|---|---|---|
| ≤ L-360 d | Coming Soon page live (optional) | Up to 360 days before launch, created during initial submission, needs approval. The older monetization overview says 180 days |
| ≤ L-90 d | Pre-orders open (optional) | Up to 90 days before launch; paid apps or free apps with paid IAP; charged 24 h before launch |
| L-6 to L-4 weeks | Release candidate in RC channel; full VRC audit passes; listing complete | Leave room for one rejection cycle |
| ≥ L-2 weeks | **Submit for Review** | Meta: submit at least two weeks before the target launch |
| ≥ L-14 d | Release date set (pre-order launches) | Full release submission with a date at least 14 days out |
| After approval | Choose release now or a scheduled date; wishlists open | You can't set a release date before approval. Managed releases go through the account manager |
| L | Launch | Copy future builds to production to update users. Subscribe to threshold alerts (crash rate > 5%, Quest 3 FPS < 72, 1-star ratings, startup > 19 s); analytics need > 100 lifetime active users |
| L + weeks | Growth experiments | Price A/B test: starts ≥ 3 days after setup, up to 90 days, US only, keep it 30+ days away from a sale. TBYB: first trial at ≤ 50% audience (30-day experiment) |
| Spring 2027 | Meta VR Glasses on sale | An approved Production build marked Future devices reaches them with no new APK |
| L + 1 year | DUC recertification | Otherwise the app is removed from the Store |

Plan for review taking longer than two weeks. Meta's `hz-store-submit` says holidays and
resubmissions stretch it.

## Before telling the user a date or rule

These rules change. Re-verify with `metavr docs search "<rule>"` or `metavr docs fetch <url>` if
"Last verified" is more than a few months old, and cite the URL in the answer.

## Sources

Pages moved from `developers.meta.com/horizon/` to `/vr/` (old links redirect).

- https://developers.meta.com/vr/policy/distribution-options/ (2026-03-23)
- https://developers.meta.com/vr/resources/publish-submit/ (2026-05-08)
- https://developers.meta.com/vr/resources/publish-app-review/ (2026-07-07)
- https://developers.meta.com/vr/resources/early-access/ (2025-12-17)
- https://developers.meta.com/vr/resources/pre-launch-listings/ (2026-08-06)
- https://developers.meta.com/vr/resources/monetization/ (2026-05-11)
- https://developers.meta.com/vr/resources/publish-data-use/ (2026-10-14)
- https://developers.meta.com/vr/resources/age-groups/ (2026-04-02)
- https://developers.meta.com/vr/resources/publish-organization-verification/ (2026-08-24)
- https://developers.meta.com/vr/resources/publish-release-channels/ (2026-03-23), /publish-release-channels-device-targeting/ (2026-09-04)
- https://developers.meta.com/vr/resources/monetization-price-ab-testing/ (2025-10-01), /try-before-you-buy/ (2026-06-29), /publish-analytics-alerts/ (2025-10-01)
- https://developers.meta.com/vr/essentials/app-compatibility/ (2026-10-13)
- https://developers.meta.com/blog/meta-connect-recap/ (Connect 2026, 2026-09-24)
- https://developers.meta.com/vr/blog/smarter-pricing-better-trials-deeper-analytics-for-VR-developers/ (alert thresholds, 2026-08-11)
- https://developers.meta.com/vr/blog/get-apps-ready-app-lab-meta-horizon-store-meta-quest-developers/ (App Lab merge)
- Meta Developer Dashboard, observed while submitting a Quest app (2026-10-24): Build and Submission tabs, Add-ons, Data Use Checkup

Last verified: 2026-10-24
