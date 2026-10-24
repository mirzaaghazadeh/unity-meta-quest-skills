---
name: horizon-store-listing-assets
license: MIT
metadata:
  author: Navid Mirzaaghazadeh
  author-url: https://github.com/mirzaaghazadeh
  repository: https://github.com/mirzaaghazadeh/unity-meta-quest-skills
  version: "1.2.0"
description: >-
  Prepare and validate a Meta Horizon Store product listing for a Quest game:
  exact art specs (covers, Hero, Logo, Icon, Spatialized tile, screenshots,
  trailer, 360 preview), asset VRCs, metadata limits, specs (play area, input,
  comfort), age group and IARC rating, privacy policy and Data Use Checkup,
  pricing, price A/B tests, Try Before You Buy, sales, bundles, promo codes,
  keys, Early Access, Coming Soon, pre-orders, Store analytics and alerts, and
  what Meta VR Glasses change. Use when the user asks for "store assets",
  "cover art size", "hero image dimensions", "screenshots for the Horizon
  Store", "trailer requirements", "store description", "age rating / IARC",
  "privacy policy for my Quest app", "set the price", "A/B test my price",
  "free trial", "run a sale", "generate keys", "early access", or "store
  analytics". Replaces the asset table in Meta's hz-store-submit, which has
  wrong dimensions and limits.
---

# Horizon Store listing: assets, metadata, rating, pricing

