# Unity patterns for the VRCs Unity games most often fail

The API names come from Meta's Unity "Application lifecycle handling" page (updated 2026-10-07)
unless marked **verify**. For those, confirm the signature with
`metavr docs api-details <Class>` or `metavr docs api-search "<query>"` before you write code.

## Lifecycle: pause (Functional.2) and focus awareness (Input.4)

Meta's documented event order:
- App start: HMD Mounted, then Input Focus Acquired.
- Universal Menu opened: Input Focus Lost. Closed: Input Focus Acquired.
- Power button off: Application Paused, then Input Focus Lost. On: Unpaused, then Input Focus Acquired.
- Headset removed past Auto Sleep: HMD Unmounted, then Input Focus Lost and Application Paused.

Meta's guidance:
- **Paused**: stop simulation, rendering and input.
- **Focus lost**: stop rendering tracked controllers and hands, and ignore all input. Pausing
  gameplay is your call, but single-player games must pause to pass Functional.2.
- **Keep rendering the scene** while unfocused (Input.4).

```csharp
using UnityEngine;

// One per scene, or DontDestroyOnLoad on the rig.
public class QuestLifecycle : MonoBehaviour
{
    [SerializeField] GameObject[] handAndControllerVisuals; // controller models, hand meshes, rays, pointers
    [SerializeField] bool singlePlayer = true;

    public static bool InputAllowed { get; private set; } = true;
    float savedTimeScale = 1f;
    bool gameplayPaused;

    void OnEnable()
    {
        OVRManager.InputFocusLost += OnFocusLost;
        OVRManager.InputFocusAcquired += OnFocusAcquired;
    }

    void OnDisable()
    {
        OVRManager.InputFocusLost -= OnFocusLost;
        OVRManager.InputFocusAcquired -= OnFocusAcquired;
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) { SaveGame(); PauseGameplay(); }   // Functional.4: persist on pause
    }

    void OnFocusLost()
    {
        InputAllowed = false;                           // gameplay input code must check this
        foreach (var go in handAndControllerVisuals) if (go) go.SetActive(false);
        if (singlePlayer) PauseGameplay();              // the scene keeps rendering
    }

    void OnFocusAcquired()
    {
        InputAllowed = true;
        foreach (var go in handAndControllerVisuals) if (go) go.SetActive(true);
        // Leave the game paused behind your own pause menu. The user resumes explicitly.
    }

    void PauseGameplay()
    {
        if (gameplayPaused) return;
        gameplayPaused = true;
        savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;                            // ambience may keep playing (allowed)
        // Open your in-game pause menu here.
    }

    public void ResumeGameplay()                        // called from the pause menu
    {
        gameplayPaused = false;
        Time.timeScale = savedTimeScale;
    }

    void SaveGame() { /* write to Application.persistentDataPath atomically */ }
}
```

Common misses:
- Physics or animation driven by `Time.unscaledDeltaTime`, timers based on `Time.realtimeSinceStartup`,
  and networking-driven gameplay keep running after `timeScale = 0`. Gate them on your own pause flag.
- Interaction SDK interactors still receive poses while unfocused. Disable the interactor
  GameObjects too, or gate on `OVRManager.hasInputFocus`.
- Audio: pausing all audio is fine. Meta lets non-interactive ambience continue.
- Test in a **build**, not just Play mode. Focus events come from the runtime.

## Fast first frame (Performance.3) and passthrough splash (Functional.14)

- Make scene 0 a tiny boot scene with the rig and a loading indicator, then load the real scene
  with `SceneManager.LoadSceneAsync`.
- Set the system splash in **OVRCameraRig > OVR Manager > Quest tab > Quest Features > General >
  System Splash Screen** (PNG; Mono or Stereo). The Background is **Black** or
  **Passthrough (Contextual)**. When Passthrough Support ≠ None it is forced to Passthrough
  (Contextual), which is what Functional.14 needs for MR apps. Clear Unity's Player
  "virtual reality splash image" if you don't want a second splash after the system one.

## Recenter (Functional.9)

- The user recenters from the system UI. Don't fight it: re-anchor menus and world-space UI
  after a recenter.
- **verify**: `OVRManager.display.RecenteredPose` (an event on `OVRDisplay`) has been reported to
  fire on Quest recenter. Confirm with `metavr docs api-details OVRDisplay` before relying on it.
  Polling the camera rig's forward vector each frame is a fallback.
- `OVRManager.AllowRecenter` is documented as applying to PC VR.

## Controller and hands switching (Input.7) and modality-complete play (Tracking.2)

- The Interaction SDK rig (`hz-unity-meta-interaction-sdk`) swaps hand and controller
  interactors automatically. Prefer it over custom input code.
- Custom code: **verify** `OVRInput.GetActiveController()` and `OVRInput.Controller.Hands` /
  `OVRInput.Controller.Touch` with `metavr docs api-details OVRInput`.
- If the store metadata lists hand tracking, every menu, locomotion mode and gameplay action must
  work with hands only from launch.

## Entitlement (Security.1, recommended)

Use the `EntitlementGate` in `hz-unity-platform-sdk` (`references/entitlements.md`). Correction
to that skill: the VRC page (2025-04-08) makes the check **recommended**, and failure handling may
be an exit, an error message, or a limited demo mode. The 10-second budget still applies.

## Per-user data (Functional.12)

- Key save files by the platform user ID (`Oculus.Platform.Users.GetLoggedInUser()`, see
  `hz-unity-platform-sdk/references/users.md`), or rely on Android per-user app storage. Never cache
  one user's entitlement or profile for another account.

## Permissions (Security.2)

Remove a permission that a package injects:

```xml
<!-- Assets/Plugins/Android/AndroidManifest.xml -->
<manifest xmlns:android="http://schemas.android.com/apk/res/android"
          xmlns:tools="http://schemas.android.com/tools" package="com.company.game">
  <uses-permission android:name="android.permission.READ_PHONE_STATE" tools:node="remove" />
</manifest>
```

Merge this with the Meta XR-generated manifest. Don't replace it, and never hand-delete
`com.oculus.*` entries (see `hz-unity-meta-core-sdk/references/android-manifest.md`). For runtime
permissions such as RECORD_AUDIO, the game must survive the user denying the prompt.

## Internet notice (Functional.7) and language (Functional.13)

```csharp
if (Application.internetReachability == NetworkReachability.NotReachable)
    ShowWorldSpaceNotice("An internet connection is required for online play.");

SystemLanguage lang = Application.systemLanguage; // pick the matching locale, else English
```

Last verified: 2026-10-16
