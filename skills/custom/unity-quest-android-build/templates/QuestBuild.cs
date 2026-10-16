// QuestBuild.cs: headless Meta Quest (Android) build entry points for the unity CLI.
//
// Copy to an Editor folder, e.g. Assets/Editor/QuestBuild.cs (-executeMethod only finds Editor scripts).
// Needs Unity 6000.0+ (BuildProfile API). The Meta Quest build profile itself needs Unity 6000.1+.
//
//   One-time, then commit ProjectSettings/:
//     unity run <project> -- -executeMethod QuestBuild.ApplyQuestSettings
//   Build:
//     unity build <project> --target Android --execute-method QuestBuild.BuildRelease \
//       --output-path Builds/Quest/MyGame.apk --format json
//
// Configuration comes from environment variables, which keeps secrets out of argv and shell history:
//   QUEST_BUILD_PROFILE   optional, project-relative .asset path of a Meta Quest build profile
//   QUEST_VERSION_CODE    optional, integer; must be higher than every build already uploaded
//   QUEST_VERSION_NAME    optional, e.g. 1.2.0
//   QUEST_KEYSTORE_PATH, QUEST_KEYSTORE_PASS, QUEST_KEY_ALIAS, QUEST_KEY_ALIAS_PASS
//                         required for BuildRelease (the Horizon Store rejects debug-signed APKs)
//   QUEST_SKIP_VALIDATION set to 1 to build even when the settings check fails
// The output path comes from -buildOutput (unity build passes --output-path that way), then
// -questOutput, then Builds/Quest/<productName>.apk.
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

public static class QuestBuild
{
    public static void BuildRelease() => Build(development: false);

    public static void BuildDevelopment() => Build(development: true);

    // Writes the Horizon Store baseline into the global Android Player settings and saves them.
    // Run once (or after a settings regression), review the ProjectSettings diff, commit.
    // If the project uses a Meta Quest build profile with Player Settings Overrides, set the same
    // values on that profile in File > Build Profiles: these calls change the shared Android settings.
    public static void ApplyQuestSettings()
    {
        var android = NamedBuildTarget.Android;
        PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34;
        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
        PlayerSettings.Android.textureCompressionFormats = new[] { TextureCompressionFormat.ASTC };
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        // Vulkan only, matching Unity's Meta Quest profile default. Add GraphicsDeviceType.OpenGLES3
        // after Vulkan if you need a fallback.
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        PlayerSettings.colorSpace = ColorSpace.Linear;
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        EditorUserBuildSettings.buildAppBundle = false;
        AssetDatabase.SaveAssets();
        Debug.Log("[QuestBuild] Applied Quest Android settings. Review and commit ProjectSettings/.");
    }

