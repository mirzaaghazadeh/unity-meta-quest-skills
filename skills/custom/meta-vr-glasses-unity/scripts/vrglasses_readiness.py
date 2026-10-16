#!/usr/bin/env python3
"""Read-only Meta VR Glasses readiness scan of a Unity project (no Unity needed).

Checks project files that decide whether a Quest app can run, and be used, on Meta VR Glasses:
Unity and package versions, IL2CPP/ARM64, OVRProjectConfig hand/eye tracking and experimental
flags, a custom AndroidManifest.xml, screen-space canvases, OVRManager dynamic resolution, and
C# code that only works with controllers or checks the headset model.

It's a heuristic. It complements Meta's Device Readiness Check (Window > Meta > Tools) and does
not replace testing in Meta XR Simulator or on a device. It never writes to the project.

Usage:
  python3 -I vrglasses_readiness.py <UnityProject> [--json] [--include-samples]
Exit codes: 0 no High findings, 1 at least one High finding, 2 not a Unity project.
"""
import json
import os
import re
import sys

HIGH, MEDIUM, LOW, INFO = "High", "Medium", "Low", "Info"
ORDER = {HIGH: 0, MEDIUM: 1, LOW: 2, INFO: 3}
DEVICE_TYPES = {1: "Quest", 2: "Quest2", 3: "QuestPro", 4: "Quest3", 5: "Quest3S", 6: "VRGlasses"}
SKIP_DIRS = {"Oculus", "Plugins", "ThirdParty", "Third Party", "TextMesh Pro", "MetaXR"}

# (id, severity, regex, problem, fix). Patterns are matched per line in Assets/**/*.cs.
CODE_RULES = [
    ("controller-button", HIGH,
     r"OVRInput\.Get(?:Down|Up)?\s*\(\s*OVRInput\.(?:Button\.(?:Primary|Secondary)(?:IndexTrigger|HandTrigger|Thumbstick)\b|Button\.(?:Two|Four)\b|RawButton\.(?:[LR](?:IndexTrigger|HandTrigger|Thumbstick)|B|Y)\b)",
     "Trigger, grip, thumbstick-click or B/Y read: no hand equivalent",
     "Use ISDK pinch/poke/palm grab (or gaze + pinch). A/X reads keep working: OVRInput maps index pinch to A/X"),
    ("controller-axis", HIGH,
     r"OVRInput\.Get\s*\(\s*OVRInput\.(?:Axis2D\.(?:Primary|Secondary)Thumbstick|RawAxis2D\.[LR]Thumbstick|Axis1D\.|RawAxis1D\.)",
     "Thumbstick or analog trigger/grip axis: no hand equivalent",
     "Teleport for locomotion, physical turning, pinch-and-drag or microgesture swipes for scrolling"),
    ("controller-pose", HIGH,
     r"OVRInput\.GetLocalController(?:Position|Rotation|Velocity|AngularVelocity|Acceleration|AngularAcceleration)",
     "Controller pose read: unavailable without controllers",
     "Use hand joint poses (OVRHand/OVRSkeleton or ISDK HandRef) and handle tracking loss"),
    ("legacy-grab", HIGH, r"\bOVRGrabb(?:er|able)\b",
     "Legacy controller-driven grab (OVRGrabber/OVRGrabbable)",
     "Migrate to ISDK Grabbable + HandGrabInteractable, or GazeInteractable + Grabbable"),
    ("device-model-check", MEDIUM,
     r"GetSystemHeadsetType|SystemHeadset\.|systemHeadsetType|SystemInfo\.deviceModel|Build\.MODEL",
     "Headset-model check: in compatibility mode VR Glasses can report an older Quest model",
     "Branch on capabilities (eye tracking supported, hands/controllers connected), not the model"),
    ("xri-controller", MEDIUM, r"\bActionBasedController\b|\bXRController\b|\bXRBaseController\b",
     "XR Interaction Toolkit controller component",
     "Check every flow also works with XRI hand interactors or switch the flow to ISDK gaze/hands"),
    ("legacy-input", LOW, r"\bInput\.(?:GetButton|GetAxis|GetKey)\w*\s*\(",
     "Legacy Unity Input read (keyboard/gamepad style)",
     "Confirm it isn't the only way to reach a required action on device"),
    ("hover-dependency", LOW, r"IPointerEnterHandler|OnPointerEnter\s*\(|WhenHover|\.Hover\b",
     "Hover handler: no required flow may depend on hover with gaze input",
     "Keep hover cosmetic; make every action reachable by select alone"),
    ("refresh-rate", LOW, r"systemDisplayFrequency\s*=\s*\d",
     "Hard-coded refresh rate",
     "Pick from OVRManager.display.displayFrequenciesAvailable at runtime"),
    ("haptics", INFO, r"OVRInput\.SetControllerVibration|\bOVRHaptics\b|HapticClipPlayer",
     "Controller haptics: silent without controllers",
     "Give audio/visual feedback too"),
]


