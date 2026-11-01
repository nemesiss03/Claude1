using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SakanfarProductions.FolderOrganizer
{
    /// <summary>Small original symbols, independent of optional Unity modules and icon names.</summary>
    [InitializeOnLoad]
    internal static class FolderCategoryIcons
    {
        private static readonly Dictionary<FolderIconId, Texture2D> Icons = new Dictionary<FolderIconId, Texture2D>();

        static FolderCategoryIcons()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Clear;
            EditorApplication.quitting += Clear;
        }

        public static Texture2D Get(FolderIconId id)
        {
            if (Icons.TryGetValue(id, out var existing) && existing != null) return existing;
            const int size = 64;
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    // Four coverage samples per pixel keep tiny badges smooth.
                    var sum = Color.clear;
                    for (var sy = 0; sy < 2; sy++)
                        for (var sx = 0; sx < 2; sx++)
                            sum += Sample(id, (x + (sx + 0.5f) / 2f) / 2f, (y + (sy + 0.5f) / 2f) / 2f);
                    var alpha = sum.a / 4f;
                    pixels[y * size + x] = sum.a > 0 ? new Color(sum.r / sum.a, sum.g / sum.a, sum.b / sum.a, alpha) : Color.clear;
                }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "FolderOrganizer." + id,
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            Icons[id] = texture;
            return texture;
        }

        private static Color Sample(FolderIconId id, float x, float y)
        {
            switch (id)
            {
                case FolderIconId.Audio:
                    if (Circle(x, y, 8, 9, 4) || Circle(x, y, 21, 12, 4) ||
                        Box(x, y, 10, 9, 13, 25) || Box(x, y, 23, 12, 26, 28) ||
                        (x >= 10 && x <= 26 && y >= 22 + (x - 10) * 0.1875f && y <= 26 + (x - 10) * 0.1875f))
                        return new Color(0.32f, 0.83f, 1f);
                    break;
                case FolderIconId.VFX:
                    if (Spark(x, y, 16, 15, 12) || Spark(x, y, 6, 26, 4) || Spark(x, y, 27, 25, 3))
                        return new Color(1f, 0.81f, 0.28f);
                    break;
                case FolderIconId.Settings:
                    var dx = x - 16; var dy = y - 16;
                    var radius = Mathf.Sqrt(dx * dx + dy * dy);
                    var teeth = Mathf.Cos(Mathf.Atan2(dy, dx) * 8) > 0.15f ? 13f : 10.5f;
                    if (radius >= 5 && radius <= teeth) return new Color(0.76f, 0.85f, 0.96f);
                    break;
                case FolderIconId.Art:
                    if (Circle(x, y, 22, 10, 3.5f)) break;
                    if ((x - 16) * (x - 16) / 169f + (y - 16) * (y - 16) / 121f > 1) break;
                    if (Circle(x, y, 8, 17, 2.5f)) return new Color(1f, 0.35f, 0.40f);
                    if (Circle(x, y, 12, 23, 2.5f)) return new Color(1f, 0.79f, 0.2f);
                    if (Circle(x, y, 20, 23, 2.5f)) return new Color(0.3f, 0.78f, 0.5f);
                    if (Circle(x, y, 25, 18, 2.5f)) return new Color(0.25f, 0.6f, 1f);
                    return new Color(0.98f, 0.77f, 0.58f);
                case FolderIconId.Plugins:
                    if (Box(x, y, 10, 13, 24, 21) || Circle(x, y, 17, 13, 7) ||
                        Box(x, y, 11, 21, 14, 28) || Box(x, y, 20, 21, 23, 28) || Box(x, y, 15, 3, 19, 10))
                        return new Color(0.77f, 0.57f, 1f);
                    break;
            }
            return Color.clear;
        }

        private static bool Circle(float x, float y, float cx, float cy, float radius) =>
            (x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius;
        private static bool Box(float x, float y, float left, float bottom, float right, float top) =>
            x >= left && x <= right && y >= bottom && y <= top;
        private static bool Spark(float x, float y, float cx, float cy, float radius) =>
            Mathf.Pow(Mathf.Abs(x - cx) / radius, 0.6f) + Mathf.Pow(Mathf.Abs(y - cy) / radius, 0.6f) <= 1;

        private static void Clear()
        {
            foreach (var texture in Icons.Values)
                if (texture != null) Object.DestroyImmediate(texture);
            Icons.Clear();
        }
    }
}
