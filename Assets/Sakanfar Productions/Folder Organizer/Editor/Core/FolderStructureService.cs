using System;
using System.Collections.Generic;
using UnityEditor;

namespace SakanfarProductions.FolderOrganizer
{
    /// <summary>Creates missing project folders without moving or deleting existing assets.</summary>
    public static class FolderStructureService
    {
        public sealed class CreationResult
        {
            public string rootPath;
            public readonly List<string> created = new List<string>();
            public readonly List<string> skipped = new List<string>();
            public string Error;
        }

        public static CreationResult Create(string location, string rootName, IEnumerable<FolderTemplateItem> items)
        {
            var result = new CreationResult();
            location = FolderPathUtility.Normalize(location);
            if (!FolderPathUtility.IsValidProjectFolderPath(location)) { result.Error = "Choose a valid folder inside the Assets folder."; return result; }
            if (!FolderPathUtility.IsSafeRelativeFolderName(rootName)) { result.Error = "The root folder name is not valid."; return result; }

            var root = location + "/" + rootName;
            result.rootPath = root;
            EnsureFolder(location, rootName, result);
            foreach (var item in items ?? Array.Empty<FolderTemplateItem>())
            {
                if (item == null || !item.enabled || string.IsNullOrWhiteSpace(item.relativePath)) continue;
                var current = root;
                var parts = item.relativePath.Replace('\\', '/').Split('/');
                foreach (var part in parts)
                {
                    if (!FolderPathUtility.IsSafeRelativeFolderName(part)) continue;
                    if (!AssetDatabase.IsValidFolder(current + "/" + part))
                        EnsureFolder(current, part, result);
                    current += "/" + part;
                }
            }
            AssetDatabase.Refresh();
            return result;
        }

        private static void EnsureFolder(string parent, string name, CreationResult result)
        {
            var path = parent + "/" + name;
            if (AssetDatabase.IsValidFolder(path)) { result.skipped.Add(path); return; }
            var guid = AssetDatabase.CreateFolder(parent, name);
            if (!string.IsNullOrEmpty(guid)) result.created.Add(path);
            else result.Error = "Unity could not create " + path + ".";
        }
    }
}