def read(path):
    try:
        with open(path, encoding="utf-8", errors="replace") as f:
            return f.read()
    except OSError:
        return None


def version_tuple(text):
    nums = re.findall(r"\d+", text or "")
    return tuple(int(n) for n in nums[:4]) if nums else None


def yaml_value(text, key):
    m = re.search(r"^\s*" + re.escape(key) + r":\s*(.*)$", text or "", re.M)
    return m.group(1).strip() if m else None


def add(findings, sev, area, msg, fix="", where=""):
    findings.append({"severity": sev, "area": area, "message": msg, "fix": fix, "where": where})


def check_versions(root, findings, facts):
    pv = read(os.path.join(root, "ProjectSettings", "ProjectVersion.txt"))
    editor = yaml_value(pv, "m_EditorVersion")
    facts["unity"] = editor
    if editor and version_tuple(editor) < (6000, 0, 66, 2):
        add(findings, HIGH, "Unity", f"Unity {editor} is older than 6000.0.66f2 (Meta XR SDK v207 floor)",
            "Upgrade the Editor (unity-quest-dev-setup)")
    manifest = read(os.path.join(root, "Packages", "manifest.json"))
    lock = read(os.path.join(root, "Packages", "packages-lock.json"))
    deps = {}
    try:
        deps = json.loads(manifest or "{}").get("dependencies", {})
    except ValueError:
        add(findings, MEDIUM, "Packages", "Packages/manifest.json is not valid JSON")
    resolved = {}
    try:
        resolved = {k: v.get("version", "") for k, v in json.loads(lock or "{}").get("dependencies", {}).items()}
    except ValueError:
        pass

    def ver(name):
        return resolved.get(name) or deps.get(name)

    facts["packages"] = {n: ver(n) for n in (
        "com.meta.xr.sdk.core", "com.meta.xr.sdk.interaction", "com.meta.xr.sdk.interaction.ovr",
        "com.meta.xr.sdk.all", "com.unity.xr.openxr", "com.unity.xr.interaction.toolkit") if ver(n)}
    core = ver("com.meta.xr.sdk.core") or ver("com.meta.xr.sdk.all")
    if not core:
        add(findings, HIGH, "Packages", "Meta XR Core SDK not found in Packages/",
            "Add com.meta.xr.sdk.core 207+ (unity-package-management)")
    elif version_tuple(core) and version_tuple(core) < (207,):
        add(findings, MEDIUM, "Packages", f"Meta XR SDK {core} < 207: no Device Readiness Check, FoV simulation or ISDK gaze",
            "Upgrade with hz-unity-sdk-upgrade")
    isdk = ver("com.meta.xr.sdk.interaction")
    if not isdk:
        add(findings, MEDIUM, "Packages", "Interaction SDK not installed: Meta's look-and-pinch route uses ISDK (OVR integration)",
            "Add com.meta.xr.sdk.interaction + com.meta.xr.sdk.interaction.ovr 207+")
    elif version_tuple(isdk) and version_tuple(isdk) < (207,):
        add(findings, MEDIUM, "Packages", f"Interaction SDK {isdk} < 207: no gaze interactions",
            "Upgrade ISDK to 207+")
    openxr = ver("com.unity.xr.openxr")
    if openxr and version_tuple(openxr) and version_tuple(openxr) < (1, 17):
        add(findings, LOW, "Packages", f"Unity OpenXR {openxr} < 1.17.0: Meta XR Operator needs 1.17.0+")
    if ver("com.meta.xr.simulator"):
        add(findings, MEDIUM, "Packages", "com.meta.xr.simulator is deprecated and breaks compilation with Core 207",
            "Remove it; use the standalone Meta XR Simulator app")
    if ver("com.unity.xr.oculus"):
        add(findings, MEDIUM, "Packages", "Oculus XR Plugin is deprecated", "Use the Unity OpenXR Plugin")


