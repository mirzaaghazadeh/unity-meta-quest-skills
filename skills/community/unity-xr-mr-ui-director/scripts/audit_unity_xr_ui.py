#!/usr/bin/env python3
"""Read-only Unity XR/MR UI project audit.

The script emits JSON and never changes the Unity project. It checks editor
version, known XR/UI package presence, and text-serialized scene/prefab hints
for duplicate UI input systems.
"""

from __future__ import annotations

import argparse
from collections import defaultdict
from collections.abc import Iterable, Mapping
import json
import os
from pathlib import Path
import re
import sys
from typing import Any


PACKAGE_GROUPS = {
    "rendering": ["com.unity.render-pipelines.universal"],
    "input": ["com.unity.inputsystem"],
    "xr_management": ["com.unity.xr.management"],
    "openxr": ["com.unity.xr.openxr"],
    "xri": ["com.unity.xr.interaction.toolkit"],
    "hands": ["com.unity.xr.hands"],
    "ui": ["com.unity.ugui"],
    "text": ["com.unity.textmeshpro"],
    "meta_core": ["com.meta.xr.sdk.core", "com.meta.xr.sdk.all"],
    "mruk": ["com.meta.xr.mrutilitykit"],
}

SCAN_SUFFIXES = {".unity", ".prefab"}
COMPONENT_META_FILENAMES = {
    "event_system": ("EventSystem.cs.meta",),
    "standalone_input_module": ("StandaloneInputModule.cs.meta",),
    "input_system_ui_module": ("InputSystemUIInputModule.cs.meta",),
    "xr_ui_input_module": ("XRUIInputModule.cs.meta",),
    "tracked_device_graphic_raycaster": ("TrackedDeviceGraphicRaycaster.cs.meta",),
    "meta_pointable_canvas": ("PointableCanvas.cs.meta",),
    "meta_pointable_canvas_module": ("PointableCanvasModule.cs.meta",),
}
COMPONENT_KEYS = (*COMPONENT_META_FILENAMES, "canvas")
UI_INPUT_MODULE_KEYS = (
    "standalone_input_module",
    "input_system_ui_module",
    "xr_ui_input_module",
    "meta_pointable_canvas_module",
)
GUID_LINE_PATTERN = re.compile(r"^guid:\s*([0-9a-fA-F]{32})\s*$", re.MULTILINE)
SCRIPT_GUID_PATTERN = re.compile(
    r"m_Script:\s*\{[^}\n]*\bguid:\s*([0-9a-fA-F]{32})\b[^}\n]*\}"
)
CANVAS_PATTERN = re.compile(r"^Canvas:\s*$", re.MULTILINE)

UNITY_HUB_EDITORS_ENV = "UNITY_HUB_EDITORS_PATH"


def load_json(path: Path, *, required: bool = False) -> dict[str, Any]:
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError as exc:
        if required:
            raise ValueError(f"required JSON file not found: {path}") from exc
        return {}
    except (OSError, json.JSONDecodeError) as exc:
        raise ValueError(f"cannot parse {path}: {exc}") from exc
    if not isinstance(data, dict):
        raise ValueError(f"expected a JSON object in {path}")
    return data


def editor_version(project: Path) -> str | None:
    path = project / "ProjectSettings" / "ProjectVersion.txt"
    try:
        text = path.read_text(encoding="utf-8", errors="replace")
    except OSError:
        return None
    match = re.search(r"^m_EditorVersion:\s*(.+?)\s*$", text, re.MULTILINE)
    return match.group(1) if match else None


def _dedupe_paths(paths: Iterable[Path]) -> list[Path]:
    result: list[Path] = []
    seen: set[str] = set()
    for path in paths:
        expanded = path.expanduser()
        key = os.path.normcase(str(expanded))
        if key in seen:
            continue
        seen.add(key)
        result.append(expanded)
    return result