    // Read-only check of the global Android Player settings against the Horizon Store baseline.
    public static string[] FindProblems()
    {
        var problems = new List<string>();
        if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP)
            problems.Add("Scripting backend must be IL2CPP.");
        if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            problems.Add("Target architectures must be ARM64 only (is " + PlayerSettings.Android.targetArchitectures + ").");
        if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel29)
            problems.Add("minSdkVersion must be at least 29; 32 is recommended (is " + PlayerSettings.Android.minSdkVersion + ").");
        if (PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevel34)
            problems.Add("targetSdkVersion should be set explicitly to 34; apps created after 2026-03-01 must use 34 (is " + PlayerSettings.Android.targetSdkVersion + ").");
        if (EditorUserBuildSettings.buildAppBundle)
            problems.Add("Build App Bundle (AAB) is on; the Horizon Store takes APKs.");
        var subtarget = EditorUserBuildSettings.androidBuildSubtarget;
        var formats = PlayerSettings.Android.textureCompressionFormats;
        var astc = subtarget == MobileTextureSubtarget.ASTC
            || (subtarget == MobileTextureSubtarget.Generic && formats.Length > 0 && formats[0] == TextureCompressionFormat.ASTC);
        if (!astc)
            problems.Add("Texture compression should be ASTC (subtarget " + subtarget + ", formats " + string.Join(",", formats) + ").");
        if (PlayerSettings.colorSpace != ColorSpace.Linear)
            problems.Add("Color space should be Linear.");
        return problems.ToArray();
    }

    static void Build(bool development)
    {
        try
        {
            var output = Arg("-buildOutput") ?? Arg("-questOutput")
                ?? Path.Combine("Builds", "Quest", PlayerSettings.productName + ".apk");
            var profilePath = Env("QUEST_BUILD_PROFILE");

            if (string.IsNullOrEmpty(profilePath))
            {
                var problems = FindProblems();
                foreach (var problem in problems)
                    Debug.LogError("[QuestBuild] " + problem);
                if (problems.Length > 0 && Env("QUEST_SKIP_VALIDATION") != "1")
                    throw new Exception(problems.Length + " Quest setting problem(s). Run QuestBuild.ApplyQuestSettings or set QUEST_SKIP_VALIDATION=1.");
            }
            else
            {
                // A profile can override Player settings, which the static PlayerSettings API does not
                // reflect, so check the finished APK instead (see the skill's "Verify the APK" step).
                Debug.Log("[QuestBuild] Building with profile " + profilePath + "; settings check skipped, verify the APK.");
            }

            var versionCode = Env("QUEST_VERSION_CODE");
            if (!string.IsNullOrEmpty(versionCode))
                PlayerSettings.Android.bundleVersionCode = int.Parse(versionCode);
            var versionName = Env("QUEST_VERSION_NAME");
            if (!string.IsNullOrEmpty(versionName))
                PlayerSettings.bundleVersion = versionName;

            ApplySigning(development);
            EditorUserBuildSettings.buildAppBundle = false;

            var fullOutput = Path.GetFullPath(output);
            Directory.CreateDirectory(Path.GetDirectoryName(fullOutput));
            var options = development ? BuildOptions.Development : BuildOptions.None;

            BuildReport report;
            if (!string.IsNullOrEmpty(profilePath))
            {
                var profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(profilePath);
                if (profile == null)
                    throw new Exception("Build profile not found at " + profilePath + " (path must be project-relative).");
                report = BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions
                {
                    buildProfile = profile,
                    locationPathName = fullOutput,
                    options = options,
                });
            }
            else
            {
                var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
                if (scenes.Length == 0)
                    throw new Exception("No enabled scenes in Build Settings.");
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = fullOutput,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    options = options,
                });
            }

            var summary = report.summary;
            Debug.Log($"[QuestBuild] result={summary.result} output={summary.outputPath} bytes={summary.totalSize} " +
                      $"errors={summary.totalErrors} warnings={summary.totalWarnings} time={summary.totalTime}");
            if (summary.result != BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }
        catch (Exception e)
        {
            Debug.LogError("[QuestBuild] " + e);
            EditorApplication.Exit(1);
        }
    }

    // Unity keeps keystore and alias names in ProjectSettings. Passwords are set here for this session only.
    static void ApplySigning(bool development)
    {
        var keystore = Env("QUEST_KEYSTORE_PATH");
        if (string.IsNullOrEmpty(keystore))
        {
            if (!development)
                throw new Exception("BuildRelease needs QUEST_KEYSTORE_PATH, QUEST_KEYSTORE_PASS, QUEST_KEY_ALIAS and QUEST_KEY_ALIAS_PASS.");
            Debug.Log("[QuestBuild] No QUEST_KEYSTORE_PATH; using the project's current signing settings (debug key if none).");
            return;
        }
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = Path.GetFullPath(keystore);
        PlayerSettings.Android.keystorePass = RequireEnv("QUEST_KEYSTORE_PASS");
        PlayerSettings.Android.keyaliasName = RequireEnv("QUEST_KEY_ALIAS");
        PlayerSettings.Android.keyaliasPass = RequireEnv("QUEST_KEY_ALIAS_PASS");
    }

    static string Arg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++)
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        return null;
    }

    static string Env(string name) => Environment.GetEnvironmentVariable(name);

    static string RequireEnv(string name)
    {
        var value = Env(name);
        if (string.IsNullOrEmpty(value))
            throw new Exception("Missing environment variable " + name + ".");
        return value;
    }
}
#endif
