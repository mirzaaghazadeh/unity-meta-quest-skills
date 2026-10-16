# Contributing

Suggestions for new skills, fixes and updated values are welcome. Open an issue or a pull request.

## Proposing a third-party skill

Third-party skills are only accepted when they meet all of these:

1. **The license allows redistribution** (MIT, Apache-2.0, BSD, Unlicense, CC-BY or similar).
   Repos without a license can be listed as links in `docs/community-candidates.md`, but not copied.
2. **They are accurate for current Unity 6 and Quest development**, and don't duplicate a skill
   that's already here.
3. **They pass review.** Every `SKILL.md` and script gets read for prompt injection, pipe-to-shell
   from unknown hosts, edits to agent configuration, credential access and network calls.

Copy the folder **unchanged** into `skills/community/<owner>__<skill-name>/`. Add the upstream
`LICENSE` (and `NOTICE`, if there is one), plus a `SOURCE.md` with the repo URL, commit SHA, license,
fetch date and review notes. Then add a row to `THIRD_PARTY_NOTICES.md` and the catalog in
`README.md`. Never copy real `.env` files, keystores or credentials.

## Changing a custom skill (`skills/custom/`)

- Keep the `name:` field equal to the folder name, and keep the `description:` precise: what it
  does and when to use it.
- Every number, flag or requirement must cite a Meta or Unity page. If you can't confirm a value,
  mark it unverified rather than guessing.
- Update the skill's `Last verified:` date.
- Contributions to `skills/custom/` are licensed under the repository's MIT license.

## Official Meta and Unity skills

Don't edit `skills/meta-official/` or `skills/unity-official/` in place. Refresh them from upstream
(see each `SOURCE.md`). Record any disagreement with the live docs in
`skills/meta-official/ERRATA.md`, or report it to Meta or Unity.

## Security

Don't post secrets in issues or pull requests. If you find a security problem in a collected
skill, open an issue without the exploit details, or report it to the upstream author.
