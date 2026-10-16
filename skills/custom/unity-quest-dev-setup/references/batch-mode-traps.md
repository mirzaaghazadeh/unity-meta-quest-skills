# Batch-mode traps (Editor scripts run headless)

Read this before running Editor code without a live Editor: `unity run <project> -- -executeMethod
…`, raw `Unity -batchmode -executeMethod …`, CI jobs, build scripts, or project generators. With
a live Editor and `com.unity.pipeline`, prefer `unity command run_script`: most of these traps
come from the batch run quitting before asynchronous Editor work finishes.

**Observed, not from docs**, on 2026-10-16 with Unity 6000.6.6f1 (Apple Silicon), URP 17.6.0,
Unity OpenXR Plugin 1.18.0, XR Plug-in Management 4.7.0, Meta XR Core SDK 207.0.1. Each row points
to the package source that explains it. Re-check after upgrading.

## 1. Asynchronous work that a `-quit` run never finishes

| Trap | What happened | Do instead |
|---|---|---|
| **TextMeshPro resources** | `TMP_PackageResourceImporter.ImportResources(true, false, false)` and `AssetDatabase.ImportPackage(path, false)`, called from `-executeMethod` in a `-batchmode -quit` run, both returned without error and left no `Assets/TextMesh Pro`. The importer calls `UnityEditor.AssetPackage.Package.Import`, which completes later (source: `com.unity.ugui/Runtime/TMP/TMP_PackageResourceImporter.cs`). | Import at launch, before the method runs: `unity assets import "<ugui>/Package Resources/TMP Essential Resources.unitypackage" --project <p>` (wraps the Editor's `-importPackage`; the project must not be open), or add `-importPackage <path>` to a raw batch launch. The package lives in `Library/PackageCache/com.unity.ugui@*/Package Resources/`. In a live Editor the importer works (see `hz-unity-tmp-resources`). |
| **Meta Project Setup "Fix All"** | `OVRProjectSetup.FixAllAsync(group)` queues fixes that run on `EditorApplication.update`. A `-quit` run exits before they apply. Don't `Task.Wait()` it on the main thread (deadlock, per Meta's `hz-unity-meta-core-sdk/references/project-setup-tool.md`). | Launch **without** `-quit`, poll the task from `EditorApplication.update`, then `EditorApplication.Exit(0)`. Example below. Afterwards, `-executeMethod OVRProjectSetupCLI.GenerateProjectSetupReport -reportFile <path>` writes the checklist as JSON (`tasksStatus[]` with `level` and `isDone`). |
| **Package resolution looks hung** | The first batch launch after adding `com.meta.xr.sdk.core` sat at 0% CPU for 11 minutes: Meta's npm registry served the 200 MB tarball at about 0.3 MB/s. | Watch `<project>/Logs/upm.log` for `Downloading package` / `Finished downloading` before assuming a hang. |

`FixAllAsync` in batch mode, with a timeout:

```csharp
public static void FixMetaProjectSetup()   // run WITHOUT -quit; it exits by itself
{
    var groups = new Queue<BuildTargetGroup>(new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone });
    var started = EditorApplication.timeSinceStartup;
    Task current = null;
    EditorApplication.update += Step;
    void Step()
    {
        if (EditorApplication.timeSinceStartup - started > 300) EditorApplication.Exit(1);
        if (current != null && !current.IsCompleted) return;
        if (current != null && current.IsFaulted) { Debug.LogException(current.Exception); EditorApplication.Exit(1); return; }
        if (groups.Count == 0) { EditorApplication.update -= Step; AssetDatabase.SaveAssets(); EditorApplication.Exit(0); return; }
        current = OVRProjectSetup.FixAllAsync(groups.Dequeue());
    }
}
```

`unity run` reserves `-quit`, and whether it always adds it isn't documented (unverified). If it
does, run this through a raw `Unity -batchmode` launch, or use the live-Editor route from Meta's skill.

## 2. Ordering: work that only takes effect after a domain reload

| Trap | What happened | Do instead |
|---|---|---|
| **OpenXR validation after enabling features** | Rules for newly enabled OpenXR features (for example Meta Quest Support) register at Editor load (`[InitializeOnLoadMethod]` in `com.unity.xr.openxr/Editor/OpenXRProjectValidationRulesSetup.cs`). Validating in the same run as enabling misses their rules. | Enable features in one run, validate in the next: `OpenXRProjectValidation.GetCurrentValidationIssues(list, BuildTargetGroup.Android)` is public (namespace `UnityEditor.XR.OpenXR`); apply `rule.fixIt()` where `fixItAutomatic`. |
| **Samples and packages added by script** | Code in an imported package sample isn't compiled until the next launch, so prefabs from it can't be used in the same run. | Import in one run, use in the next. |

