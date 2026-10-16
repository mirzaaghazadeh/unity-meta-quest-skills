// QuestPreflight.cs: read-only report of a project's Quest/Android build settings, for a live Editor.
//
// Put it OUTSIDE Assets/ (e.g. <project>/AgentScripts/QuestPreflight.cs) so writing it triggers no
// import or domain reload, then run:
//   unity command run_script --project-path <project> --file AgentScripts/QuestPreflight.cs \
//     --entry QuestPreflight.Run --format json
// It changes nothing. It reads the global Android Player settings, so when a build profile with
// Player Settings Overrides is active, treat the report as a hint and verify the built APK instead.
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEngine;

public static class QuestPreflight
{
    public static string Run()
    {
        var sb = new StringBuilder();
        void Line(string key, object value) => sb.Append(key).Append(": ").Append(value).Append('\n');

        Line("unityVersion", Application.unityVersion);
        Line("activeBuildTarget", EditorUserBuildSettings.activeBuildTarget);
        var profile = BuildProfile.GetActiveBuildProfile();
        Line("activeBuildProfile", profile != null ? AssetDatabase.GetAssetPath(profile) : "(none: classic platform)");

        var android = NamedBuildTarget.Android;
        Line("applicationId", PlayerSettings.GetApplicationIdentifier(android));
        Line("versionName", PlayerSettings.bundleVersion);
        Line("versionCode", PlayerSettings.Android.bundleVersionCode);
        Line("scriptingBackend", PlayerSettings.GetScriptingBackend(android));
        Line("targetArchitectures", PlayerSettings.Android.targetArchitectures);
        Line("minSdkVersion", PlayerSettings.Android.minSdkVersion);
        Line("targetSdkVersion", PlayerSettings.Android.targetSdkVersion);
        Line("installLocation", PlayerSettings.Android.preferredInstallLocation);
        Line("graphicsApis", string.Join(",", PlayerSettings.GetGraphicsAPIs(BuildTarget.Android)));
        Line("colorSpace", PlayerSettings.colorSpace);
        Line("textureSubtarget", EditorUserBuildSettings.androidBuildSubtarget);
        Line("textureCompressionFormats", string.Join(",", PlayerSettings.Android.textureCompressionFormats));
        Line("buildAppBundle", EditorUserBuildSettings.buildAppBundle);
        Line("useCustomKeystore", PlayerSettings.Android.useCustomKeystore);
        Line("keystoreName", string.IsNullOrEmpty(PlayerSettings.Android.keystoreName) ? "(none)" : PlayerSettings.Android.keystoreName);
        Line("developmentBuild", EditorUserBuildSettings.development);

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        Line("enabledScenes", scenes.Length == 0 ? "(none)" : string.Join(", ", scenes));
        return sb.ToString();
    }
}