> Made by [Navid Mirzaaghazadeh](https://github.com/mirzaaghazadeh) · part of [unity-meta-quest-skills](https://github.com/mirzaaghazadeh/unity-meta-quest-skills) · MIT

Correction to Meta's `hz-store-submit` skill: its asset table and metadata limits are wrong.
It lists a 1024 px icon, a 2560 × 1440 hero, a 3-screenshot minimum, and a 30/150/4000-character
name/short/long. Meta's live pages (verified 2026-10-17) give the values below. Run
`metavr docs fetch https://developers.meta.com/vr/resources/asset-guidelines/` if this
skill is older than its "Last verified" date. Policy details are in
[references/policy-pricing.md](references/policy-pricing.md); what Connect 2026 changed is in
this repo's `docs/meta-reference/store-2026.md`.

**Meta VR Glasses (ships spring 2027) don't change the listing:** no new asset type, no new
metadata field and no new VRC as of 2026-10-17. What changes is reach: the Store shows
hand-capable titles first to VR Glasses owners without controllers (Connect recap, 2026-09-24).
Declare only the input your build supports (section 2).

## 1. Art assets (Store Art Guidelines, updated 2026-05-11)

| Asset | Size (W × H) | Ratio | Format | Rules |
|---|---|---|---|---|
| Cover Landscape | 2560 × 1440 | 16:9 | 24-bit PNG | Same design as the other covers and the Hero |
| Cover Square | 1440 × 1440 | 1:1 | 24-bit PNG | Same design family |
| Cover Portrait | 1008 × 1440 | 7:10 | 24-bit PNG | Same design family |
| Hero Cover | 3000 × 900 | 10:3 | 24-bit PNG | Same design as the covers; branding centered (Asset.4) |
| Mini Landscape | 1080 × 360 | 3:1 | 24-bit PNG | In the spec table; the page doesn't say where it's shown. The Dashboard's Assets form had **no field** for it on 2026-10-24 |
| Logo | 360 × 360 min, 9000 × 1440 max | - | 32-bit PNG | **Transparent background** (Asset.1); legible when scaled. **Optional** in the Dashboard form |
| Icon | 512 × 512 | 1:1 | 24-bit PNG | Solid fill, **no transparency**, square corners |
| Spatialized tile | Dashboard: 512 × 512 background + 512 × 512 foreground; 180 × 180 versions optional | 1:1 | PNG | **Optional.** Background solid; foreground transparent, inside a 392 × 392 safe area (Dashboard form, 2026-10-24). The guidelines page gives only 180 × 180 with 18 dp padding |
| Screenshots | 2560 × 1440 | 16:9 | 24-bit PNG | 5 images, no duplicates; real in-experience content; no branded art |
| Trailer | 1080p min, "2K" max | 16:9 | MP4, H.264, AAC | 30 s to 2 min (Asset.7: ≤ 2 min); may open and close on the logo but not use it throughout |
| Trailer cover | 2560 × 1440 | 16:9 | 24-bit PNG | Landscape |
| 360 preview (cubestrip) | ≥ 12288 × 2048 target; Dashboard minimum 6000 × 1000 | 6:1 | 32-bit PNG, JPEG (q ≥ 80), KTX | Optional. ≤ 2.3 MB; face order left, right, up, down, front, back; content ≥ 10 m from the viewer |
| 360 preview (equirect) | ≥ 7680 × 3840 target; Dashboard minimum 6000 × 3000 | 2:1 | 24-bit PNG, JPEG (q ≥ 80), KTX | Optional. ≤ 2.3 MB |
| Immersive Image Layers (Left, Right, Logo) | unverified, not on the guidelines page | - | - | Transparent backgrounds (Asset.9). The Dashboard's Assets form had no field for them on 2026-10-24 |

Global rules:
- **PNG depth:** every asset accepts 24- or 32-bit PNG, but "the 8-bit alpha layer is ignored
  unless it's for the Logo". Use 24-bit everywhere except the Logo, so transparent areas don't
  render unexpectedly.
- Branded assets (covers, hero, logo) must carry the **exact app title**. Only the icon is exempt.
- Keep titles and key focal points in the **safe area**. The bleed area may be cropped. Templates:
  https://metaque.st/storetemplates. Meta's page gives no pixel safe-area values, so use the PSDs.
- The Dashboard has a cropping tool and an AI expansion tool for reaching the required sizes.
  The Assets form (2026-10-24) also offers a **Universal basic asset**: one PNG of at least
  2560 × 1440 from which it generates every cover size. Each cover upload opens a **Safe area
  preview** ("Use image") that shows whether the title stays clear of the cropped edges. Required
  there: the three covers, the hero, the icon and 5 screenshots; logo, spatialized tile, trailer
  and 360° preview are optional.
- Each extra store language needs its own localized assets.
- Test cover art before a launch or a sale with the Dashboard's **creative A/B test** on the
  product page (ab-testing-instructions, 2024-12-16).

Asset VRCs to self-review: Asset.2 (cover art shows branding only, no taglines or banners),
Asset.3 (Rec: no text in the top or bottom 20% of cover art), Asset.5 (screenshots: no added logos,
text or icons), Asset.6 (no other VR platforms' hardware or logos anywhere), Asset.8 (Rec: art text
≥ 24 pt), Asset.10 (Rec: "Captured on Meta Quest Pro." on Pro-exclusive footage), Functional.6
(only Meta Quest headsets and controllers shown).

**Validate a folder of assets:** `scripts/check-store-assets.sh <dir>` checks names, pixel sizes,
alpha, screenshot count, and trailer duration, codec and resolution. It uses `sips` on macOS or
ImageMagick, plus `ffprobe`. File names it expects: `cover_landscape.png`, `cover_square.png`,
`cover_portrait.png`, `hero.png`, `logo.png`, `icon.png`, `tile_background.png`,
`tile_foreground.png`, `screenshot_*.png`, `trailer.mp4`, `trailer_cover.png`.

**Capturing screenshots from a Unity game:**
- Capture on device, not from the Editor. Meta's hz-store-submit also notes that reviewers compare
  shots with the real app.
- `metavr capture screenshot -o shots/s1.png --width 2560 --height 1440`. The `--width` and
  `--height` flags exist (default 1024 × 1024). Check the output size, and if it isn't 2560 × 1440,
  crop or scale to 16:9 without adding text.
- Hide debug HUDs, FPS counters and dev overlays. Use the game's own UI only. If the build has
  the experimental **Simulate Device Field of View** mask on, turn it off first.
- Pick moments that match the store description (Content.2: metadata must match the app).

## 2. Metadata (App Metadata page, updated 2025-08-20)

| Field | Limit / rule |
|---|---|
| Name | **max 40 characters**. Meets content guidelines (Publishing.4); no other platforms' names |
| Short description | **max 500 characters** (Publishing.5) |
| Long description | **max 1500 characters** (Publishing.6) |
| Search keywords | optional, **up to 5**; relevant only (Publishing.7). The field picks from suggestions and rejected a keyword with a space ("hand tracking") on 2026-10-24 |
| Categorization | Category + a **required primary genre** + up to 2 more, plus optional Gameplay mechanics. Game genres offered (2026-10-24): Action, Adventure, Education, Hangout, Narrative, Party game, Puzzle, Rhythm, Role playing, Sandbox, Simulation, Sports. No Casual or Arcade genre: use keywords for those |
| Specs | Game Modes, Supported Controllers, Play Area, Supported Player Modes, Comfort Level, Internet Connection, Subscription, Purchase-blocked devices, Social Features, Supported Languages in App, Early Access. The Dashboard pre-fills some answers from the uploaded build (Internet Connection, for one); check each against the real game |
| Details | Developer and Publisher (64 characters each; Developer defaults to the team name until "Use team name" is unticked), Website (Publishing.1, must resolve), Privacy Policy (Privacy.1-4), optional External Support (Publishing.2) and Terms of Service (Publishing.3) |
| Content rating | Developer contact email (shared with IARC) + IARC certificate |

Dashboard order: App Submissions > submission > App Metadata (name, descriptions, languages) >
Categorization > Specs > Details > Assets > Content Rating, saving each step. A submission has
four tabs, **Submission**, **Pricing**, **Build** and **App metadata** (2026-10-24). **Metadata
preview** (Start preview) shows the product page in the Store, the phone app and the headset for
an hour, visible only to you.

Copywriting checklist:
- The first sentence of the short description says what the player does, in VR terms ("Climb...",
  "Duel...").
- Every claim is visible in the build (Content.2). No "coming soon" features unless Early Access
  is enabled and explained.
- Don't name competing platforms or hardware (Asset.6, Functional.6). Use Meta brand names only
  as the Brand Guidelines allow (Publishing.8).
- No keyword stuffing, all-caps titles, or price or sale text in the name or art.
- **Play Area** and **Supported Controllers** must match reality. Tracking.1 (6.5' × 6.5' for
  standing/roomscale; seated means no standing, no floor reach, no turning past 90°) and Tracking.2
  (every declared modality works on its own) are tested against what you declare. The build's
  manifest declares input too: no hand-tracking entry = controllers only; `required="false"` =
  controllers and hands; `required="true"` = hands only (app-compatibility, 2026-10-13). Keep the
  two consistent. Meta notes that Store labels and visibility rules can differ from tested
  behavior, so don't market hand support you haven't tested.
- **Purchase-Blocked Devices** hides the app from a device's Store (target-builds-by-device,
  2026-09-04).