## 3. Asset database

| Trap | What happened | Do instead |
|---|---|---|
| **Duplicate folders (`XR 1`)** | `Directory.CreateDirectory("Assets/XR")`, then XR Plug-in Management creating its settings, produced empty duplicates `Assets/XR 1` and `Assets/XR/Settings 1`. The folder existed on disk but not yet in the AssetDatabase. | Create folders with `AssetDatabase.CreateFolder(parent, name)` after checking `AssetDatabase.IsValidFolder`. |
| **Built-in pipeline materials render magenta** | Third-party sample materials (Standard, `Unlit/Color`, custom BiRP shaders) render magenta in URP. | URP has a headless converter: `-executeMethod UnityEditor.Rendering.Universal.Converters.RunInBatchModeCmdLine -container BuiltInToURP -typesFilter Material --inclusive` (`--list` shows containers and types; it calls `EditorApplication.Exit` itself, so don't pass `-quit`; source: `com.unity.render-pipelines.universal/Editor/Tools/Converters/Converters.cs`). It logs "No upgrader found" for `Unlit/Color` and custom shaders: switch those by script, for example to `Universal Render Pipeline/Unlit`, copying `_Color` to `_BaseColor`. |
| **Generated meshes in scenes** | Meshes created in code and only referenced from a saved scene are fragile across regenerations. | Save them as assets: `AssetDatabase.CreateAsset(firstMesh, path)`, then `AddObjectToAsset(mesh, path)` for the rest. |

## 4. Rendering captures in batch mode

| Trap | What happened | Do instead |
|---|---|---|
| **First frame uses stale material data** | The first `Camera.Render()` after `EditorSceneManager.OpenScene` drew every object with the *last* created material's color. | Render once and discard, then capture. The community skill `scenario-unity-expert` reports the same trap on 6.3 as "first frame flat white" (`references/unity-6.3-traps.md`, O1). |
| **TextMeshPro text missing in captures** | TMP builds its mesh on an Editor update, which a batch run doesn't tick. | Call `ForceMeshUpdate()` on every `TMP_Text` before rendering. |
| **3D TextMeshPro lost its position** | A root-level `TextMeshPro` object created by script and positioned with `transform.position` was saved with `m_AnchoredPosition: {x: 0, y: 0}`; only z survived. | Parent the text under a plain `Transform` that carries the position. |
| **Unity's procedural skybox** | `Skybox/Procedural` rendered a green-yellow sky for a low sun (dusk). | Use a small gradient-sky shader. Include the stereo macros (`UNITY_VERTEX_OUTPUT_STEREO` etc.) so it works with single-pass instanced rendering on Quest. |

## 5. Machine and Editor lifecycle

| Trap | What happened | Do instead |
|---|---|---|
| **Disk space** | Hub `install-modules` for Android aborted with `database or disk is full` at 11 GB free: the 1.8 GB installer and its 1.7 GB unpacked copy sat on disk next to the 5.4 GB being installed. After one IL2CPP Quest build, `Library/Bee` was 5.2 GB. | Keep at least 25 GB free for a Quest project. After a failed install, delete leftovers in `~/Library/Application Support/UnityHub/downloads/` and `$TMPDIR/unity-module-pkg-*`, then install the missing child modules only. `Library/Bee` can be deleted (it rebuilds; the next build is slower). |
| **Quitting the Editor from a script** | `osascript -e 'tell application "Unity" to quit'` and quitting by bundle id were both ignored by 6000.6.6f1. | `unity close` (doesn't save; see `unity-cli`) or ask the user. Batch runs need the project closed. |
| **Creating a project offline without the CLI** | — | `Unity -batchmode -quit -createProject <dir> -cloneFromTemplate <Unity.app>/Contents/Resources/PackageManager/ProjectTemplates/com.unity.template.urp-blank-<ver>.tgz` made a URP project in about 1 minute with no sign-in. With the CLI, `unity projects create … --no-cloud` does the same. |
| **Separate simulator package** | `com.meta.xr.simulator` 81.0.1 doesn't compile against Core SDK 207. | See `skills/meta-official/ERRATA.md` ("Observed in a real project"). |

Last verified: 2026-10-16
