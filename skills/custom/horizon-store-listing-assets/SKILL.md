---
name: horizon-store-listing-assets
license: MIT
description: >-
  Prepare and validate a Meta Horizon Store product listing for a Quest game:
  exact art specs (Cover Landscape/Square/Portrait, Hero, Logo, Icon, Spatialized
  tile, screenshots, trailer, 360 preview), asset VRCs, metadata limits (name,
  short and long description, keywords), specs metadata (play area, input,
  comfort), age group self-certification and IARC rating, privacy policy and
  Data Use Checkup, pricing limits, sales, promo codes, keys, Early Access,
  Coming Soon and pre-orders. Use when the user asks for "store assets", "cover
  art size", "hero image dimensions", "screenshots for the Horizon Store",
  "trailer requirements", "store description", "age rating / IARC", "privacy
  policy for my Quest app", "set the price", "run a sale", "generate keys", or
  "early access". Replaces the asset table in Meta's hz-store-submit, which has
  wrong dimensions and limits.
---

# Horizon Store listing: assets, metadata, rating, pricing

Correction to Meta's `hz-store-submit` skill: its asset table and metadata limits are wrong.
It lists a 1024 px icon, a 2560 × 1440 hero, a 3-screenshot minimum, and a 30/150/4000-character
name/short/long. Meta's live pages (verified 2026-10-16) give the values below. Run
`metavr docs fetch https://developers.meta.com/horizon/resources/asset-guidelines/` if this
skill is older than its "Last verified" date. Policy details are in
[references/policy-pricing.md](references/policy-pricing.md).

## 1. Art assets (Store Art Guidelines, updated 2026-05-11)

| Asset | Size (W × H) | Ratio | Format | Rules |
|---|---|---|---|---|
| Cover Landscape | 2560 × 1440 | 16:9 | 24-bit PNG | Same design as the other covers and the Hero |
| Cover Square | 1440 × 1440 | 1:1 | 24-bit PNG | Same design family |
| Cover Portrait | 1008 × 1440 | 7:10 | 24-bit PNG | Same design family |
| Hero Cover | 3000 × 900 | 10:3 | 24-bit PNG | Same design as the covers; branding centered (Asset.4) |
| Logo | variable, max 9000 × 1440 | - | 32-bit PNG | **Transparent background** (Asset.1); legible when scaled |
| Icon | 512 × 512 | 1:1 | 24-bit PNG | Solid fill, **no transparency**, square corners |
| Spatialized tile | 180 × 180 background + 180 × 180 foreground | 1:1 | PNG | Foreground layer transparent; content inside the safe area with 18 dp padding |
| Screenshots | 2560 × 1440 | 16:9 | 24-bit PNG | 5 images, no duplicates; real in-experience content; no branded art |
| Trailer | 1080p min, "2K" max | 16:9 | MP4, H.264, AAC | 30 s to 2 min (Asset.7: ≤ 2 min); may open and close on the logo but not use it throughout |
| Trailer cover | 2560 × 1440 | 16:9 | 24-bit PNG | Landscape |
| 360 preview (cubestrip) | ≥ 12288 × 2048 target | 6:1 | 32-bit PNG, JPEG (q ≥ 80), KTX | ≤ 2.3 MB; face order left, right, up, down, front, back; content ≥ 10 m from the viewer |
| 360 preview (equirect) | ≥ 7680 × 3840 target | 2:1 | 24-bit PNG, JPEG (q ≥ 80), KTX | ≤ 2.3 MB |
| Immersive Image Layers (Left, Right, Logo) | unverified, not on the guidelines page | - | - | Transparent backgrounds (Asset.9). Check the Dashboard upload form |

Global rules:
- Branded assets (covers, hero, logo) must carry the **exact app title**. Only the icon is exempt.
- Keep titles and key focal points in the **safe area**. The bleed area may be cropped. Templates:
  https://metaque.st/storetemplates. Meta's page gives no pixel safe-area values, so use the PSDs.
- The Dashboard has a cropping tool and an AI expansion tool for reaching the required sizes.
- Each extra store language needs its own localized assets.

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
- Hide debug HUDs, FPS counters and dev overlays. Use the game's own UI only.
- Pick moments that match the store description (Content.2: metadata must match the app).