- **Comfort Level** (App policies §5.1, 2026-03-10): Comfortable generally avoids camera movement,
  player motion and disorienting effects (typically a fixed camera); Moderate has more camera and
  player motion; Intense usually has first-person camera motion, acceleration or significant
  movement. Meta's App Lab migration blog says Comfort Level and Keywords can be updated once the
  app is on the Horizon Store.

## 3. Age group and rating

- **IARC rating**: request it from the Content Rating step. It's free and instant. Apps released
  in **South Korea** also need a **GRAC** rating (App policies §5.2.1, 2026-03-10).
- Paid add-ons can carry their own IARC rating; one rated higher than the base app must be paid
  (add-ons-setup, 2026-09-28).
- **Age group self-certification** is separate and mandatory for Quest 2/3-family apps. The values
  are Teens and Adults (13+), Mixed Ages (10-12 plus 13+), and Children (10-12). Mixed Ages apps
  must implement the Get Age Category API within 30 days (`hz-unity-platform-sdk`
  `references/user-age-category.md`) or risk removal. Children apps may not use the API or any
  Platform SDK feature. The same value goes in `ovr-platform-util --age-group`.
- Don't derive the age group from the IARC rating alone. Meta says so explicitly.

In the Dashboard (observed 2026-10-24):

- **Age group** is now on the submission's **Build** tab: "Before you can submit this app for
  review, you must self-certify its age group" > Start. The last step is a legal
  self-certification in the developer's name; leave Confirm to the user. The build upload dialog
  also warns that it's "required before publishing a binary".
- **IARC:** Content rating > country of origin and contact email > **Add certificate** opens the
  IARC questionnaire in a new tab, where the user accepts IARC's terms. Follow-up questions appear
  as you answer (for example Digital Purchases > "Purchases of digital goods" > random items?
  trading?). The tab ends on "IARC Certificate Created". A game with one durable unlock and no
  violence came back as ESRB Everyone and PEGI 3, with the "In-Game Purchases" notice.
