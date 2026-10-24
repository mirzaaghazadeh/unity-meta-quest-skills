# Store policy, pricing and promotion details

Every value below comes from developers.meta.com (now under `/vr/resources/<page>/`) and was
fetched on 2026-10-16, with the experiment, trial, bundle and analytics sections added on
2026-10-17 and Developer Dashboard observations (age group, Data Use Checkup) on 2026-10-24. The page date is in brackets. Anything marked **unverified** was not confirmed on a
Meta page. Look it up with `metavr docs search "<topic>"` before quoting it to the user.

## Prerequisites to publish
- **Org verification** [2026-08-24]: admin (government ID, usually minutes) or business
  (incorporation docs). Without it you can't publish, update apps, or submit DUC recertification.
- **Payout and tax info** [publish-overview-smooth-payment, date not checked]: keep the payout
  details and the financial entity field accurate, because banks have rejected payouts over
  wrong data. **unverified**: the exact tax form steps.
- **Developer Distribution Agreement**: an org admin signs it under Legal Documents before the
  first upload. **unverified** on the docs site; the source is Meta's hz-store-pwa skill plus
  forum reports.

## Review [publish-app-review 2026-07-07, publish-submit 2026-05-08]
- Two stages. **Technical review** checks against the VRCs. **Content review** looks at
  completeness, polish and value, and starts only after technical review passes.
- A failed technical review comes with a report that links fixes. Resubmit after fixing.
- Submit **at least two weeks** before the target launch. In saturated categories, review may
  stop at the first guideline violation.
- After approval, set the release date: release immediately or schedule it. Users can wishlist
  between approval and release. Managed releases set the date through the account manager.
- Since 2024-07-31 every Quest app goes through the Meta Horizon Store; App Lab is merged
  [distribution-options 2026-03-23].

## Pricing [monetization-pricing 2026-03-30]
| Item | Min | Max |
|---|---|---|
| App MSRP (unless free) | $0.99 | $299.99 |
| Add-on MSRP | $0.99 | $99.99 |
| Subscription monthly / quarterly / bi-annual / annual | $0.99 / $2.99 / $4.99 / $9.99 | $39.99 / $119.99 / $199.99 / $399.99 |
- Subscription increases: monthly capped at the lower of $5 or 50%. Quarterly, bi-annual and
  annual caps follow the same pattern with $15/$25/$50. One increase per 90 days, effective 7 days
  after publishing.
- Price can change after submission, within consumer-protection law. Price A/B tests work best
  after about 3 sales.
- **unverified**: the revenue share percentage, which isn't on these pages and is set by the
  developer agreement.

## Sales and promo codes [2026-04-29]
- Sales: Dashboard > app > **Growth > Manage Promotions > Create**. Enter name, product, dates and
  discount %, review the currency conversions, then Save as Draft and Submit for Review. A sale
  can't be changed once processed. Only org/team admins can create one.
- There may be an MSRP hold before the first discount; the period is stated in the submission
  form. **unverified**: the min/max discount % and cooldown rules, which these pages don't give.
- Meta events: enroll in the Promotion Calendar. Cancel during enrollment, or email
  metaqueststore@meta.com up to 24 h before the event starts.
- Custom promo codes: **Growth > Promo Codes > Create New**. No limit on concurrent campaigns.
  Set a usage limit. Codes work only on published apps and IAP, never on free apps.

## Keys [distribution-options 2026-03-23]
- **Distribution > Keys > Create**: up to **250 keys per request** and **2,000 every 6 months**.
  The allocation refreshes every 6 months. Meta may cut it, grant more, or charge a fee.
- **unverified** (search-result summary only): keys are 25-character codes, generated after the
  app is approved and redeemed in the Meta Quest app, optionally tied to an IAP SKU.
- Keys are for press and creators; release channels (up to 2,500 users) are for testers.

## Early Access [2025-12-17]
- A label for **newly published** apps. Enable it under Distribution > App Submissions > Edit app >
  App Metadata > Specs > Early Access, then submit for review.
- Turning it off goes through the same path and is **irreversible**. Afterward the app may appear
  in New Releases.
- Explain in the description why the app is in Early Access. **unverified**: Early Access pricing
  rules; a Meta help page says EA apps may be priced lower.

## Coming Soon and pre-orders [pre-launch-listings 2026-08-06]
- Coming Soon is allowed up to **360 days** before launch. The monetization overview
  [2026-05-11] still says 180, but the newer page wins.
- Pre-orders: up to **90 days** before launch, for paid apps or free apps with paid IAP.
  Customers are charged **24 h before launch**, and the price can't change during the pre-order.
- Both listings can be created **only during the initial app submission** and need approval.
  Launch needs a full release submission with a date set **≥ 14 days** out. You can't move the
  date to within 2 weeks of today. Reverting a pre-order to Coming Soon cancels all pre-orders.

## Age groups [2026-04-02]
- Teens and Adults (13+); Mixed Ages (10-12 and 13+); Children (10-12). Users under 10 are not
  allowed on the platform.