def check_player(root, findings, facts):
    ps = read(os.path.join(root, "ProjectSettings", "ProjectSettings.asset")) or ""
    m = re.search(r"scriptingBackend:\s*\n((?:\s{4}\S.*\n)+)", ps)
    backend = re.search(r"Android:\s*(\d+)", m.group(1)).group(1) if m and re.search(r"Android:\s*(\d+)", m.group(1)) else None
    facts["androidScriptingBackend"] = {"0": "Mono", "1": "IL2CPP"}.get(backend, "not set (editor default)")
    if backend == "0":
        add(findings, HIGH, "Player", "Android scripting backend is Mono: a 32-bit-only build cannot run on VR Glasses",
            "Set IL2CPP + ARM64 (quest-unity-project-settings)")
    elif backend is None:
        add(findings, MEDIUM, "Player", "Android scripting backend not set explicitly",
            "Set IL2CPP, or check the Meta Quest build profile override")
    arch = yaml_value(ps, "AndroidTargetArchitectures")
    if arch and arch.isdigit():
        bits = int(arch)
        facts["androidArchitectures"] = [n for b, n in ((1, "ARMv7"), (2, "ARM64"), (4, "x86"), (8, "x86_64")) if bits & b]
        if not bits & 2:
            add(findings, HIGH, "Player", "ARM64 is not a target architecture", "Target ARM64 only")
        elif bits & 1:
            add(findings, LOW, "Player", "ARMv7 is also targeted (store wants ARM64 only)", "Untick ARMv7")


def find_first(root, name):
    for dirpath, dirnames, filenames in os.walk(os.path.join(root, "Assets")):
        if name in filenames:
            return os.path.join(dirpath, name)
    return None


def check_ovr_config(root, findings, facts):
    path = find_first(root, "OculusProjectConfig.asset")
    if not path:
        add(findings, MEDIUM, "OVRProjectConfig", "No OculusProjectConfig.asset found",
            "Open the project once with Meta XR Core SDK, then re-run")
        return
    cfg = read(path)
    rel = os.path.relpath(path, root)
    hands = yaml_value(cfg, "handTrackingSupport")
    eyes = yaml_value(cfg, "eyeTrackingSupport")
    facts["handTrackingSupport"] = {"0": "ControllersOnly", "1": "ControllersAndHands", "2": "HandsOnly"}.get(hands, hands)
    facts["eyeTrackingSupport"] = {"0": "None", "1": "Supported", "2": "Required"}.get(eyes, eyes)
    if hands == "0":
        add(findings, HIGH, "OVRProjectConfig",
            "Hand Tracking Support = Controllers Only: VR Glasses users without controllers hit the Switch to Controllers dialog",
            "Set Controllers And Hands (or Hands Only) once every required flow works with hands", rel)
    if eyes == "0":
        add(findings, MEDIUM, "OVRProjectConfig", "Eye Tracking Support = None: gaze targeting can't use eye tracking",
            "Set Supported (keeps a ray fallback) if you add ISDK gaze", rel)
    if yaml_value(cfg, "experimentalFeaturesEnabled") == "1":
        add(findings, MEDIUM, "OVRProjectConfig",
            "Experimental Features Enabled: marks the build experimental (needed for the Unity FoV toggle and on-device XR Operator)",
            "Turn off and rebuild before any store upload", rel)
    if yaml_value(cfg, "disableBackups") == "1":
        add(findings, INFO, "OVRProjectConfig",
            "Disable Backups is on (writes android:allowBackup=false). Unverified whether this stops Cloud Backup restoring Quest saves on VR Glasses",
            "If you rely on cross-device restore, test it", rel)
    raw = re.sub(r"[^0-9a-fA-F]", "", yaml_value(cfg, "targetDeviceTypes") or "")
    ids = [int.from_bytes(bytes.fromhex(raw[i:i + 8]), "little") for i in range(0, len(raw) - 7, 8)]
    facts["ovrTargetDevices"] = [DEVICE_TYPES.get(i, str(i)) for i in ids]
    fov = find_first(root, "OculusRuntimeSettings.asset")
    if fov and yaml_value(read(fov), "fovSimulationEnabled") == "1":
        add(findings, MEDIUM, "FoV simulation", "Simulate Device Field of View is on for Android builds",
            "Turn off before a release build", os.path.relpath(fov, root))


def check_manifest(root, findings, facts):
    path = os.path.join(root, "Assets", "Plugins", "Android", "AndroidManifest.xml")
    text = read(path)
    if text is None:
        facts["customManifest"] = False
        return
    facts["customManifest"] = True
    rel = os.path.relpath(path, root)
    ht = re.search(r'oculus\.software\.handtracking"[^>]*required="(true|false)"', text) or \
        re.search(r'required="(true|false)"[^>]*oculus\.software\.handtracking"', text)
    facts["manifestHandTracking"] = ht.group(1) if ht else "absent"
    add(findings, INFO, "Manifest",
        f"Custom AndroidManifest.xml: hand-tracking entry is {facts['manifestHandTracking']}; the build keeps existing entries instead of following Hand Tracking Support",
        "Window > Meta > Tools > Android Manifest Tool > Update AndroidManifest.xml for Store Compatibility", rel)
    sd = re.search(r'com\.oculus\.supportedDevices"\s+android:value="([^"]*)"', text)
    if sd:
        facts["manifestSupportedDevices"] = sd.group(1)
    if "com.oculus.experimental.enabled" in text:
        add(findings, MEDIUM, "Manifest", "Manifest declares com.oculus.experimental.enabled", "Remove it from release builds", rel)