- **Sharing preferences:** the Submission tab shows "Please confirm your app sharing preferences
  before submitting". Those settings cover players streaming and capturing the app, under Meta's
  Sharing Terms, which the user accepts or declines.

## 4. Privacy policy and Data Use Checkup

Write a privacy policy that the team hosts, with a stable URL, and that covers:
1. Who you are, and a contact for privacy requests (Privacy.1).
2. **What data** the game collects: Platform SDK data (user ID, display name, followers),
   analytics/crash SDKs, voice chat, device and performance data, purchases (Privacy.2).
3. **How it is used and shared**, including third-party SDKs and retention (Privacy.3).
4. **How users request deletion** of their data, plus response time and channel (Privacy.4).
5. Children and teens, if Mixed Ages or Children (COPPA/GDPR responsibility sits with you).

Data Use Checkup is required if the game uses Platform SDK features (User ID, User Profile,
Followers, Invites, Blocking, Matchmaking, Parties, Deep Linking, Avatars, Challenges, IAP,
Subscriptions, User Age Group). An admin selects features, describes each use, and certifies. Meta
reviews the privacy policy as part of it. Auto-approval ends at the first review submission or a
raised channel limit. Recertify **yearly** or the app is removed. Test users are exempt from DUC
gating while a request is pending. DUC compliance also gates the User Demographics dashboard.

**Filling it in** (Requirements > Data Use Checkup, observed 2026-10-24; the user signs it). The
page warns that DUCs "are most commonly rejected for privacy policy issues and incomplete feature
requests".

1. **Add** each feature the game uses: User ID, User profile, User age group, In-app purchases
   and/or downloadable content, Subscriptions, Avatars, Deep linking, Friends, Blocked users,
   Invites, Parties, Challenges, Device ban. Each request asks for:
   - a use case and a description of how the feature adds value and why it's needed;
   - optional screenshots;
   - an "If approved, I agree…" checkbox.

   **Add to request** stays disabled until all of that is filled in; if it stays disabled after
   pasting text, type a character. Without User ID and User Profile the page recommends them "if
   you wish to use the Platform SDK".
2. **Submit requests** opens **Data handling**:
   - data processors or service providers with access to Meta user data;
   - the responsible person or entity, and its country;
   - whether you gave users' data to public authorities in the past 12 months;
   - your policies for such requests: Required review, Provisions for challenging, Data
     minimization, Documentation, **None of the above**, or "prohibited from answering", which
     Meta warns may restrict access;
   - your organization's country.
3. **Review requests**, then **Submit**. The privacy policy URL is reviewed with the request, and
   the form ends with an "I certify compliance with all applicable Meta terms and policies…" box.
   While one checkup is under review, no other can be submitted.

These are statements in the developer's name. An agent can fill the form in, but should leave
the certification boxes to the user and must not claim policies the developer doesn't have.

## 5. Pricing, experiments and promotions (summary; details in references)

