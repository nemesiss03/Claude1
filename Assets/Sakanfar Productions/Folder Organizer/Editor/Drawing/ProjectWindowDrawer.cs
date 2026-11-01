using UnityEditor;
using UnityEngine;

namespace SakanfarProductions.FolderOrganizer
{
    /// <summary>Colors only the folder icon, leaving Unity's labels and selection untouched.</summary>
    [InitializeOnLoad]
    public static class ProjectWindowDrawer
    {
        private static readonly FolderDrawingState Cache = new FolderDrawingState();

        static ProjectWindowDrawer()
        {
            EditorApplication.projectWindowItemOnGUI += Draw;
            EditorApplication.projectChanged += ResetStaticState;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Undo.undoRedoPerformed += ResetStaticState;
            EditorApplication.delayCall += RemoveLegacyCache;
        }

        [InitializeOnLoadMethod]
        private static void ResetStaticState()
        {
            Cache.Reset();
            RepaintProjectWindow();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.EnteredEditMode)
                ResetStaticState();
        }

        public static void RepaintProjectWindow() => EditorApplication.RepaintProjectWindow();

        internal static void RemoveLegacyCache()
        {
            // Older releases stored a hidden native ScriptableObject across domain reloads.
            // Keep its original native base type until those objects have been disposed.
            foreach (var legacy in Resources.FindObjectsOfTypeAll<FolderProjectWindowDrawerCache>())
                if (!AssetDatabase.Contains(legacy) && legacy.hideFlags == HideFlags.HideAndDontSave)
                    Object.DestroyImmediate(legacy);
            SessionState.EraseInt("SakanfarProductions.FolderOrganizer.ProjectWindowDrawerCache");
        }

        // Keep all drawing inside the icon area. Grid labels occupy the bottom row.
        internal static Rect GetIconRect(Rect itemRect)
        {
            if (itemRect.height <= 30f)
                return new Rect(itemRect.x, itemRect.y + (itemRect.height - 16f) * 0.5f, 16f, 16f);
            var size = Mathf.Max(0, Mathf.Min(itemRect.width, itemRect.height - 18f));
            return new Rect(itemRect.x + (itemRect.width - size) * 0.5f, itemRect.y, size, size);
        }

        private static void Draw(string guid, Rect selectionRect)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint || string.IsNullOrEmpty(guid)) return;
            Cache.EnsureReady();
            var entry = Cache.Appearance == null ? null : Cache.Appearance.Get(guid);
            if (entry == null || (!entry.hasColor && !entry.isFavorite && entry.icon == FolderIconId.None)) return;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!AssetDatabase.IsValidFolder(path)) return;
            var iconRect = GetIconRect(selectionRect);
            if (iconRect.width <= 0) return;
            if (entry.hasColor)
            {
                var icon = AssetDatabase.GetCachedIcon(path);
                if (icon != null)
                {
                    var previousColor = GUI.color;
                    try
                    {
                        GUI.color = new Color(entry.color.r, entry.color.g, entry.color.b, 1f);
                        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
                    }
                    finally { GUI.color = previousColor; }
                }
            }
            if (entry.icon != FolderIconId.None)
            {
                var badge = FolderIconPresets.GetTexture(entry.icon);
                if (badge != null)
                {
                    var size = iconRect.width > 20 ? Mathf.Min(24f, iconRect.width * 0.5f) : 12f;
                    var badgeRect = new Rect(iconRect.xMax - size, iconRect.yMax - size, size, size);
                    GUI.DrawTexture(badgeRect, badge, ScaleMode.ScaleToFit, true);
                }
            }
            if (entry.isFavorite)
            {
                var size = iconRect.width > 20 ? 7f : 4f;
                var marker = new Rect(iconRect.x, iconRect.yMax - size - 1f, size, size);
                EditorGUI.DrawRect(new Rect(marker.x - 1, marker.y - 1, size + 2, size + 2), new Color(0.2f, 0.2f, 0.2f));
                EditorGUI.DrawRect(marker, new Color(1f, 0.80f, 0.28f));
            }
        }
    }

    /// <summary>Managed cache with no native object IDs or hidden Unity objects.</summary>
    internal sealed class FolderDrawingState
    {
        private const string AppearancePath = "Assets/Sakanfar Productions/Folder Organizer/Editor/Data/FolderAppearanceDatabase.asset";
        public FolderAppearanceDatabase Appearance { get; private set; }

        public void EnsureReady()
        {
            if (Appearance == null)
                Appearance = AssetDatabase.LoadAssetAtPath<FolderAppearanceDatabase>(AppearancePath);
        }

        public void Reset() => Appearance = null;
    }
}