def iter_files(root, exts, include_samples):
    for dirpath, dirnames, filenames in os.walk(os.path.join(root, "Assets")):
        dirnames[:] = [d for d in dirnames if d not in SKIP_DIRS and (include_samples or d != "Samples")]
        for f in filenames:
            if f.endswith(exts):
                yield os.path.join(dirpath, f)


def check_assets(root, findings, facts, include_samples):
    rules = [(rid, sev, re.compile(rx), prob, fix) for rid, sev, rx, prob, fix in CODE_RULES]
    counts = {}
    for path in iter_files(root, (".cs",), include_samples):
        rel = os.path.relpath(path, root)
        for n, line in enumerate((read(path) or "").splitlines(), 1):
            if line.lstrip().startswith("//"):
                continue
            for rid, sev, rx, prob, fix in rules:
                if rx.search(line):
                    counts[rid] = counts.get(rid, 0) + 1
                    add(findings, sev, "Code:" + rid, prob, fix, f"{rel}:{n}")
    facts["codeMatches"] = counts
    dynres = False
    for path in iter_files(root, (".unity", ".prefab"), include_samples):
        text = read(path) or ""
        rel = os.path.relpath(path, root)
        if re.search(r"^\s*_enableDynamicResolution:\s*1", text, re.M):
            dynres = True
        for block in text.split("\n--- !u!")[1:]:
            if block.startswith("223 ") and "\nCanvas:" in block:
                mode = re.search(r"m_RenderMode:\s*(\d)", block)
                if mode and mode.group(1) == "0":
                    add(findings, MEDIUM, "UI", "Screen Space - Overlay canvas: not shown in the headset",
                        "Use a World Space canvas", rel)
                elif mode and mode.group(1) == "1":
                    add(findings, MEDIUM, "UI", "Screen Space - Camera canvas: head-locked, may clip in the 70x66 deg FoV",
                        "Move it to world space or keep it inside the central FoV; verify with FoV simulation", rel)
    facts["dynamicResolutionInScenes"] = dynres
    if not dynres:
        add(findings, LOW, "Performance", "No OVRManager with Enable Dynamic Resolution found in scenes/prefabs",
            "Meta recommends dynamic resolution on VR Glasses")


def main(argv):
    args = [a for a in argv if not a.startswith("--")]
    if len(args) != 1:
        print(__doc__.strip().split("Usage:")[1], file=sys.stderr)
        return 2
    root = os.path.abspath(args[0])
    if not os.path.isfile(os.path.join(root, "ProjectSettings", "ProjectVersion.txt")):
        print(f"Not a Unity project: {root}", file=sys.stderr)
        return 2
    findings, facts = [], {}
    check_versions(root, findings, facts)
    check_player(root, findings, facts)
    check_ovr_config(root, findings, facts)
    check_manifest(root, findings, facts)
    check_assets(root, findings, facts, "--include-samples" in argv)
    def where_key(w):
        path, _, line = w.rpartition(":")
        return (path, int(line)) if line.isdigit() else (w, 0)

    findings.sort(key=lambda f: (ORDER[f["severity"]], f["area"], where_key(f["where"])))
    if "--json" in argv:
        print(json.dumps({"project": root, "facts": facts, "findings": findings}, indent=2))
    else:
        print(f"# VR Glasses readiness scan: {root}\n")
        for k, v in facts.items():
            print(f"- {k}: {v}")
        print()
        for sev in (HIGH, MEDIUM, LOW, INFO):
            group = [f for f in findings if f["severity"] == sev]
            if group:
                print(f"## {sev} ({len(group)})")
                for f in group:
                    where = f" `{f['where']}`" if f["where"] else ""
                    fix = f" Fix: {f['fix']}" if f["fix"] else ""
                    print(f"- [{f['area']}]{where} {f['message']}.{fix}")
                print()
        print("Heuristic scan. Confirm with Meta's Device Readiness Check, Meta XR Simulator (VR Glasses profile) and a device.")
    return 1 if any(f["severity"] == HIGH for f in findings) else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
