#!/usr/bin/env python3
"""Download CC0 textures from Poly Haven's public API (https://github.com/Poly-Haven/Public-API).

Examples:
    python3 -I polyhaven_fetch.py --list concrete            # popular textures in a category
    python3 -I polyhaven_fetch.py --ids concrete_floor_02,metal_plate --res 1k --out RawAssets/polyhaven
    python3 -I polyhaven_fetch.py --ids metal_plate --maps Diffuse,nor_gl,arm --res 2k

Files land in <out>/<id>/. Defaults: 1k JPG, Diffuse + nor_gl (Unity uses OpenGL-style normals).
Standard library only. Poly Haven assets are CC0: https://polyhaven.com/license
Part of unity-meta-quest-skills (MIT).
"""
import argparse
import json
import os
import sys
import urllib.request

API = "https://api.polyhaven.com"
HEADERS = {"User-Agent": "unity-meta-quest-skills/polyhaven_fetch"}


def get_json(url):
    with urllib.request.urlopen(urllib.request.Request(url, headers=HEADERS), timeout=30) as response:
        return json.load(response)


def list_category(category, limit):
    assets = get_json(f"{API}/assets?t=textures&c={category}")
    ranked = sorted(assets.items(), key=lambda item: -item[1].get("download_count", 0))[:limit]
    for asset_id, info in ranked:
        print(f"{asset_id:32} {info.get('name', '')}  ({', '.join(info.get('categories', [])[:4])})")


def fetch(asset_id, maps, resolution, fmt, out_dir):
    files = get_json(f"{API}/files/{asset_id}")
    target = os.path.join(out_dir, asset_id)
    os.makedirs(target, exist_ok=True)
    for map_name in maps:
        try:
            url = files[map_name][resolution][fmt]["url"]
        except KeyError:
            print(f"  {asset_id}: no {map_name} at {resolution} {fmt}; available maps: {', '.join(sorted(files))}", file=sys.stderr)
            continue
        path = os.path.join(target, os.path.basename(url))
        with urllib.request.urlopen(urllib.request.Request(url, headers=HEADERS), timeout=120) as response, open(path, "wb") as out:
            out.write(response.read())
        print(f"  {path} ({os.path.getsize(path) // 1024} KB)")


def main():
    parser = argparse.ArgumentParser(description="Download CC0 textures from Poly Haven.")
    parser.add_argument("--list", metavar="CATEGORY", help="list popular textures in a category and exit")
    parser.add_argument("--limit", type=int, default=10, help="how many to list (default 10)")
    parser.add_argument("--ids", help="comma-separated asset ids, e.g. concrete_floor_02,metal_plate")
    parser.add_argument("--maps", default="Diffuse,nor_gl", help="comma-separated map names (default Diffuse,nor_gl)")
    parser.add_argument("--res", default="1k", help="1k, 2k, 4k or 8k (default 1k; Quest: 1k–2k)")
    parser.add_argument("--format", default="jpg", help="jpg or png (default jpg)")
    parser.add_argument("--out", default="RawAssets/polyhaven", help="output folder (default RawAssets/polyhaven)")
    args = parser.parse_args()

    if args.list:
        list_category(args.list, args.limit)
        return
    if not args.ids:
        parser.error("give --ids or --list")
    maps = [m.strip() for m in args.maps.split(",") if m.strip()]
    for asset_id in (i.strip() for i in args.ids.split(",") if i.strip()):
        print(asset_id)
        fetch(asset_id, maps, args.res, args.format, args.out)


if __name__ == "__main__":
    main()
