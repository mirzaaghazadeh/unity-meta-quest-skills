// Meta VR Glasses: report or apply the OVRProjectConfig input settings (Editor only).
// Copy to Assets/Editor/, or run from AgentScripts/ with `unity command run_script`.
// Menu: Tools > Quest > VR Glasses. Batch: -executeMethod VRGlassesSettings.Report
// Written against Meta XR Core SDK 207.0.1 (OVRProjectConfig public fields).
using UnityEditor;
using UnityEngine;

public static class VRGlassesSettings
{
    [MenuItem("Tools/Quest/VR Glasses/Report Settings")]
    public static void Report()
    {
        var config = OVRProjectConfig.CachedProjectConfig;
        if (config == null)
        {
            Debug.LogError("[VRGlasses] OVRProjectConfig not found. Is Meta XR Core SDK installed?");
            return;
        }

        Debug.Log("[VRGlasses] handTrackingSupport=" + config.handTrackingSupport
            + " eyeTrackingSupport=" + config.eyeTrackingSupport
            + " experimentalFeaturesEnabled=" + config.experimentalFeaturesEnabled
            + " disableBackups=" + config.disableBackups
            + " targetDeviceTypes=" + string.Join(",", config.targetDeviceTypes));
    }

    // Keeps controllers working and adds hands; eye tracking optional so the ray fallback stays.
    // Apply only once every required flow works with hands: the manifest entry doesn't add hand input.
    [MenuItem("Tools/Quest/VR Glasses/Apply Controllers And Hands + Eye Tracking Supported")]
    public static void ApplyControllersAndHands()
    {
        Apply(OVRProjectConfig.HandTrackingSupport.ControllersAndHands);
    }

    // Hands-only apps: the manifest declares hand tracking as required.
    public static void ApplyHandsOnly()
    {
        Apply(OVRProjectConfig.HandTrackingSupport.HandsOnly);
    }

    static void Apply(OVRProjectConfig.HandTrackingSupport handTracking)
    {
        var config = OVRProjectConfig.CachedProjectConfig;
        if (config == null)
        {
            Debug.LogError("[VRGlasses] OVRProjectConfig not found. Is Meta XR Core SDK installed?");
            return;
        }

        config.handTrackingSupport = handTracking;
        if (config.eyeTrackingSupport == OVRProjectConfig.FeatureSupport.None)
        {
            config.eyeTrackingSupport = OVRProjectConfig.FeatureSupport.Supported;
        }

        OVRProjectConfig.CommitProjectConfig(config);
        AssetDatabase.SaveAssets();
        Report();
    }
}