- Mixed Ages: implement the Get Age Category API within 30 days. After 60 days without it, the
  app loses some Platform SDK features.
- Children: no Get Age Category API and no Platform SDK features. User data may not be shared
  with Meta.
- Set it in My Apps > app > Production build > App Product Detail Page > Age Group
  Self-Certification > Confirm age group. In the Dashboard of 2026-10-24 it sits on the app
  submission's **Build** tab ("Age group self-certification" > Start), and must be done before
  Submit for Review.

## Data Use Checkup [2026-10-14]
- Required for Platform SDK features: User ID, User Profile, Followers, Invites, Blocked Users,
  Matchmaking, Parties, Deep Linking, Avatars, Challenges, IAP, Subscriptions, User Age Group.
- Requests are auto-approved during early development until the first review submission or a
  raised release-channel limit. Meta's hz-store-submit skill puts that threshold at 200 users.
- Recertify **yearly**, or the app is removed from the Store and loses platform features.
- Test users are exempt while a request is pending.
- The Dashboard form (Requirements > Data Use Checkup, 2026-10-24) lists User ID, User profile,
  User age group, In-app purchases and/or downloadable content, Subscriptions, Avatars, Deep
  linking, Friends, Blocked users, Invites, Parties, Challenges and Device ban. It asks a use case
  and description per feature, then data-handling questions, then a certification. Steps:
  SKILL.md §4. An add-on can't be published until the DUC includes in-app purchases.

## Price A/B testing [monetization-price-ab-testing 2025-10-01]
- Dashboard > app > **Growth > Price A/B Testing > Create Pricing Experiment**. Needs the App
  Submission, Manage App and View App permissions.
- Types: **boundary** (one price above and one below the current price) and **incrementality**
  (how big a change pays off).
- The start date is at least **3 days** after setup; the tool suggests the earliest end date for a
  significant result; maximum **90 days**. Tests start and end at 11:00 Pacific.
- **United States only.** A submitted test can't be edited, only cancelled and resubmitted.
  Results arrive by email.
- If a test (or a later price change) falls within **30 days** of a sale event, sale eligibility
  may suffer.
- Connect 2026 (session posted 2026-09-30) listed "A/B Pricing" as new. **unverified**: what
  changed; the page still says US only.

## Try Before You Buy [try-before-you-buy 2026-06-29; Meta blog 2026-08-11]
- Dashboard > Growth > **Try before you buy** > Enable Now. Only for published Store apps;
  **not for 2D apps**.
- Time-based: 15-30 minutes in 5-minute steps. Achievement-based: the trial ends when a chosen
  "simple" achievement unlocks (implement it with the Platform SDK first), with an optional
  fallback time.
- Audience 25/50/75/100%, picked at random. Below 100% it runs as a **30-day experiment**; 100%
  stays on until disabled. Meta suggests ≤ 50% for the first trial.
- One trial per user per app; trials pause when the headset comes off; no IAP or DLC during a
  trial, but achievements are kept. Disabling ends every trial at once with an upsell.
- Analytics: probability of conversion lift (Bayesian, combined with prior experiments), change in
  conversion, estimated purchase-value lift, weekly peer benchmarks by genre.

## Bundles, add-ons and Meta Credits
- Bundles [monetization-bundles 2026-06-30; blog 2026-08-11]: can be added to promo groups so they
  join store-wide sales; per-country base price; live bundles can be edited without review
  (descriptions and assets are still reviewed); scheduled changes go live 3 days later.
- Add-ons [working-with-add-ons 2026-10-14; add-ons-setup 2026-09-28]: bulk TSV upload (prices in
  **USD only**, converted at purchase); a consumable that the app doesn't consume within **3 days
  is refunded automatically**; Store-visible add-ons need about **7 days** of review; add-ons can
  have their own IARC rating (higher than the base app only if paid).
- Meta Credits [2026-06-09]: no longer usable for Store purchases; Horizon Worlds only.

## Analytics and alerts [publish-metrics-benchmarks 2026-05-22; user-demographics 2026-05-06; publish-analytics-alerts 2025-10-01]
- All three need more than (or at least) **100 lifetime active users**. Demographics also needs
  DUC compliance.
- Benchmarks: weekly (Sun-Sat) comparison against segments of 15+ apps: reach→PDP, PDP→purchase,
  add-on revenue per active user, retention, crash rate.
- Alerts: Analytics > Alerting. Universal alerts ("Low User Crash Rate", "Low App Ratings",
  "High Average Max Consecutive Stale Frames") go to org admins; no crash or performance alerts for 2D apps. The August 2026 blog lists
  subscribe-once threshold alerts: crash rate > 5%, Quest 3 average FPS < 72, each 1-star rating,
  startup > 19 s, plus chart annotations for releases and OS updates.
- **unverified**: the "AI-Powered Analytics" and "PREQ alerts" named at Connect 2026. The Meta AI
  Developer Assistant [ai-developer-assistant 2026-09-24] already answers analytics questions in
  the Dashboard.
