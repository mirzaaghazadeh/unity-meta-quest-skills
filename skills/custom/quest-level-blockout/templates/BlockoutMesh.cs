// Blockout geometry for generated levels. Copy into Assets/Editor/ (it uses UnityEditor).
// Part of unity-meta-quest-skills (MIT), skill quest-level-blockout.
using System.IO;
using UnityEditor;
using UnityEngine;

namespace QuestBlockout
{
    public static class BlockoutMesh
    {
        /// <summary>
        /// An axis-aligned box whose UVs are world-space meters times <paramref name="uvPerMeter"/>, so a
        /// tiling texture keeps one density and lines up across neighbouring boxes. Place the GameObject
        /// at <paramref name="worldCenter"/> with identity rotation and scale.
        /// Submesh 0 is the top face, submesh 1 the sides and bottom (two materials: roof and facade).
        /// </summary>
        public static Mesh Box(string name, Vector3 worldCenter, Vector3 size, float uvPerMeter)
        {
            var h = size / 2;
            var vertices = new Vector3[24];
            var uvs = new Vector2[24];
            var top = new int[6];
            var rest = new int[30];
            var restCount = 0;

            // Unity's front faces wind clockwise, so Cross(u, v) must equal -normal.
            // u then also runs left-to-right for a viewer facing the face (textures aren't mirrored).
            var faces = new (Vector3 normal, Vector3 u, Vector3 v)[]
            {
                (Vector3.up, Vector3.right, Vector3.forward),
                (Vector3.down, Vector3.right, Vector3.back),
                (Vector3.right, Vector3.forward, Vector3.up),
                (Vector3.left, Vector3.back, Vector3.up),
                (Vector3.forward, Vector3.left, Vector3.up),
                (Vector3.back, Vector3.right, Vector3.up),
            };
            var corners = new[] { (-1, -1), (-1, 1), (1, 1), (1, -1) };

            for (var f = 0; f < 6; f++)
            {
                var (n, u, v) = faces[f];
                var center = Vector3.Scale(n, h);
                var uExtent = Mathf.Abs(Vector3.Dot(u, h));
                var vExtent = Mathf.Abs(Vector3.Dot(v, h));
                for (var c = 0; c < 4; c++)
                {
                    var local = center + u * (corners[c].Item1 * uExtent) + v * (corners[c].Item2 * vExtent);
                    var world = worldCenter + local;
                    vertices[f * 4 + c] = local;
                    uvs[f * 4 + c] = new Vector2(Vector3.Dot(world, u), Vector3.Dot(world, v)) * uvPerMeter;
                }
                var tris = new[] { f * 4, f * 4 + 1, f * 4 + 2, f * 4, f * 4 + 2, f * 4 + 3 };
                if (f == 0)
                    tris.CopyTo(top, 0);
                else
                {
                    tris.CopyTo(rest, restCount);
                    restCount += 6;
                }
            }

            var mesh = new Mesh { name = name, vertices = vertices, uv = uvs, subMeshCount = 2 };
            mesh.SetTriangles(top, 0);
            mesh.SetTriangles(rest, 1);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>A torus in the XY plane, facing +Z (checkpoint rings, hoops).</summary>
        public static Mesh Torus(string name, float radius, float tube, int segments = 48, int sides = 10)
        {
            var vertices = new Vector3[(segments + 1) * (sides + 1)];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[segments * sides * 6];
            for (var i = 0; i <= segments; i++)
            {
                var a = i * Mathf.PI * 2 / segments;
                var ring = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0);
                for (var j = 0; j <= sides; j++)
                {
                    var b = j * Mathf.PI * 2 / sides;
                    var normal = ring * Mathf.Cos(b) + Vector3.forward * Mathf.Sin(b);
                    var index = i * (sides + 1) + j;
                    vertices[index] = ring * radius + normal * tube;
                    normals[index] = normal;
                    uvs[index] = new Vector2((float)i / segments, (float)j / sides);
                }
            }
            var t = 0;
            for (var i = 0; i < segments; i++)
            for (var j = 0; j < sides; j++)
            {
                var a = i * (sides + 1) + j;
                var b = a + sides + 1;
                triangles[t++] = a; triangles[t++] = a + 1; triangles[t++] = b;
                triangles[t++] = b; triangles[t++] = a + 1; triangles[t++] = b + 1;
            }
            var mesh = new Mesh { name = name, vertices = vertices, normals = normals, uv = uvs, triangles = triangles };
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Writes (once) and imports a 4 x 4 m grid texture: thin lines every meter, a heavier line at
        /// the tile edge. Use it with uvPerMeter = 0.25 so one grid square is one meter.
        /// </summary>
        public static Texture2D GridTexture(string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (existing != null)
                return existing;

            const int size = 512;
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var major = x < 4 || y < 4;
                var minor = x % (size / 4) < 2 || y % (size / 4) < 2;
                var value = major ? (byte)110 : minor ? (byte)175 : (byte)235;
                pixels[y * size + x] = new Color32(value, value, value, 255);
            }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true);
            texture.SetPixels32(pixels);
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.anisoLevel = 4;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }
    }
}