def default_unity_editor_roots(
    *,
    platform: str | None = None,
    environ: Mapping[str, str] | None = None,
    home: Path | None = None,
) -> list[Path]:
    """Return platform defaults or an explicit environment override.

    UNITY_HUB_EDITORS_PATH may contain one or more directories separated by
    the platform path separator (`:` on POSIX and `;` on Windows).
    """

    current_platform = platform or sys.platform
    environment = os.environ if environ is None else environ
    override = environment.get(UNITY_HUB_EDITORS_ENV, "").strip()
    if override:
        return _dedupe_paths(
            Path(value.strip())
            for value in override.split(os.pathsep)
            if value.strip()
        )

    if current_platform == "darwin":
        return [Path("/Applications/Unity/Hub/Editor")]

    if current_platform.startswith("win"):
        roots: list[Path] = []
        for variable in ("ProgramFiles", "ProgramFiles(x86)"):
            base = environment.get(variable)
            if base:
                roots.append(Path(base) / "Unity" / "Hub" / "Editor")
        return _dedupe_paths(roots or [Path(r"C:\Program Files\Unity\Hub\Editor")])

    user_home = home or Path.home()
    return _dedupe_paths(
        [
            user_home / "Unity" / "Hub" / "Editor",
            Path("/opt/Unity/Hub/Editor"),
            Path("/opt/unity/editors"),
        ]
    )


def unity_editor_executable_candidates(version_dir: Path, platform: str | None = None) -> list[Path]:
    current_platform = platform or sys.platform
    if current_platform == "darwin":
        return [version_dir / "Unity.app" / "Contents" / "MacOS" / "Unity"]
    if current_platform.startswith("win"):
        return [version_dir / "Editor" / "Unity.exe"]
    return [version_dir / "Editor" / "Unity", version_dir / "Unity"]


def installed_unity_versions(
    roots: list[Path] | None = None,
    platform: str | None = None,
) -> list[str]:
    versions: set[str] = set()
    search_roots = roots if roots is not None else default_unity_editor_roots(platform=platform)
    for root in search_roots:
        if not root.is_dir():
            continue
        try:
            version_dirs = list(root.iterdir())
        except OSError:
            continue
        for version_dir in version_dirs:
            if not version_dir.is_dir():
                continue
            if any(path.is_file() for path in unity_editor_executable_candidates(version_dir, platform)):
                versions.add(version_dir.name)
    return sorted(versions)


def package_report(project: Path) -> tuple[dict[str, Any], dict[str, str]]:
    manifest_path = project / "Packages" / "manifest.json"
    manifest = load_json(manifest_path, required=True)
    deps = manifest.get("dependencies")
    if not isinstance(deps, dict):
        raise ValueError(f"expected a dependencies object in {manifest_path}")
    normalized = {str(k): str(v) for k, v in deps.items()}
    report: dict[str, Any] = {}
    for group, names in PACKAGE_GROUPS.items():
        matches = {name: normalized[name] for name in names if name in normalized}
        report[group] = {
            "present": bool(matches),
            "packages": matches,
        }
    return report, normalized


def discover_component_guids(project: Path) -> dict[str, set[str]]:
    """Resolve component script GUIDs from the project's local .cs.meta files."""

    result = {name: set() for name in COMPONENT_META_FILENAMES}
    filenames_to_components: dict[str, list[str]] = defaultdict(list)
    for component, filenames in COMPONENT_META_FILENAMES.items():
        for filename in filenames:
            filenames_to_components[filename].append(component)

    search_roots = [
        project / "Assets",
        project / "Packages",
        project / "Library" / "PackageCache",
    ]
    for root in search_roots:
        if not root.is_dir():
            continue
        try:
            meta_paths = root.rglob("*.cs.meta")
            for meta_path in meta_paths:
                components = filenames_to_components.get(meta_path.name)
                if not components:
                    continue
                try:
                    text = meta_path.read_text(encoding="utf-8", errors="replace")
                except OSError:
                    continue
                match = GUID_LINE_PATTERN.search(text)
                if not match:
                    continue
                guid = match.group(1).lower()
                for component in components:
                    result[component].add(guid)
        except OSError:
            continue
    return result


