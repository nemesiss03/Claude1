using System;
using UnityEngine;

namespace SakanfarProductions.FolderOrganizer
{
    /// <summary>Persistent appearance information for one project folder.</summary>
    [Serializable]
    public sealed class FolderAppearanceEntry
    {
        public string guid;
        public string lastKnownPath;
        public bool hasColor;
        public Color color = Color.white;
        public FolderIconId icon = FolderIconId.None;
        public bool isFavorite;
        public bool cameFromAutomaticRule;

        public FolderAppearanceEntry(string folderGuid, string path)
        {
            guid = folderGuid;
            lastKnownPath = path;
        }
    }
}
