// QuestReleaseBuild.cs: headless signed release APK for the Meta Horizon Store.
// Copy into <Project>/Assets/Editor/. Entry point: -executeMethod QuestReleaseBuild.Build
//
// Inputs (environment variables, or -args where noted):
//   -buildOutput <path> | QUEST_BUILD_OUTPUT   APK path (default Builds/Quest/<product>-<code>.apk)
//   -versionCode <int>  | QUEST_VERSION_CODE   Bundle Version Code; must exceed every uploaded build
//   -versionName <str>  | QUEST_VERSION_NAME   optional display version
//   QUEST_KEYSTORE_PATH, QUEST_KEYSTORE_PASS, QUEST_KEY_ALIAS, QUEST_KEY_ALIAS_PASS (required)
//   QUEST_SPLIT_OBB=1                          optional: split into APK + main OBB (APK must be < 1 GB)
//   QUEST_ALLOW_DEBUG_SIGNING=1                optional: local smoke builds only, never upload these
//
// Store-critical values follow developers.meta.com publish-mobile-manifest and
// unity-prepare-for-publish (verified 2026-10-16). Re-verify with `metavr docs search` if Meta
// changes the target SDK rule. Unity does not write keystore passwords to disk, but the other
// PlayerSettings changes below are saved to ProjectSettings.asset. Review them before committing.
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class QuestReleaseBuild
{
    const int MinSdk = 32;    // Meta recommended value for in-lifecycle Quest devices
    const int TargetSdk = 34; // required for apps created since 2026-03-01; immersive range is 32-34

    public static void Build()
    {
        try
        {
            int code = RunBuild();
            EditorApplication.Exit(code);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    static int RunBuild()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        // Version code
        string codeText = Arg("-versionCode") ?? Env("QUEST_VERSION_CODE");
        if (!string.IsNullOrEmpty(codeText))
        {
            if (!int.TryParse(codeText, out int versionCode) || versionCode < 1)
                return Fail($"Invalid version code '{codeText}'");
            PlayerSettings.Android.bundleVersionCode = versionCode;
        }
        else
        {
            Debug.LogWarning($"[QuestReleaseBuild] No version code given; using existing {PlayerSettings.Android.bundleVersionCode}. It must exceed every uploaded build.");
        }
        string versionName = Arg("-versionName") ?? Env("QUEST_VERSION_NAME");
        if (!string.IsNullOrEmpty(versionName))
            PlayerSettings.bundleVersion = versionName;

        // Release packaging requirements
        EditorUserBuildSettings.development = false;          // android:debuggable must be false/unset
        EditorUserBuildSettings.buildAppBundle = false;       // Horizon Store takes APK (+OBB), not AAB
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64; // 64-bit only
        PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)MinSdk;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)TargetSdk; // never "Automatic"
        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
        PlayerSettings.Android.splitApplicationBinary = Env("QUEST_SPLIT_OBB") == "1";

        // Signing
        string keystore = Env("QUEST_KEYSTORE_PATH");
        if (!string.IsNullOrEmpty(keystore))
        {
            if (!File.Exists(keystore))
                return Fail($"Keystore not found: {keystore}");
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = Path.GetFullPath(keystore);
            PlayerSettings.Android.keystorePass = Env("QUEST_KEYSTORE_PASS") ?? "";
            PlayerSettings.Android.keyaliasName = Env("QUEST_KEY_ALIAS") ?? "";
            PlayerSettings.Android.keyaliasPass = Env("QUEST_KEY_ALIAS_PASS") ?? "";
            if (PlayerSettings.Android.keyaliasName.Length == 0)
                return Fail("QUEST_KEY_ALIAS is empty");
        }
        else if (Env("QUEST_ALLOW_DEBUG_SIGNING") == "1")
        {
            PlayerSettings.Android.useCustomKeystore = false;
            Debug.LogWarning("[QuestReleaseBuild] Debug-signed build. Do NOT upload it to the Horizon Store.");
        }
        else
        {
            return Fail("QUEST_KEYSTORE_PATH is not set. Release builds must be signed with the app's permanent keystore.");
        }

        // Scenes
        string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0)
            return Fail("No enabled scenes in Build Settings");

        string output = Arg("-buildOutput") ?? Env("QUEST_BUILD_OUTPUT")
            ?? $"Builds/Quest/{Sanitize(PlayerSettings.productName)}-{PlayerSettings.Android.bundleVersionCode}.apk";
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)) ?? ".");

        Debug.Log($"[QuestReleaseBuild] {PlayerSettings.applicationIdentifier} versionCode={PlayerSettings.Android.bundleVersionCode} " +
                  $"versionName={PlayerSettings.bundleVersion} minSdk={MinSdk} targetSdk={TargetSdk} -> {output}");

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = output,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log($"[QuestReleaseBuild] Result={summary.result} size={summary.totalSize} bytes time={summary.totalTime} errors={summary.totalErrors}");
        if (summary.result != BuildResult.Succeeded)
            return Fail("Build did not succeed");

        long apkBytes = new FileInfo(output).Length;
        if (apkBytes >= 1024L * 1024 * 1024)
            Debug.LogWarning($"[QuestReleaseBuild] APK is {apkBytes} bytes. VRC.Quest.Packaging.5 requires < 1 GB; set QUEST_SPLIT_OBB=1 or move content to asset files.");
        return 0;
    }

    static int Fail(string message)
    {
        Debug.LogError("[QuestReleaseBuild] " + message);
        return 1;
    }

    static string Env(string name)
    {
        string value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrEmpty(value) ? null : value;
    }

    static string Arg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        return null;
    }

    static string Sanitize(string s) => new string(s.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
}
#endif