## 2. Metadata (App Metadata page, updated 2025-08-20)

| Field | Limit / rule |
|---|---|
| Name | **max 40 characters**. Meets content guidelines (Publishing.4); no other platforms' names |
| Short description | **max 500 characters** (Publishing.5) |
| Long description | **max 1500 characters** (Publishing.6) |
| Search keywords | optional; relevant only (Publishing.7) |
| Categorization | Category + Genres |
| Specs | Game Modes, Supported Controllers, Play Area, Supported Player Modes, Comfort Level, Internet Connection, Subscription, Purchase-blocked devices, Social Features, Supported Languages in App, Early Access |
| Details | Publisher Name, Website (Publishing.1, must resolve), Privacy Policy (Privacy.1-4), optional External Support (Publishing.2) and Terms of Service (Publishing.3) |
| Content rating | Developer contact email (shared with IARC) + IARC certificate |

Dashboard order: App Submissions > submission > App Metadata (name, descriptions, languages) >
Categorization > Specs > Details > Assets > Content Rating, saving each step.

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
  (every declared modality works on its own) are tested against what you declare.
- **Comfort Level**, per Meta's store policy (unverified on developers.meta.com): Comfortable means
  little or no camera or player motion, typically a fixed camera. Moderate means more camera and
  player motion. Intense means first-person camera motion, acceleration, or significant movement.
  Meta judges the default experience. Meta's App Lab migration blog says Comfort Level and
  Keywords can be updated once the app is on the Horizon Store.

## 3. Age group and rating

- **IARC rating**: request it from the Content Rating step. It's free and instant.
- **Age group self-certification** is separate and mandatory for Quest 2/3-family apps. The values
  are Teens and Adults (13+), Mixed Ages (10-12 plus 13+), and Children (10-12). Mixed Ages apps
  must implement the Get Age Category API within 30 days (`hz-unity-platform-sdk`
  `references/user-age-category.md`) or risk removal. Children apps may not use the API or any
  Platform SDK feature. The same value goes in `ovr-platform-util --age-group`.
- Don't derive the age group from the IARC rating alone. Meta says so explicitly.

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
gating while a request is pending.

## 5. Pricing and promotions (summary; details in references)

- App price: free, or **$0.99 to $299.99** MSRP. Add-ons: $0.99 to $99.99. Local pricing per
  country is supported. Price can change after submission.
- Sales: Growth > Manage Promotions > Create (an admin submits them for review). Meta-run events
  are listed in the Promotion Calendar. An MSRP hold period before discounting may apply; it is
  shown in the submission form.
- Promo codes: Growth > Promo Codes. Not for free apps. Set a usage limit.
- Keys: Distribution > Keys. Up to 250 per request and 2,000 every 6 months.
- Early Access: Specs > Early Access > Enable, for newly published apps only. Disabling it is
  **irreversible**. Explain why the game is in Early Access in the description.
- Coming Soon: up to 360 days before launch (pre-launch page 2026-08-06; the older monetization
  overview says 180). Pre-orders: up to 90 days before launch, created only during the initial
  submission, charged 24 h before launch, price locked, release date set ≥ 14 days out.

## Sources

- https://developers.meta.com/horizon/resources/asset-guidelines/ (2026-05-11)
- https://developers.meta.com/horizon/resources/publish-app-metadata/ (2025-08-20)
- https://developers.meta.com/horizon/resources/publish-quest-req/ (Asset/Privacy/Content/Publishing VRCs, 2026-08-19)
- https://developers.meta.com/horizon/resources/age-groups/ (2026-04-02)
- https://developers.meta.com/horizon/resources/publish-data-use/ (2026-10-14)
- https://developers.meta.com/horizon/resources/monetization/ (2026-05-11), /monetization-pricing/ (2026-03-30)
- https://developers.meta.com/horizon/resources/monetization-sales-promotions/ (2026-04-29), /monetization-custom-promo-codes/ (2026-04-29)
- https://developers.meta.com/horizon/resources/early-access/ (2025-12-17), /pre-launch-listings/ (2026-08-06)
- https://developers.meta.com/vr/policy/distribution-options/ (Keys, 2026-03-23)
- https://developer.oculus.com/policy/store-policies (comfort ratings, IARC; not re-fetched)

Last verified: 2026-10-16
