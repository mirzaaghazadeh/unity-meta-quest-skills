// App-specific Meta XR Operator tools: let an agent read game state and jump to a test state.
// Add to a GameObject in your test scene. Needs Meta XR Core SDK 207+ with the Unity OpenXR Plugin
// (MetaXROperatorExternalTool only exists when Core is built for OpenXR).
// Compiled only in the Editor and Development builds, matching the Operator layer itself.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using Meta.XR;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AgentTools : MonoBehaviour
{
#pragma warning disable CS0649 // filled in by JsonUtility
    [Serializable]
    struct LoadSceneArgs
    {
        public string scene;
    }
#pragma warning restore CS0649

    // Register in Start, not Awake: the XR session must exist. Each name can be registered once.
    void Start()
    {
        var state = MetaXROperatorExternalTool.RegisterAgenticTool(
            "MyGame_GetState",
            "Returns JSON with the active scene name, time since start and paused state.",
            null,
            GetState);

        var load = MetaXROperatorExternalTool.RegisterAgenticTool(
            "MyGame_LoadScene",
            "Loads a scene that is in the build list. Argument: scene (exact scene name).",
            new[]
            {
                new AgenticToolParameter
                {
                    Name = "scene",
                    Description = "Exact name of a scene in the build list, for example Level01.",
                    ParamType = XrAgenticExternalToolParameterTypeMETAX1.String,
                    IsRequired = true,
                },
            },
            LoadScene);

        if (state != XrResult.Success || load != XrResult.Success)
        {
            Debug.LogWarning($"[AgentTools] Registration: GetState={state}, LoadScene={load}");
        }
    }

    // Callbacks run on the main thread by default: keep them short.
    static string GetState(string parameters)
    {
        return JsonUtility.ToJson(new GameState
        {
            scene = SceneManager.GetActiveScene().name,
            time = Time.timeSinceLevelLoad,
            paused = Time.timeScale == 0f,
        });
    }

    // Arguments come from an agent: validate them and never act on anything outside the build list.
    static string LoadScene(string parameters)
    {
        LoadSceneArgs args;
        try
        {
            args = JsonUtility.FromJson<LoadSceneArgs>(parameters);
        }
        catch (ArgumentException)
        {
            return "{\"ok\":false,\"error\":\"invalid JSON\"}";
        }

        if (string.IsNullOrEmpty(args.scene) || !Application.CanStreamedLevelBeLoaded(args.scene))
        {
            return "{\"ok\":false,\"error\":\"unknown scene\"}";
        }

        SceneManager.LoadScene(args.scene);
        return "{\"ok\":true}";
    }

    [Serializable]
    struct GameState
    {
        public string scene;
        public float time;
        public bool paused;
    }
}
#endif
