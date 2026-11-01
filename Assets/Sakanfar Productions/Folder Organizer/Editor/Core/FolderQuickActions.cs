using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SakanfarProductions.FolderOrganizer
{
    /// <summary>Shared, undoable actions for the window and folder context menu.</summary>
    public static class FolderQuickActions
    {
        public static string[] SelectedFolderGuids()
        {
            return Selection.assetGUIDs.Where(guid =>
                FolderPathUtility.IsValidProjectFolderPath(AssetDatabase.GUIDToAssetPath(guid))).Distinct().ToArray();
        }

        public static void SetColor(IEnumerable<string> guids, Color? color)
        {
            var database = FolderAppearanceDatabase.LoadOrCreate();
            Undo.RecordObject(database, color.HasValue ? "Color Folders" : "Reset Folder Color");
            foreach (var guid in guids.Distinct())
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!FolderPathUtility.IsValidProjectFolderPath(path)) continue;
                var entry = color.HasValue ? database.GetOrCreate(guid, path) : database.Get(guid);
                if (entry == null) continue;
                entry.hasColor = color.HasValue;
                if (color.HasValue) { var value = color.Value; value.a = 1; entry.color = value; }
                entry.cameFromAutomaticRule = false;
                if (!entry.hasColor && !entry.isFavorite && entry.icon == FolderIconId.None) database.Remove(guid);
            }
            Save(database);
        }

        public static void SetFavorite(IEnumerable<string> guids, bool favorite)
        {
            var database = FolderAppearanceDatabase.LoadOrCreate();
            Undo.RecordObject(database, favorite ? "Favorite Folders" : "Remove Folder Favorites");
            foreach (var guid in guids.Distinct())
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (favorite && !FolderPathUtility.IsValidProjectFolderPath(path)) continue;
                var entry = favorite ? database.GetOrCreate(guid, path) : database.Get(guid);
                if (entry == null) continue;
                entry.isFavorite = favorite;
                if (!entry.hasColor && !entry.isFavorite && entry.icon == FolderIconId.None) database.Remove(guid);
            }
            Save(database);
        }

        public static void SetIcon(IEnumerable<string> guids, FolderIconId icon)
        {
            var database = FolderAppearanceDatabase.LoadOrCreate();
            Undo.RecordObject(database, "Change Folder Icon");
            foreach (var guid in guids.Distinct())
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!FolderPathUtility.IsValidProjectFolderPath(path)) continue;
                var entry = icon == FolderIconId.None ? database.Get(guid) : database.GetOrCreate(guid, path);
                if (entry == null) continue;
                entry.icon = icon;
                entry.cameFromAutomaticRule = false;
                if (!entry.hasColor && !entry.isFavorite && entry.icon == FolderIconId.None) database.Remove(guid);
            }
            Save(database);
        }

        private static void Save(FolderAppearanceDatabase database)
        {
            database.MarkChanged();
            AssetDatabase.SaveAssets();
            ProjectWindowDrawer.RepaintProjectWindow();
        }

        public static IEnumerable<(string name, Color color)> Palette()
        {
            yield return ("Red", new Color(0.94f, 0.40f, 0.42f));
            yield return ("Orange", new Color(0.96f, 0.63f, 0.32f));
            yield return ("Yellow", new Color(0.94f, 0.79f, 0.36f));
            yield return ("Green", new Color(0.40f, 0.77f, 0.55f));
            yield return ("Teal", new Color(0.32f, 0.76f, 0.77f));
            yield return ("Blue", new Color(0.36f, 0.64f, 0.96f));
            yield return ("Purple", new Color(0.68f, 0.52f, 0.91f));
            yield return ("Pink", new Color(0.91f, 0.51f, 0.72f));
            yield return ("Coral", new Color(1f, 0.48f, 0.36f));
            yield return ("Lime", new Color(0.68f, 0.85f, 0.28f));
            yield return ("Mint", new Color(0.57f, 0.91f, 0.75f));
            yield return ("Sky", new Color(0.51f, 0.80f, 1f));
            yield return ("Indigo", new Color(0.48f, 0.49f, 0.88f));
            yield return ("Rose", new Color(1f, 0.69f, 0.77f));
            yield return ("Sand", new Color(0.83f, 0.74f, 0.59f));
            yield return ("Silver", new Color(0.76f, 0.79f, 0.84f));
        }
    }
}
