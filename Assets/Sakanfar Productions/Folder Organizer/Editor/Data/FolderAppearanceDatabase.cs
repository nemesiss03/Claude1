using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SakanfarProductions.FolderOrganizer
{
    /// <summary>Project asset containing all folder styles and favorites.</summary>
    public sealed class FolderAppearanceDatabase : ScriptableObject
    {
        private const string AssetPath = "Assets/Sakanfar Productions/Folder Organizer/Editor/Data/FolderAppearanceDatabase.asset";

        [SerializeField] private List<FolderAppearanceEntry> entries = new List<FolderAppearanceEntry>();
        private Dictionary<string, FolderAppearanceEntry> lookup;

        public IReadOnlyList<FolderAppearanceEntry> Entries => entries;

        private void OnEnable() => lookup = null;
        private void OnValidate() => lookup = null;

        public static FolderAppearanceDatabase LoadOrCreate()
        {
            var database = AssetDatabase.LoadAssetAtPath<FolderAppearanceDatabase>(AssetPath);
            if (database != null)
                return database;

            database = CreateInstance<FolderAppearanceDatabase>();
            AssetDatabase.CreateAsset(database, AssetPath);
            AssetDatabase.SaveAssets();
            return database;
        }

        public FolderAppearanceEntry Get(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            EnsureLookup();
            lookup.TryGetValue(guid, out var entry);
            return entry;
        }

        public FolderAppearanceEntry GetOrCreate(string guid, string path)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            var entry = Get(guid);
            if (entry == null)
            {
                entry = new FolderAppearanceEntry(guid, path);
                entries.Add(entry);
                lookup = null;
            }
            else if (!string.IsNullOrEmpty(path))
            {
                entry.lastKnownPath = path;
            }
            return entry;
        }

        public void Remove(string guid)
        {
            var entry = Get(guid);
            if (entry == null) return;
            entries.Remove(entry);
            lookup = null;
        }

        public void RemoveEmptyEntries()
        {
            entries.RemoveAll(entry => entry == null || string.IsNullOrEmpty(entry.guid));
            lookup = null;
        }

        public void MarkChanged()
        {
            EditorUtility.SetDirty(this);
            lookup = null;
        }

        public IEnumerable<FolderAppearanceEntry> Favorites()
        {
            return entries.Where(entry => entry != null && entry.isFavorite);
        }

        private void EnsureLookup()
        {
            if (lookup != null) return;
            lookup = new Dictionary<string, FolderAppearanceEntry>();
            foreach (var entry in entries)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.guid))
                    lookup[entry.guid] = entry;
            }
        }
    }
}
