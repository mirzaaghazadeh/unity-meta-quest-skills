// Headless review images for a Quest scene, no headset needed. Copy into Assets/Editor/.
//   Tools > Capture > Viewpoints  (or -executeMethod QuestBlockout.SceneCapture.Viewpoints)
//   Tools > Capture > Quest 3 Eyes (or -executeMethod QuestBlockout.SceneCapture.Eyes)
// Reads Assets/Capture/capture.json, or the path after -captureConfig on the command line.
// Editor rendering on the development machine: right for layout and look, never for performance.
// Part of unity-meta-quest-skills (MIT), skill quest-level-blockout.
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuestBlockout
{
    public static class SceneCapture
    {
        const string DefaultConfig = "Assets/Capture/capture.json";

        [Serializable]
        public class Config
        {
            public string scene;
            public string outputDir = "Screenshots";
            public int width = 1920, height = 1080;
            public View[] viewpoints;
            public EyeView eyes;
        }

        [Serializable]
        public class View
        {
            public string name;
            public float[] pos, lookAt;
            public float fov = 75f;
            public float orthoSize;       // > 0 renders orthographic (top-down maps)
            public bool noFog;            // layout diagrams read better without fog
            public bool hideEditorOnly;   // hide debug labels and markers
        }

        // Defaults: Quest 3 per-eye resolution 2064 x 2208 (Meta device-optimization-comparison,
        // 2026-05-11). The field-of-view angles and IPD are APPROXIMATIONS (unverified): read the real
        // per-eye angles from the runtime (OpenXR xrLocateViews) if you need them exact.
        [Serializable]
        public class EyeView
        {
            public string name = "player_view";
            public float[] pos;           // a point on the floor where the player stands
            public float yaw, pitch = 5f; // pitch > 0 looks slightly down
            public float eyeHeight = 1.65f, ipd = 0.063f;
            public float fovTowardsNose = 44f, fovAwayFromNose = 52f, fovUp = 46f, fovDown = 50f;
            public int width = 2064, height = 2208;
        }

        [MenuItem("Tools/Capture/Viewpoints")]
        public static void Viewpoints()
        {
            var config = Load();
            var scene = EditorSceneManager.OpenScene(config.scene);
            Prepare(scene);
            var camera = CreateCamera(config.width, config.height, out var target);
            Directory.CreateDirectory(config.outputDir);
            var editorOnly = scene.GetRootGameObjects().Where(go => go.CompareTag("EditorOnly")).ToArray();

            foreach (var view in config.viewpoints)
            {
                var position = V(view.pos);
                camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(V(view.lookAt) - position));
                camera.orthographic = view.orthoSize > 0;
                camera.orthographicSize = view.orthoSize;
                camera.fieldOfView = view.fov > 0 ? view.fov : 60f;
                foreach (var go in editorOnly)
                    go.SetActive(!view.hideEditorOnly);

                var fog = RenderSettings.fog;
                RenderSettings.fog = fog && !view.noFog;
                var path = $"{config.outputDir}/{view.name}.png";
                File.WriteAllBytes(path, Render(camera, target).EncodeToPNG());
                RenderSettings.fog = fog;
                Debug.Log($"[SceneCapture] {path}");
            }
            Cleanup(camera, target);
            RestoreScene(config);
        }

        // One image per eye, as the headset would draw them: asymmetric per-eye frustums, eyes apart
        // by the IPD, editor-only objects hidden (as in a build). Writes <name>_stereo.png and
        // <name>_left_eye.png. Not reproduced: lens blur, foveated rendering, the device GPU.
        [MenuItem("Tools/Capture/Quest 3 Eyes")]
        public static void Eyes()
        {
            var config = Load();
            var e = config.eyes ?? throw new Exception("capture config has no \"eyes\" block");
            var scene = EditorSceneManager.OpenScene(config.scene);
            Prepare(scene);
            foreach (var go in scene.GetRootGameObjects().Where(go => go.CompareTag("EditorOnly")))
                go.SetActive(false);

            var head = new GameObject("Capture Head").transform;
            head.SetPositionAndRotation(V(e.pos) + Vector3.up * e.eyeHeight, Quaternion.Euler(e.pitch, e.yaw, 0f));
            var camera = CreateCamera(e.width, e.height, out var target);
            camera.transform.SetParent(head, false);

            const float near = 0.05f, far = 1500f;
            float Edge(float degrees) => Mathf.Tan(degrees * Mathf.Deg2Rad) * near;
            var stereo = new Texture2D(e.width * 2, e.height, TextureFormat.RGB24, false);
            foreach (var side in new[] { -1, 1 }) // left eye, then right eye
            {
                camera.transform.localPosition = new Vector3(side * e.ipd / 2, 0f, 0f);
                var left = side < 0 ? -e.fovAwayFromNose : -e.fovTowardsNose;
                var right = side < 0 ? e.fovTowardsNose : e.fovAwayFromNose;
                camera.projectionMatrix = Matrix4x4.Frustum(Edge(left), Edge(right), Edge(-e.fovDown), Edge(e.fovUp), near, far);
                var eye = Render(camera, target);
                stereo.SetPixels(side < 0 ? 0 : e.width, 0, e.width, e.height, eye.GetPixels());
                if (side < 0)
                    File.WriteAllBytes($"{config.outputDir}/{e.name}_left_eye.png", eye.EncodeToPNG());
                Object.DestroyImmediate(eye);
            }
            stereo.Apply();
            Directory.CreateDirectory(config.outputDir);
            File.WriteAllBytes($"{config.outputDir}/{e.name}_stereo.png", stereo.EncodeToPNG());
            Debug.Log($"[SceneCapture] {config.outputDir}/{e.name}_stereo.png ({e.width} x {e.height} per eye)");
            Object.DestroyImmediate(stereo);
            Object.DestroyImmediate(head.gameObject);
            Cleanup(null, target);
            RestoreScene(config);
        }

        // Captures hide editor-only objects. In a live Editor (unity command menu …) that would leave
        // the open scene modified, so reload it from disk and drop the changes.
        static void RestoreScene(Config config) => EditorSceneManager.OpenScene(config.scene, OpenSceneMode.Single);

        static Config Load()
        {
            var args = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(args, "-captureConfig");
            var path = index >= 0 && index + 1 < args.Length ? args[index + 1] : DefaultConfig;
            var config = JsonUtility.FromJson<Config>(File.ReadAllText(path));
            Directory.CreateDirectory(config.outputDir);
            return config;
        }

        // TextMesh Pro builds its meshes on an Editor update, which batch mode doesn't tick.
        static void Prepare(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (var text in scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<TMP_Text>(true)))
                text.ForceMeshUpdate();
        }

        static Camera CreateCamera(int width, int height, out RenderTexture target)
        {
            var camera = new GameObject("Capture Camera").AddComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 1500f;
            camera.clearFlags = CameraClearFlags.Skybox;
            target = new RenderTexture(width, height, 24) { antiAliasing = 4 }; // MSAA 4x, LDR, like Quest
            camera.targetTexture = target;
            return camera;
        }

        // The first render after opening a scene can use stale per-material data: render once, discard.
        static Texture2D Render(Camera camera, RenderTexture target)
        {
            camera.Render();
            camera.Render();
            RenderTexture.active = target;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
            RenderTexture.active = null;
            return image;
        }

        static void Cleanup(Camera camera, RenderTexture target)
        {
            if (camera != null)
                camera.targetTexture = null;
            Object.DestroyImmediate(target);
            if (camera != null)
                Object.DestroyImmediate(camera.gameObject);
        }

        static Vector3 V(float[] a) => new(a[0], a[1], a[2]);
    }
}
