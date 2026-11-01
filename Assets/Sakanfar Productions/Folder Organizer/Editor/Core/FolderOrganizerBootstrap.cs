using UnityEditor;

namespace SakanfarProductions.FolderOrganizer
{
    [InitializeOnLoad]
    internal static class FolderOrganizerBootstrap
    {
        static FolderOrganizerBootstrap()
        {
            EditorApplication.delayCall += EnsureAssets;
        }

        private static void EnsureAssets()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Sakanfar Productions")) AssetDatabase.CreateFolder("Assets", "Sakanfar Productions");
            if (!AssetDatabase.IsValidFolder("Assets/Sakanfar Productions/Folder Organizer")) AssetDatabase.CreateFolder("Assets/Sakanfar Productions", "Folder Organizer");
            if (!AssetDatabase.IsValidFolder("Assets/Sakanfar Productions/Folder Organizer/Editor")) AssetDatabase.CreateFolder("Assets/Sakanfar Productions/Folder Organizer", "Editor");
            if (!AssetDatabase.IsValidFolder("Assets/Sakanfar Productions/Folder Organizer/Editor/Data")) AssetDatabase.CreateFolder("Assets/Sakanfar Productions/Folder Organizer/Editor", "Data");
            FolderAppearanceDatabase.LoadOrCreate();
        }
    }
}
