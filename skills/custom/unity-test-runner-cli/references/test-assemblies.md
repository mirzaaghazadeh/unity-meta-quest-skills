# Test assembly layout and starter tests

Part of **`unity-test-runner-cli`**.

## Rules from the Unity Manual (6000.3)

- Tests live in a **test assembly**: an `.asmdef` that references `nunit.framework.dll`,
  `UnityEngine.TestRunner` and `UnityEditor.TestRunner`. That combination is what marks it as a
  test assembly. The `UnityEditor.TestRunner` reference is only available to Edit mode tests.
- **Edit mode** test assemblies target the **Editor platform only** (`"includePlatforms": ["Editor"]`).
- **Play mode** test assemblies need their own asmdef. To run on a Player such as Quest, the
  platforms must include the target: **Any Platform** (`"includePlatforms": []`) or Android.
- A test assembly **can't reference `Assembly-CSharp`**. Move the game code under test into its own
  asmdef and reference that.
- Use NUnit `[Test]` by default. Use `[UnityTest]` (a coroutine) only when you need to wait frames
  or time (Play mode) or yield Editor instructions (Edit mode).

The easiest way to get exactly the shape your Unity version expects is **Assets > Create >
Testing > Test Assembly Folder** (or the Test Runner window's button) in the Editor, then edit
`includePlatforms` and `references`. The files below are hand-written equivalents to write when no
Editor is available. Recompile after writing them (`unity recompile`) and fix anything the compiler
reports.

## Recommended layout

```
Assets/
  Game/                         # gameplay code
    Game.asmdef                 # { "name": "Game", ... }  -> references Meta/XR asmdefs it uses
    Scripts/...
  Tests/
    EditMode/
      Game.Tests.EditMode.asmdef
      ScoreTests.cs
    PlayMode/
      Game.Tests.PlayMode.asmdef
      SpawnTests.cs
```

To find the asmdef **names** of SDK assemblies your code calls, look at the `"name"` field of the
`.asmdef` files inside the package, e.g. `Library/PackageCache/com.meta.xr.sdk.core*/**/*.asmdef`.
Don't guess them.

## `Assets/Tests/EditMode/Game.Tests.EditMode.asmdef`

```json
{
  "name": "Game.Tests.EditMode",
  "rootNamespace": "",
  "references": ["UnityEngine.TestRunner", "UnityEditor.TestRunner", "Game"],
  "includePlatforms": ["Editor"],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": true,
  "precompiledReferences": ["nunit.framework.dll"],
  "autoReferenced": false,
  "defineConstraints": ["UNITY_INCLUDE_TESTS"],
  "versionDefines": [],
  "noEngineReferences": false
}
```

## `Assets/Tests/PlayMode/Game.Tests.PlayMode.asmdef`

```json
{
  "name": "Game.Tests.PlayMode",
  "rootNamespace": "",
  "references": ["UnityEngine.TestRunner", "UnityEditor.TestRunner", "Game"],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": true,
  "precompiledReferences": ["nunit.framework.dll"],
  "autoReferenced": false,
  "defineConstraints": ["UNITY_INCLUDE_TESTS"],
  "versionDefines": [],
  "noEngineReferences": false
}
```

(`includePlatforms: []` = Any Platform, so these tests can also run on a Quest Player.)

## Starter tests

```csharp
// Assets/Tests/EditMode/ScoreTests.cs: pure logic, runs in milliseconds, no headset needed
using NUnit.Framework;

public class ScoreTests
{
    [Test]
    public void AddingPointsIncreasesScore()
    {
        var score = new Game.Score();        // your type in the Game assembly
        score.Add(10);
        Assert.That(score.Value, Is.EqualTo(10));
    }
}
```

```csharp
// Assets/Tests/PlayMode/SpawnTests.cs: needs frames to pass
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SpawnTests
{
    [UnityTest]
    public IEnumerator SpawnedObjectFallsUnderGravity()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.AddComponent<Rigidbody>();
        var startY = go.transform.position.y;
        yield return new WaitForSeconds(0.5f);
        Assert.Less(go.transform.position.y, startY);
        Object.Destroy(go);
    }

    [Test, Category("Device")]                // only meaningful on a headset; exclude elsewhere
    public void XrDisplayIsRunning()
    {
        var displays = new System.Collections.Generic.List<UnityEngine.XR.XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(displays);
        Assert.IsTrue(displays.Exists(d => d.running));
    }
}
```

Exclude device-only tests in Editor and CI runs with the Unity Test Framework's `-testCategory`
argument, forwarded after `--` (it isn't one of `unity test`'s reserved flags):
`unity test <project> --mode PlayMode -- -testCategory "!Device"`.

## XR testing notes

- Keep XR-free logic in plain C# classes so EditMode tests cover it without a device.
- In Editor PlayMode there is no headset. For interaction flows, use Meta's XR Simulator
  (`hz-xr-simulator-install-and-configure`, `hz-xr-simulator-control`) or mock your input layer
  behind an interface.
- Frame-dependent tests in an **unfocused** Editor can stall. Before `test run --mode playmode`,
  run `unity command editor set_auto_tick --enable true` (Pipeline 0.8: `run_tests`,
  `set_autotick`; see the `unity-pipeline` skill).

Sources: Unity Manual 6000.3 Test Framework pages,
https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction/getting-started/edit-mode-vs-play-mode-tests.md
and `.../getting-started/workflow-create-test-assembly.md`. The asmdef JSON is hand-written to
match what the Test Runner generates. Prefer the Editor's generator when one is available.
Last verified: 2026-10-16.
