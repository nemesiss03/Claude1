using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SakanfarProductions.FolderOrganizer
{
    [Serializable]
    public sealed class FolderTemplateItem
    {
        public string relativePath;
        public bool enabled = true;
        public FolderTemplateItem(string path) { relativePath = path; }
    }

    /// <summary>Serializable custom folder structure template.</summary>
    public sealed class FolderStructureTemplate : ScriptableObject
    {
        public string rootName = "_ Project";
        public List<FolderTemplateItem> folders = new List<FolderTemplateItem>();

        public static FolderStructureTemplate CreateDefault()
        {
            var template = CreateInstance<FolderStructureTemplate>();
            template.rootName = "_ Project";
            var paths = new[] { "Animations", "Art", "Art/Materials", "Art/Models", "Art/Shaders", "Art/Sprites", "Art/Textures", "Art/VFX", "Audio", "Audio/Music", "Audio/SFX", "Code", "Code/Editor", "Code/Runtime", "Code/ScriptableObjects", "Code/Tests", "Documentation", "Fonts", "Plugins", "Prefabs", "Resources", "Scenes", "Settings", "UI" };
            foreach (var path in paths) template.folders.Add(new FolderTemplateItem(path));
            return template;
        }

        public static FolderStructureTemplate LoadCustom()
        {
            const string path = "Assets/Sakanfar Productions/Folder Organizer/Editor/Data/FolderOrganizerCustomTemplate.asset";
            return AssetDatabase.LoadAssetAtPath<FolderStructureTemplate>(path);
        }

        public void SaveCustom()
        {
            const string path = "Assets/Sakanfar Productions/Folder Organizer/Editor/Data/FolderOrganizerCustomTemplate.asset";
            var existing = AssetDatabase.LoadAssetAtPath<FolderStructureTemplate>(path);
            if (existing != null && existing != this)
            {
                EditorUtility.CopySerialized(this, existing);
                EditorUtility.SetDirty(existing);
            }
            else if (AssetDatabase.GetAssetPath(this) != path)
            {
                AssetDatabase.CreateAsset(this, path);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
