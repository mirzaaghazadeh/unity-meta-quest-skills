// QuestProjectSettingsAudit.cs: audit (and optionally apply) Meta-documented Unity settings for Quest.
// Copy into <Project>/Assets/Editor/.
//   Menu:   Tools > Quest > Audit Project Settings  /  Apply Store-Critical + Recommended
//   Batch:  -executeMethod QuestProjectSettingsAudit.Audit [-auditReport Logs/quest-settings-audit.json]
//           (exit code 1 if any Required check fails)
//           -executeMethod QuestProjectSettingsAudit.ApplyRecommended
// Optional packages (XR Management, OpenXR, URP) are read via reflection, so the script compiles
// without them. A check reports UNKNOWN when a package or property isn't found.
// Rules and sources: see ../SKILL.md (verified against developers.meta.com 2026-10-16).
// This does not replace Meta's Project Setup Tool (Meta > Tools > Project Setup Tool); run both.
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

public static class QuestProjectSettingsAudit
{
    enum Status { PASS, FAIL, WARN, UNKNOWN }

    struct Finding
    {
        public string Id, Level, Actual, Expected, Source;
        public Status Status;
    }

    [MenuItem("Tools/Quest/Audit Project Settings")]
    static void AuditMenu() => Run(out _);

    [MenuItem("Tools/Quest/Apply Store-Critical + Recommended")]
    static void ApplyMenu() { ApplyRecommended(); Run(out _); }

