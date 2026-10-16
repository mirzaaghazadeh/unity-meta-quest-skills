// Checks a route of rectangular platforms (rooftops, islands, ledges) visited in order: the gap to
// the next platform, the height change, and how much the two overlap on the other axis (the width
// you can land on). Engine-agnostic apart from Mathf; copy anywhere (Assets/Editor/ is typical).
// Part of unity-meta-quest-skills (MIT), skill quest-level-blockout.
using System.Collections.Generic;
using UnityEngine;

namespace QuestBlockout
{
    /// <summary>A platform's walkable top: an X/Z rectangle at height Top (meters).</summary>
    public struct Platform
    {
        public string Id;
        public float MinX, MaxX, MinZ, MaxZ, Top;

        public Platform(string id, float minX, float maxX, float minZ, float maxZ, float top)
        {
            Id = id; MinX = minX; MaxX = maxX; MinZ = minZ; MaxZ = maxZ; Top = top;
        }
    }

    public static class RouteValidator
    {
        /// <summary>
        /// Appends one measured line per jump to <paramref name="report"/> and returns the errors.
        /// Ranges are the game's own design rules (inclusive), not engine or store limits.
        /// </summary>
        public static List<string> CheckRoute(IReadOnlyList<Platform> route, Vector2 gapRange, Vector2 heightStepRange,
            float minLandingWidth, List<string> report)
        {
            var errors = new List<string>();
            for (var i = 0; i + 1 < route.Count; i++)
            {
                var a = route[i];
                var b = route[i + 1];
                // Positive on the axis where the platforms are apart, negative (overlap) on the other.
                var dx = Mathf.Max(b.MinX - a.MaxX, a.MinX - b.MaxX);
                var dz = Mathf.Max(b.MinZ - a.MaxZ, a.MinZ - b.MaxZ);
                var gap = Mathf.Max(dx, dz);
                var landingWidth = -Mathf.Min(dx, dz);
                var rise = b.Top - a.Top;

                report.Add($"{a.Id} -> {b.Id}: gap {gap:F1} m, height {rise:+0.0;-0.0} m, landing width {landingWidth:F1} m");
                if (!Within(gap, gapRange))
                    errors.Add($"{a.Id} -> {b.Id}: gap {gap:F1} m is outside {gapRange.x}–{gapRange.y} m");
                if (!Within(Mathf.Abs(rise), heightStepRange))
                    errors.Add($"{a.Id} -> {b.Id}: height change {rise:F1} m is outside {heightStepRange.x}–{heightStepRange.y} m");
                if (landingWidth < minLandingWidth)
                    errors.Add($"{a.Id} -> {b.Id}: landing width {landingWidth:F1} m is below {minLandingWidth} m");
            }
            return errors;
        }

        static bool Within(float value, Vector2 range) => value >= range.x - 0.001f && value <= range.y + 0.001f;
    }
}
