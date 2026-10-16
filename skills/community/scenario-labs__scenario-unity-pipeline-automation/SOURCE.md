# Source: scenario-unity-pipeline-automation

- **Repo:** https://github.com/scenario-labs/skills
- **Path:** `skills/game-engines/unity/scenario-unity-pipeline-automation/`
- **Commit:** `49dd26e47371163ae0020c7f5627d0993626bfc2` (2026-10-15)
- **License:** MIT, Copyright 2026 Scenario Inc. (`LICENSE` copied)
- **Stars at fetch:** 1032
- **Fetched:** 2026-10-16

## Why it was picked

It covers the Blender-to-Unity content pipeline and build automation:
- deterministic `AssetPostprocessor` import rules, proven with `-consistencyCheck`;
- scripted prefab and scene edits through serialization (correct Undo and SetDirty);
- Addressables grouped by loading condition, with zero implicit duplicates;
- content-validation Edit Mode tests as a pre-build gate;
- one `-buildTarget` on every stage;
- release builds from a clean git tree with provenance;
- a generated `ci/pipeline.sh` and GitHub Actions workflow that use the official Unity CLI (`unity test`, `unity build`, `unity run`).

It pairs directly with the Blender skills: FBX "All + Apply Transform" from `scenario-blender-expert`.

## Security review

- I read the SKILL.md. I grepped references, the 12 C# files and `ut_pipeline.py`.
- **subprocess targets:** Unity, `blender -b --factory-startup`, read-only `git` (`rev-parse`, `status`), and `ruby -ryaml` to validate generated YAML.
- **The generated GitHub Actions template** contains `curl -fsSL https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.sh | UNITY_CLI_CHANNEL=beta bash`. That is Unity's official CLI installer, run on the CI runner, not locally. Review the workflow before committing it.
- `PipelineBuild.cs` honours `AGENTKIT_SKIP_GATE=1` to skip its own pre-build gate.

## Caveats

- **macOS player as the reference target.** Set `--target Android` and the Quest-specific player settings yourself. Meta's official skills cover the OpenXR, Meta and manifest side.
- **Personal licenses cannot activate in CI** (Hub sign-in only). The workflow expects a paid seat or service account.
- It needs `scenario-labs__scenario-unity-expert` installed as a sibling folder named `scenario-unity-expert`.