- App price: free, or **$0.99 to $299.99** MSRP. Add-ons: $0.99 to $99.99. Local pricing per
  country is supported. Price can change after submission. (A web app packaged as a PWA can only
  be free; that doesn't apply to Unity APKs.)
- **Add-ons** (Monetization > Add-ons, observed 2026-10-24):
  - **SKU**, which the game's code uses (`hz-unity-platform-sdk`);
  - **Add-on Type** (Durable was preselected);
  - **Applications**, where you tick the app;
  - optional **Show in Store**, a store listing for the add-on that the content team reviews;
  - then **Pricing** and **Metadata** (the name and description shown in Meta's purchase dialog).

  **Publish stays disabled** until the Data Use Checkup includes in-app purchases.
- **Price A/B test** (US only, 2025-10-01 page): Growth > Price A/B Testing > Create Pricing
  Experiment; a boundary or incrementality test that starts ≥ 3 days after setup and runs up to
  90 days. It can't be edited after submission and can cost sale eligibility if it ends within
  30 days of a sale. Connect 2026 listed "A/B Pricing" as new; no page describes changes yet.
- **Try Before You Buy**: time-based (15-30 min) or achievement-based (ends when a "simple"
  achievement unlocks). Meta suggests ≤ 50% of users for the first trial; any audience below 100%
  runs as a 30-day experiment. Not available for 2D apps. TBYB titles get a dedicated Store shelf.
- Sales: Growth > Manage Promotions > Create (an admin submits them for review). Meta-run events
  are listed in the Promotion Calendar. An MSRP hold period before discounting may apply; it is
  shown in the submission form. **Bundles** can now join store-wide sales and be edited live.
- Promo codes: Growth > Promo Codes. Not for free apps. Set a usage limit.
- Keys: Distribution > Keys. Up to 250 per request and 2,000 every 6 months.
- Early Access: Specs > Early Access > Enable, for newly published apps only. Disabling it is
  **irreversible**. Explain why the game is in Early Access in the description.
- Coming Soon: up to 360 days before launch (pre-launch page 2026-08-06; the older monetization
  overview says 180). Pre-orders: up to 90 days before launch, created only during the initial
  submission, charged 24 h before launch, price locked, release date set ≥ 14 days out. The
  pre-launch funnel in Analytics shows views, wishlists and pre-orders meanwhile.
- **Meta Credits** can no longer be used for Store purchases (since 2026-06-09).

## 6. After launch: analytics and alerts

| Tool | Where / rule |
|---|---|
| Benchmarks | Analytics > Benchmarking; > 100 lifetime active users; weekly peer comparison of funnel, revenue per active user, retention, crash rate (2026-05-22) |
| User Demographics | Analytics > User Demographics; ≥ 100 lifetime active users and DUC compliance; age, country, language, play hours (2026-05-06) |
| Alerts | Analytics > Alerting; subscribe to threshold alerts (crash rate > 5%, Quest 3 average FPS < 72, each 1-star rating, startup > 19 s) and benchmark alerts (alerts page 2025-10-01; Meta blog 2026-08-11) |
| Meta AI Developer Assistant | Read-only chat in the Dashboard (icon after Help): metrics, benchmarks, reviewer feedback, monetization data. Suggest it to the user; agents can't call it (2026-09-24) |

## Sources

All pages moved from `developers.meta.com/horizon/` to `/vr/` (the old links redirect).

- https://developers.meta.com/vr/resources/asset-guidelines/ (2026-05-11)
- https://developers.meta.com/vr/resources/publish-app-metadata/ (2025-08-20)
- https://developers.meta.com/vr/resources/publish-quest-req/ (Asset/Privacy/Content/Publishing VRCs, 2026-08-19)
- https://developers.meta.com/vr/resources/age-groups/ (2026-04-02)
- https://developers.meta.com/vr/resources/publish-data-use/ (2026-10-14)
- https://developers.meta.com/vr/resources/monetization/ (2026-05-11), /monetization-pricing/ (2026-03-30), /monetization-price-ab-testing/ (2025-10-01)
- https://developers.meta.com/vr/resources/try-before-you-buy/ (2026-06-29), /ab-testing-instructions/ (2024-12-16)
- https://developers.meta.com/vr/resources/monetization-sales-promotions/ (2026-04-29), /monetization-custom-promo-codes/ (2026-04-29), /monetization-bundles/ (2026-06-30), /monetization-meta-credits-open-beta/ (2026-06-09)
- https://developers.meta.com/vr/resources/early-access/ (2025-12-17), /pre-launch-listings/ (2026-08-06), /add-ons-setup/ (2026-09-28)
- https://developers.meta.com/vr/resources/publish-metrics-benchmarks/ (2026-05-22), /user-demographics/ (2026-05-06), /publish-analytics-alerts/ (2025-10-01)
- https://developers.meta.com/vr/resources/publish-release-channels-device-targeting/ (2026-09-04)
- https://developers.meta.com/vr/essentials/app-compatibility/ (2026-10-13), /ai-developer-assistant/ (2026-09-24)
- https://developers.meta.com/vr/policy/distribution-options/ (Keys, 2026-03-23), /app-policies/ (comfort ratings §5.1, IARC/GRAC §5.2, 2026-03-10)
- https://developers.meta.com/vr/blog/smarter-pricing-better-trials-deeper-analytics-for-VR-developers/ (2026-08-11); https://developers.meta.com/blog/meta-connect-recap-start-building-the-future-of-vr/ (2026-09-24)
- Meta Developer Dashboard, observed while listing a Quest app with a durable add-on (2026-10-24): Assets form, App metadata forms, Build and Submission tabs, IARC questionnaire, Monetization > Add-ons, Data Use Checkup

Last verified: 2026-10-24
