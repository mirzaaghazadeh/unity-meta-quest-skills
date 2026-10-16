---
name: horizon-launch-roadmap
license: MIT
description: >-
  Plan and sequence a Unity Meta Quest game launch on the Meta Horizon Store:
  which skill to use at each phase, the irreversible decisions (package name,
  signing key, Early Access, pre-order and Coming Soon windows, Data Use Checkup
  auto-approval), and a dated timeline counting back from launch day (Coming
  Soon up to 360 days, pre-orders up to 90 days, submit at least 2 weeks ahead,
  release date at least 14 days out). Use when the user asks "how do I publish
  my Quest game", "what's the release process", "plan my launch", "when should I
  submit", "can I do early access / pre-orders", or starts a new Quest game meant
  for the store.
---

# Horizon Store launch roadmap for a Unity Quest game

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

## Phases and skills

| Phase | Do | Skills |
|---|---|---|
| 0. Setup | Unity ≥ 6000.0.66f2, Meta XR SDK, OpenXR, URP; Dashboard org + app; verify org | `hz-quest-verify-first`, `unity-cli`, `hz-new-project-creation`, `hz-unity-meta-core-sdk`, `quest-unity-project-settings` |
| 1. Build the game | Interaction, UI, comfort, MR features | `hz-unity-meta-interaction-sdk`, `hz-unity-meta-quest-ui`, `hz-immersive-designer`, `hz-unity-meta-mixed-reality-utility-kit` |
| 2. Platform features | Entitlement, achievements, IAP, leaderboards, presence | `hz-unity-platform-sdk` (fill in DUC before submitting) |
| 3. Internal and external testing | Signed builds to ALPHA/BETA (200 users default, up to 2,500); test users | `horizon-store-publish-unity`, `metavr-cli`, `hz-vr-debug` |
| 4. Performance and compliance | OVR Metrics 45-min runs; VRC audit; fix FAILs | `horizon-vrc-checklist`, `hz-perfetto-debug`, `hz-unity-code-review` |
| 5. Listing | Art, metadata, IARC, age group, privacy policy, price | `horizon-store-listing-assets` |
| 6. Submit | Production channel + Submission tab > Submit for Review | `hz-store-submit` |
| 7. Release and live ops | Release now or schedule; sales, promo codes, keys; updates via production; yearly DUC recertification | `horizon-store-listing-assets` (references/policy-pricing.md), `horizon-store-publish-unity` |

## Timeline, counting back from launch day L

| When | Milestone | Rule |
|---|---|---|
| ≤ L-360 d | Coming Soon page live (optional) | Up to 360 days before launch, created during initial submission, needs approval. The older monetization overview says 180 days |
| ≤ L-90 d | Pre-orders open (optional) | Up to 90 days before launch; paid apps or free apps with paid IAP; charged 24 h before launch |
| L-6 to L-4 weeks | Release candidate in RC channel; full VRC audit passes; listing complete | Leave room for one rejection cycle |
| ≥ L-2 weeks | **Submit for Review** | Meta: submit at least two weeks before the target launch |
| ≥ L-14 d | Release date set (pre-order launches) | Full release submission with a date at least 14 days out |
| After approval | Choose release now or a scheduled date; wishlists open | You can't set a release date before approval. Managed releases go through the account manager |
| L | Launch | Copy future builds to production to update users |
| L + 1 year | DUC recertification | Otherwise the app is removed from the Store |

Plan for review taking longer than two weeks. Meta's `hz-store-submit` says holidays and
resubmissions stretch it.

## Before telling the user a date or rule

These rules change. Re-verify with `metavr docs search "<rule>"` or `metavr docs fetch <url>` if
"Last verified" is more than a few months old, and cite the URL in the answer.

## Sources

- https://developers.meta.com/vr/policy/distribution-options/ (2026-03-23)
- https://developers.meta.com/horizon/resources/publish-submit/ (2026-05-08)
- https://developers.meta.com/horizon/resources/publish-app-review/ (2026-07-07)
- https://developers.meta.com/horizon/resources/early-access/ (2025-12-17)
- https://developers.meta.com/horizon/resources/pre-launch-listings/ (2026-08-06)
- https://developers.meta.com/horizon/resources/monetization/ (2026-05-11)
- https://developers.meta.com/horizon/resources/publish-data-use/ (2026-10-14)
- https://developers.meta.com/horizon/resources/age-groups/ (2026-04-02)
- https://developers.meta.com/horizon/resources/publish-organization-verification/ (2026-08-24)
- https://developers.meta.com/horizon/resources/publish-release-channels/ (2026-03-23)
- https://developers.meta.com/horizon/blog/get-apps-ready-app-lab-meta-horizon-store-meta-quest-developers/ (App Lab merge)

Last verified: 2026-10-16