def _empty_scan(max_files: int, component_guids: dict[str, set[str]]) -> dict[str, Any]:
    return {
        "scanned_files": 0,
        "scan_limit": max_files,
        "skipped_large": 0,
        "skipped_binary_or_unreadable": 0,
        "findings": {name: [] for name in COMPONENT_KEYS},
        "asset_component_counts": {},
        "component_detection": {
            "resolved": sorted(name for name, guids in component_guids.items() if guids),
            "unresolved": sorted(name for name, guids in component_guids.items() if not guids),
        },
    }


def scan_assets(
    project: Path,
    max_files: int,
    max_bytes: int,
    component_guids: dict[str, set[str]] | None = None,
) -> dict[str, Any]:
    assets = project / "Assets"
    resolved_guids = component_guids if component_guids is not None else discover_component_guids(project)
    scan = _empty_scan(max_files, resolved_guids)
    if not assets.is_dir():
        return scan

    guid_to_components: dict[str, list[str]] = defaultdict(list)
    for component, guids in resolved_guids.items():
        for guid in guids:
            guid_to_components[guid.lower()].append(component)

    findings: dict[str, list[str]] = scan["findings"]
    asset_component_counts: dict[str, dict[str, int]] = scan["asset_component_counts"]
    scanned = 0
    skipped_large = 0
    skipped_binary = 0

    try:
        asset_paths = sorted(assets.rglob("*"))
    except OSError as exc:
        raise OSError(f"cannot enumerate {assets}: {exc}") from exc

    for path in asset_paths:
        if scanned >= max_files:
            break
        if not path.is_file() or path.suffix.lower() not in SCAN_SUFFIXES:
            continue
        try:
            size = path.stat().st_size
            if size > max_bytes:
                skipped_large += 1
                continue
            raw = path.read_bytes()
            if b"\0" in raw[:4096]:
                skipped_binary += 1
                continue
            text = raw.decode("utf-8", errors="replace")
        except OSError:
            skipped_binary += 1
            continue

        scanned += 1
        rel = path.relative_to(project).as_posix()
        counts: dict[str, int] = {}
        canvas_count = len(CANVAS_PATTERN.findall(text))
        if canvas_count:
            counts["canvas"] = canvas_count

        for match in SCRIPT_GUID_PATTERN.finditer(text):
            for component in guid_to_components.get(match.group(1).lower(), ()):
                counts[component] = counts.get(component, 0) + 1

        if counts:
            asset_component_counts[rel] = counts
            for component in counts:
                findings[component].append(rel)

    scan.update(
        {
            "scanned_files": scanned,
            "skipped_large": skipped_large,
            "skipped_binary_or_unreadable": skipped_binary,
        }
    )
    return scan


def warnings_for(
    version: str | None,
    installed: list[str],
    packages: dict[str, Any],
    scan: dict[str, Any],
) -> list[str]:
    warnings: list[str] = []
    if not version:
        warnings.append("ProjectVersion.txt is missing or unreadable")
    elif version not in installed:
        warnings.append(f"required Unity editor {version} is not installed")

    for group in ("input", "xr_management", "openxr", "xri", "ui"):
        if not packages[group]["present"]:
            warnings.append(f"core package group missing: {group}")

    for asset, counts in sorted(scan["asset_component_counts"].items()):
        module_counts = {
            name: counts.get(name, 0)
            for name in UI_INPUT_MODULE_KEYS
            if counts.get(name, 0) > 0
        }
        if sum(module_counts.values()) > 1:
            detail = ", ".join(f"{name}={count}" for name, count in module_counts.items())
            warnings.append(f"multiple UI input modules appear in serialized asset {asset}: {detail}")
        event_system_count = counts.get("event_system", 0)
        if event_system_count > 1:
            warnings.append(
                f"multiple EventSystem components appear in serialized asset {asset}: {event_system_count}"
            )

    if packages["mruk"]["present"] and not packages["meta_core"]["present"]:
        warnings.append("MRUK is present but no Meta core package was found in manifest.json")

    return warnings


