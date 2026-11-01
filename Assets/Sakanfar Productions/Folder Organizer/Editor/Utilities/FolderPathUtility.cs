using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace SakanfarProductions.FolderOrganizer
{
    /// <summary>Validation and GUID helpers for project-relative folder paths.</summary>
    public static class FolderPathUtility
    {
        public static bool IsValidProjectFolderPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            path = Normalize(path);
            if (!path.StartsWith("Assets", StringComparison.Ordinal) || path.Contains("..")) return false;
            return path == "Assets" || AssetDatabase.IsValidFolder(path);
        }

        public static string Normalize(string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : path.Replace('\\', '/').TrimEnd('/');
        }

        public static string GetGuid(string path)
        {
            path = Normalize(path);
            return IsValidProjectFolderPath(path) ? AssetDatabase.AssetPathToGUID(path) : string.Empty;
        }

        public static string ResolveGuid(string guid, string fallbackPath = null)
        {
            var path = string.IsNullOrEmpty(guid) ? string.Empty : AssetDatabase.GUIDToAssetPath(guid);
            return IsValidProjectFolderPath(path) ? path : (IsValidProjectFolderPath(fallbackPath) ? Normalize(fallbackPath) : string.Empty);
        }

        public static string FolderName(string path)
        {
            path = Normalize(path);
            return string.IsNullOrEmpty(path) ? string.Empty : Path.GetFileName(path);
        }

        public static List<string> FindAllProjectFolders()
        {
            var result = new List<string> { "Assets" };
            foreach (var guid in AssetDatabase.FindAssets("t:folder"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith("Assets/", StringComparison.Ordinal)) result.Add(path);
            }
            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        public static bool IsSafeRelativeFolderName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name == "." || name == "..") return false;
            return name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 && !name.Contains("/") && !name.Contains("\\");
        }

        public static bool IsUnderOrEqual(string path, string parent)
        {
            path = Normalize(path);
            parent = Normalize(parent);
            return string.Equals(path, parent, StringComparison.OrdinalIgnoreCase) || path.StartsWith(parent + "/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
