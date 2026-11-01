using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SakanfarProductions.FolderOrganizer
{
    /// <summary>Editor icons plus package-owned symbols for presets without reliable Unity resources.</summary>
    public static class FolderIconPresets
    {
        private static readonly Dictionary<FolderIconId, Texture> Textures = new Dictionary<FolderIconId, Texture>();
        public static readonly FolderIconId[] Choices = {
            FolderIconId.None, FolderIconId.Scripts, FolderIconId.Prefabs, FolderIconId.Scenes,
            FolderIconId.Materials, FolderIconId.Textures, FolderIconId.Audio, FolderIconId.Models,
            FolderIconId.Shaders, FolderIconId.Art, FolderIconId.UI, FolderIconId.Editor,
            FolderIconId.VFX, FolderIconId.Settings, FolderIconId.Plugins, FolderIconId.Documentation
        };

        public static string Label(FolderIconId id) => id == FolderIconId.None ? "No icon" : id == FolderIconId.Scripts ? "C# Scripts" : id.ToString();

        public static Texture GetTexture(FolderIconId id)
        {
            if (id == FolderIconId.None) return null;
            if (Textures.TryGetValue(id, out var cached) && cached != null) return cached;
            Texture icon;
            // FindTexture returns null for missing names, allowing a type-icon fallback.
            switch (id)
            {
                case FolderIconId.Scripts: icon = EditorGUIUtility.FindTexture("cs Script Icon"); break;
                case FolderIconId.Prefabs: icon = EditorGUIUtility.FindTexture("Prefab Icon"); break;
                case FolderIconId.Scenes: icon = TypeIcon(typeof(SceneAsset)); break;
                case FolderIconId.Materials: icon = TypeIcon(typeof(Material)); break;
                case FolderIconId.Textures: icon = TypeIcon(typeof(Texture2D)); break;
                case FolderIconId.Audio: icon = FolderCategoryIcons.Get(id); break;
                case FolderIconId.Models: icon = TypeIcon(typeof(Mesh)); break;
                case FolderIconId.Shaders: icon = TypeIcon(typeof(Shader)); break;
                case FolderIconId.Art: icon = FolderCategoryIcons.Get(id); break;
                case FolderIconId.UI: icon = TypeIcon(typeof(RectTransform)); break;
                case FolderIconId.Editor: icon = EditorGUIUtility.FindTexture("SettingsIcon"); break;
                case FolderIconId.VFX: icon = FolderCategoryIcons.Get(id); break;
                case FolderIconId.Settings: icon = FolderCategoryIcons.Get(id); break;
                case FolderIconId.Plugins: icon = FolderCategoryIcons.Get(id); break;
                default: icon = TypeIcon(typeof(TextAsset)); break;
            }
            if (icon == null) icon = TypeIcon(typeof(DefaultAsset));
            Textures[id] = icon;
            return icon;
        }

        private static Texture TypeIcon(Type type) => EditorGUIUtility.ObjectContent(null, type).image;
    }
}