def observations_for(packages: dict[str, Any], scan: dict[str, Any]) -> list[str]:
    """Return non-failing hints that need Unity Editor or design context."""

    observations: list[str] = []
    detection = scan["component_detection"]
    raycaster_resolved = "tracked_device_graphic_raycaster" in detection["resolved"]
    if packages["xri"]["present"] and raycaster_resolved:
        for asset, counts in sorted(scan["asset_component_counts"].items()):
            if counts.get("canvas", 0) and not counts.get("tracked_device_graphic_raycaster", 0):
                observations.append(
                    f"Canvas appears in serialized asset {asset} without a "
                    "TrackedDeviceGraphicRaycaster; confirm whether it is interactive"
                )
    return observations


def _emit_error(error_code: str, message: str, project: Path | None = None) -> None:
    result: dict[str, Any] = {
        "ok": False,
        "read_only": True,
        "error_code": error_code,
        "error": message,
    }
    if project is not None:
        result["project"] = str(project)
    print(json.dumps(result, ensure_ascii=False, sort_keys=True))


def _looks_like_unity_project(project: Path) -> bool:
    return any(
        path.exists()
        for path in (
            project / "Assets",
            project / "Packages",
            project / "ProjectSettings",
        )
    )


def main() -> int:
    parser = argparse.ArgumentParser(description="Read-only Unity XR/MR UI audit")
    parser.add_argument("project", help="absolute or relative Unity project path")
    parser.add_argument(
        "--unity-editors-path",
        action="append",
        default=[],
        metavar="DIRECTORY",
        help=(
            "Unity Hub Editor directory; repeat for multiple roots. "
            f"Overrides {UNITY_HUB_EDITORS_ENV} and platform defaults."
        ),
    )
    parser.add_argument("--max-files", type=int, default=2000)
    parser.add_argument("--max-bytes", type=int, default=8 * 1024 * 1024)
    args = parser.parse_args()

    project = Path(args.project).expanduser().resolve()
    if not project.is_dir():
        _emit_error("project_not_found", f"project directory not found: {project}", project)
        return 2
    if not _looks_like_unity_project(project):
        _emit_error("not_unity_project", f"directory does not look like a Unity project: {project}", project)
        return 2
    if args.max_files < 1 or args.max_bytes < 1:
        _emit_error("invalid_scan_limits", "scan limits must be positive", project)
        return 2

    try:
        version = editor_version(project)
        editor_roots = (
            _dedupe_paths(Path(value).expanduser().resolve() for value in args.unity_editors_path)
            if args.unity_editors_path
            else _dedupe_paths(path.resolve() for path in default_unity_editor_roots())
        )
        installed = installed_unity_versions(editor_roots)
        packages, all_packages = package_report(project)
        scan = scan_assets(project, args.max_files, args.max_bytes)
    except ValueError as exc:
        _emit_error("invalid_project_data", str(exc), project)
        return 2
    except OSError as exc:
        _emit_error("io_error", str(exc), project)
        return 2

    warnings = warnings_for(version, installed, packages, scan)
    observations = observations_for(packages, scan)
    result = {
        "ok": not warnings,
        "read_only": True,
        "project": str(project),
        "required_editor": version,
        "editor_search_roots": [str(path) for path in editor_roots],
        "installed_editors": installed,
        "editor_available": bool(version and version in installed),
        "packages": packages,
        "manifest_dependency_count": len(all_packages),
        "asset_scan": scan,
        "warnings": warnings,
        "observations": observations,
        "note": (
            "Component-name detection uses local .cs.meta GUIDs and remains heuristic; "
            "confirm active components in the Unity Editor."
        ),
    }
    print(json.dumps(result, ensure_ascii=False, indent=2, sort_keys=True))
    return 0 if not warnings else 1


if __name__ == "__main__":
    sys.exit(main())
