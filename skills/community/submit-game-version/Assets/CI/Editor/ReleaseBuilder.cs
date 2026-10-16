using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ArcadeHoops.CI
{
    /// <summary>
    /// Release-build entry point for ArcadeHoopsMR, driven over Unity MCP while the
    /// editor is OPEN. Bumps the version, injects keystore secrets (not serialized in
    /// the project), builds a signed release APK, and writes a parseable result the
    /// upload step consumes.
    ///
    /// <see cref="BuildAndroidReleaseInEditor"/> runs the full signed build but never
    /// calls EditorApplication.Exit, so the open editor survives. The "submit-game-version"
    /// skill invokes it via RunCommand, then runs CI/upload_and_record.sh for the upload.
    ///
    /// The result (version/code/apk) is logged and written to
    /// Builds/last_release_result.txt for the shell to read.
    /// </summary>
    public static class ReleaseBuilder
    {
        // Keystore path + alias already live in ProjectSettings; only the passwords are
        // secret. The open editor has no env vars set, so they are read from
        // CI/release.local.env (with an environment override if one happens to be set).
        private const string KeystorePassEnv = "KEYSTORE_PASS";
        private const string KeyaliasPassEnv = "KEYALIAS_PASS";

        private const string OutputDir = "Builds";
        private const string ResultFileName = "last_release_result.txt";
        private const string EnvFileRelativePath = "CI/release.local.env";

        private struct BuildOutcome
        {
            public bool Success;
            public string Version;
            public int Code;
            public string ApkPath;
            public string Error;
        }

        /// <summary>
        /// In-editor entry driven over Unity MCP. Runs the signed release build without
        /// ever calling EditorApplication.Exit, so the open editor survives. The version,
        /// code and APK path are logged and written to Builds/last_release_result.txt for
        /// CI/upload_and_record.sh to pick up.
        /// </summary>
        public static void BuildAndroidReleaseInEditor()
        {
            try
            {
                BuildOutcome outcome = BuildCore();
                if (!outcome.Success)
                {
                    Debug.LogError($"[ReleaseBuilder] RELEASE_BUILD_FAILED {outcome.Error}");
                    return;
                }

                Debug.Log(
                    $"RELEASE_BUILD_RESULT version={outcome.Version} code={outcome.Code} apk={outcome.ApkPath}");
                Debug.Log(
                    $"[ReleaseBuilder] SUCCESS {outcome.Version} (code {outcome.Code}) — editor left open.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ReleaseBuilder] RELEASE_BUILD_FAILED Unhandled exception: {e}");
            }
        }

        /// <summary>
        /// Bumps the version, signs, configures release, builds the APK, and (on success)
        /// writes the result file. Does NOT exit the editor.
        /// </summary>
        private static BuildOutcome BuildCore()
        {
            var outcome = new BuildOutcome();

            // --- Keystore passwords: CI/release.local.env (env-var override allowed) ---
            string keystorePass, keyaliasPass, passError;
            if (!TryLoadKeystorePasswords(out keystorePass, out keyaliasPass, out passError))
            {
                outcome.Error = passError;
                return outcome;
            }

            // --- Bump version: beta-1.0.3 -> beta-1.0.4, code 9 -> 10 ---
            string newVersion = BumpPatch(PlayerSettings.bundleVersion);
            int newCode = PlayerSettings.Android.bundleVersionCode + 1;

            PlayerSettings.bundleVersion = newVersion;
            PlayerSettings.Android.bundleVersionCode = newCode;

            // --- Signing (path + alias from project, passwords from the env file) ---
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasPass = keyaliasPass;

            // --- Release configuration ---
            EnsureAndroidTarget();
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.buildAppBundle = false; // APK for ovr-platform-util

            AssetDatabase.SaveAssets();

            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                outcome.Error = "No enabled scenes in Build Settings; nothing to build.";
                return outcome;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outDir = Path.Combine(projectRoot, OutputDir);
            Directory.CreateDirectory(outDir);

            string apkPath = Path.Combine(
                outDir, $"ArcadeHoopsMR-{newVersion}-{newCode}.apk");

            Debug.Log($"[ReleaseBuilder] Building {newVersion} (code {newCode}) -> {apkPath}");

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = apkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                outcome.Error = $"Build {summary.result} with {summary.totalErrors} error(s).";
                return outcome;
            }

            // Persist the bumped version into ProjectSettings.asset.
            AssetDatabase.SaveAssets();

            WriteResultFile(outDir, newVersion, newCode, apkPath);

            outcome.Success = true;
            outcome.Version = newVersion;
            outcome.Code = newCode;
            outcome.ApkPath = apkPath;
            return outcome;
        }

        /// <summary>
        /// Loads the keystore + key-alias passwords from CI/release.local.env. An
        /// environment variable, if set, takes precedence over the file.
        /// </summary>
        private static bool TryLoadKeystorePasswords(
            out string keystorePass, out string keyaliasPass, out string error)
        {
            keystorePass = Environment.GetEnvironmentVariable(KeystorePassEnv);
            keyaliasPass = Environment.GetEnvironmentVariable(KeyaliasPassEnv);

            if (!string.IsNullOrEmpty(keystorePass) && !string.IsNullOrEmpty(keyaliasPass))
            {
                error = null;
                return true;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string envPath = Path.Combine(projectRoot, EnvFileRelativePath);

            if (File.Exists(envPath))
            {
                foreach (string raw in File.ReadAllLines(envPath))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;

                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;

                    string key = line.Substring(0, eq).Trim();
                    string val = Unquote(line.Substring(eq + 1).Trim());

                    if (key == KeystorePassEnv && string.IsNullOrEmpty(keystorePass)) keystorePass = val;
                    else if (key == KeyaliasPassEnv && string.IsNullOrEmpty(keyaliasPass)) keyaliasPass = val;
                }
            }

            if (string.IsNullOrEmpty(keystorePass) || string.IsNullOrEmpty(keyaliasPass))
            {
                error =
                    $"{KeystorePassEnv}/{KeyaliasPassEnv} not found in {EnvFileRelativePath}. " +
                    "Copy CI/release.local.env.example to CI/release.local.env and fill in the passwords.";
                return false;
            }

            error = null;
            return true;
        }

        private static string Unquote(string value)
        {
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[value.Length - 1] == '"') ||
                 (value[0] == '\'' && value[value.Length - 1] == '\'')))
            {
                return value.Substring(1, value.Length - 2);
            }
            return value;
        }

        /// <summary>Writes version/code/apk to Builds/last_release_result.txt for the shell.</summary>
        private static void WriteResultFile(string outDir, string version, int code, string apkPath)
        {
            string resultPath = Path.Combine(outDir, ResultFileName);
            File.WriteAllText(resultPath, $"version={version}\ncode={code}\napk={apkPath}\n");
        }

        /// <summary>Increments the numeric segment after the final '.' in the version.</summary>
        private static string BumpPatch(string version)
        {
            if (string.IsNullOrEmpty(version))
                throw new InvalidOperationException("bundleVersion is empty.");

            int dot = version.LastIndexOf('.');
            if (dot < 0 || dot == version.Length - 1)
                throw new InvalidOperationException(
                    $"Cannot parse a trailing patch number from '{version}'. " +
                    "Expected something like 'beta-1.0.2'.");

            string prefix = version.Substring(0, dot + 1);
            string patchStr = version.Substring(dot + 1);

            if (!int.TryParse(patchStr, out int patch))
                throw new InvalidOperationException(
                    $"Patch segment '{patchStr}' in '{version}' is not numeric.");

            return prefix + (patch + 1);
        }

        private static void EnsureAndroidTarget()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log("[ReleaseBuilder] Switching active build target to Android.");
                EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Android, BuildTarget.Android);
            }
        }
    }
}