    public static void Audit()
    {
        bool requiredOk = Run(out string json);
        string path = Arg("-auditReport") ?? "Logs/quest-settings-audit.json";
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".");
        File.WriteAllText(path, json);
        Debug.Log($"[QuestAudit] Report written to {path}");
        if (Application.isBatchMode) EditorApplication.Exit(requiredOk ? 0 : 1);
    }

    public static void ApplyRecommended()
    {
        var android = NamedBuildTarget.Android;
        PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        if ((int)PlayerSettings.Android.minSdkVersion < 32) PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)32;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
        EditorUserBuildSettings.buildAppBundle = false;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        QualitySettings.antiAliasing = 4;
        SetUrpMsaa(4);
        AssetDatabase.SaveAssets();
        Debug.Log("[QuestAudit] Applied: IL2CPP, ARM64, minSdk>=32, targetSdk 34, install Auto, APK, Vulkan-only, ASTC, MSAA 4x. " +
                  "Not changed: color space, XR loaders and OpenXR features (use Meta Project Setup Tool Fix All).");
    }

    static bool Run(out string json)
    {
        var f = new List<Finding>();
        var android = NamedBuildTarget.Android;

        // Store-critical (Required)
        var backend = PlayerSettings.GetScriptingBackend(android);
        Add(f, "android.scriptingBackend", "Required", backend == ScriptingImplementation.IL2CPP, backend.ToString(), "IL2CPP", "unity-prepare-for-publish");
        var arch = PlayerSettings.Android.targetArchitectures;
        Add(f, "android.architectures", "Required", arch == AndroidArchitecture.ARM64, arch.ToString(), "ARM64 only", "unity-prepare-for-publish; VRC.Quest.Packaging.6");
        int min = (int)PlayerSettings.Android.minSdkVersion;
        Add(f, "android.minSdk", "Required", min >= 29 && min <= 34, min.ToString(), "29-34 (32 recommended)", "publish-mobile-manifest");
        int target = (int)PlayerSettings.Android.targetSdkVersion;
        Add(f, "android.targetSdk", "Required", target == 34, target == 0 ? "Automatic (highest installed)" : target.ToString(), "34 (explicit)", "publish-mobile-manifest: apps created since 2026-03-01 must target 34");
        var install = PlayerSettings.Android.preferredInstallLocation;
        Add(f, "android.installLocation", "Required", install == AndroidPreferredInstallLocation.Auto, install.ToString(), "Auto", "publish-mobile-manifest");
        string id = PlayerSettings.GetApplicationIdentifier(android);
        bool idOk = !string.IsNullOrEmpty(id) && !id.StartsWith("com.DefaultCompany", StringComparison.OrdinalIgnoreCase) && !id.StartsWith("com.unity.", StringComparison.OrdinalIgnoreCase);
        Add(f, "android.packageName", "Required", idOk, id, "unique, permanent (com.company.game)", "unity-prepare-for-publish");
        Add(f, "build.appBundle", "Required", !EditorUserBuildSettings.buildAppBundle, EditorUserBuildSettings.buildAppBundle ? "AAB" : "APK", "APK", "VRC.Quest.Packaging.5");
        f.Add(new Finding { Id = "build.development", Level = "Required(release)", Status = EditorUserBuildSettings.development ? Status.WARN : Status.PASS,
            Actual = EditorUserBuildSettings.development.ToString(), Expected = "false for release builds", Source = "unity-conf-settings; publish-mobile-manifest (debuggable)" });

        // Recommended
        bool defaultApis = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android);
        var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
        bool vulkanOnly = !defaultApis && apis.Length == 1 && apis[0] == GraphicsDeviceType.Vulkan;
        Add(f, "graphics.api", "Recommended", vulkanOnly, defaultApis ? "Auto" : string.Join(",", apis), "Vulkan only (required for AppSW, ETFR, Depth API)", "os-vulkan-opengl; unity-asw");
        var sub = EditorUserBuildSettings.androidBuildSubtarget;
        f.Add(new Finding { Id = "textures.compression", Level = "Recommended", Actual = sub.ToString(), Expected = "ASTC",
            Status = sub == MobileTextureSubtarget.ASTC ? Status.PASS : sub == MobileTextureSubtarget.Generic ? Status.UNKNOWN : Status.WARN,
            Source = "unity-mobile-performance-intro (Generic = uses Player > Texture Compression Formats; check ASTC is first)" });
        f.Add(new Finding { Id = "color.space", Level = "Info", Actual = PlayerSettings.colorSpace.ToString(), Expected = "Linear (older Meta tech note; stylistic)",
            Status = PlayerSettings.colorSpace == ColorSpace.Linear ? Status.PASS : Status.WARN, Source = "blog/tech-note-unity-settings-for-mobile-vr (old)" });

        object rp = GraphicsSettings.defaultRenderPipeline;
        if (rp != null)
        {
            object msaa = GetProp(rp, "msaaSampleCount");
            f.Add(new Finding { Id = "urp.msaa", Level = "Recommended", Actual = msaa?.ToString() ?? "?", Expected = "4",
                Status = msaa == null ? Status.UNKNOWN : Convert.ToInt32(msaa) == 4 ? Status.PASS : Status.WARN, Source = "unity-conf-settings (URP: set MSAA 4x manually)" });
            object hdr = GetProp(rp, "supportsHDR");
            f.Add(new Finding { Id = "urp.hdr", Level = "Info", Actual = hdr?.ToString() ?? "?", Expected = "false (per Meta hz-unity-code-review skill; not on docs pages)",
                Status = hdr == null ? Status.UNKNOWN : (bool)hdr ? Status.WARN : Status.PASS, Source = "meta-quest/agentic-tools hz-unity-code-review" });
        }
        else
        {
            Add(f, "quality.msaa", "Recommended", QualitySettings.antiAliasing == 4, QualitySettings.antiAliasing.ToString(), "4", "unity-conf-settings");
        }

        // XR loader (Unity OpenXR recommended; Oculus XR deprecated)
        var loaders = ActiveAndroidLoaders();
        if (loaders == null)
            f.Add(new Finding { Id = "xr.loader", Level = "Recommended", Status = Status.UNKNOWN, Actual = "XR Management not found", Expected = "OpenXR", Source = "unity-xr-plugin" });
        else
        {
            bool openxr = loaders.Any(n => n.Contains("OpenXRLoader"));
            bool oculus = loaders.Any(n => n.Contains("OculusLoader"));
            f.Add(new Finding { Id = "xr.loader", Level = "Recommended", Actual = loaders.Count == 0 ? "none" : string.Join(",", loaders), Expected = "OpenXR (Oculus XR plugin is deprecated)",
                Status = openxr && !oculus ? Status.PASS : Status.WARN, Source = "unity-xr-plugin (2026-10-07)" });
        }
        object renderMode = OpenXRRenderMode();
        f.Add(new Finding { Id = "openxr.renderMode", Level = "Recommended", Actual = renderMode?.ToString() ?? "?", Expected = "SinglePassInstanced (multiview)",
            Status = renderMode == null ? Status.UNKNOWN : renderMode.ToString() == "SinglePassInstanced" ? Status.PASS : Status.WARN,
            Source = "unity-openxr-settings-quest (Symmetric Projection requires multiview)" });

        // Report
        var sb = new StringBuilder();
        sb.Append("{\"generated\":\"").Append(DateTime.UtcNow.ToString("o")).Append("\",\"unity\":\"").Append(Application.unityVersion).Append("\",\"findings\":[");
        bool requiredOk = true;
        for (int i = 0; i < f.Count; i++)
        {
            var x = f[i];
            if (x.Level == "Required" && x.Status == Status.FAIL) requiredOk = false;
            string line = $"[QuestAudit] {x.Status,-7} {x.Level,-17} {x.Id}: {x.Actual} (expected {x.Expected}) [{x.Source}]";
            if (x.Status == Status.FAIL) Debug.LogError(line); else if (x.Status == Status.PASS) Debug.Log(line); else Debug.LogWarning(line);
            sb.Append(i == 0 ? "" : ",").Append("{\"id\":\"").Append(Esc(x.Id)).Append("\",\"level\":\"").Append(Esc(x.Level))
              .Append("\",\"status\":\"").Append(x.Status).Append("\",\"actual\":\"").Append(Esc(x.Actual))
              .Append("\",\"expected\":\"").Append(Esc(x.Expected)).Append("\",\"source\":\"").Append(Esc(x.Source)).Append("\"}");
        }
        sb.Append("],\"requiredOk\":").Append(requiredOk ? "true" : "false").Append(",\"manualChecks\":[")
          .Append("\"Meta Project Setup Tool: Fix All + Apply All on Android and Standalone\",")
          .Append("\"OpenXR > Meta XR feature group enabled (Android + Standalone)\",")
          .Append("\"Meta Quest feature: Symmetric Projection, Optimize Buffer Discards, Late Latching (Vulkan)\",")
          .Append("\"Target Devices set only to verified headsets\",")
          .Append("\"System Splash Screen set (OVRManager > Quest Features > General)\"]}");
        json = sb.ToString();
        Debug.Log(requiredOk ? "[QuestAudit] Required checks: OK" : "[QuestAudit] Required checks: FAILED");
        return requiredOk;
    }

    static void Add(List<Finding> f, string id, string level, bool ok, string actual, string expected, string source)
    {
        f.Add(new Finding { Id = id, Level = level, Status = ok ? Status.PASS : level == "Required" ? Status.FAIL : Status.WARN, Actual = actual, Expected = expected, Source = source });
    }

    static List<string> ActiveAndroidLoaders()
    {
        try
        {
            Type perTarget = FindType("UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget");
            if (perTarget == null) return null;
            MethodInfo forTarget = perTarget.GetMethod("XRGeneralSettingsForBuildTarget", BindingFlags.Public | BindingFlags.Static);
            object general = forTarget?.Invoke(null, new object[] { BuildTargetGroup.Android });
            if (general == null) return new List<string>();
            object manager = GetProp(general, "Manager") ?? GetProp(general, "AssignedSettings");
            if (manager == null) return new List<string>();
            var list = (GetProp(manager, "activeLoaders") ?? GetProp(manager, "loaders")) as IEnumerable;
            var names = new List<string>();
            if (list != null) foreach (object l in list) if (l != null) names.Add(l.GetType().FullName);
            return names;
        }
        catch (Exception e) { Debug.LogWarning("[QuestAudit] XR loader check failed: " + e.Message); return null; }
    }

    static object OpenXRRenderMode()
    {
        try
        {
            Type settings = FindType("UnityEngine.XR.OpenXR.OpenXRSettings");
            MethodInfo get = settings?.GetMethod("GetSettingsForBuildTargetGroup", BindingFlags.Public | BindingFlags.Static);
            object s = get?.Invoke(null, new object[] { BuildTargetGroup.Android });
            return s == null ? null : GetProp(s, "renderMode");
        }
        catch { return null; }
    }

    static void SetUrpMsaa(int samples)
    {
        object rp = GraphicsSettings.defaultRenderPipeline;
        PropertyInfo p = rp?.GetType().GetProperty("msaaSampleCount", BindingFlags.Public | BindingFlags.Instance);
        if (p != null && p.CanWrite)
        {
            p.SetValue(rp, samples);
            EditorUtility.SetDirty((UnityEngine.Object)rp);
        }
    }

    static object GetProp(object o, string name)
    {
        PropertyInfo p = o.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        return p?.GetValue(o);
    }

    static Type FindType(string fullName)
    {
        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = a.GetType(fullName, false);
            if (t != null) return t;
        }
        return null;
    }

    static string Arg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
        return null;
    }

    static string Esc(string s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
}
#endif
