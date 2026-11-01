using UnityEditor;

namespace SakanfarProductions.FolderOrganizer
{
    internal static class FolderOrganizerMenuItems
    {
        [MenuItem("Tools/Sakanfar Productions/Folder Organizer", priority = 0)]
        [MenuItem("Assets/Folder Organizer/Colors and Icons...", false, 2000)]
        private static void Open() => FolderOrganizerWindow.ShowWindow();

        [MenuItem("Assets/Folder Organizer/Add to Favorites", false, 2001)]
        private static void Favorite() => FolderQuickActions.SetFavorite(FolderQuickActions.SelectedFolderGuids(), true);

        [MenuItem("Assets/Folder Organizer/Remove from Favorites", false, 2002)]
        private static void Unfavorite() => FolderQuickActions.SetFavorite(FolderQuickActions.SelectedFolderGuids(), false);

        [MenuItem("Assets/Folder Organizer/Reset Color", false, 2003)]
        private static void ResetColor() => FolderQuickActions.SetColor(FolderQuickActions.SelectedFolderGuids(), null);

        [MenuItem("Assets/Folder Organizer/Reset Icon", false, 2004)]
        private static void ResetIcon() => FolderQuickActions.SetIcon(FolderQuickActions.SelectedFolderGuids(), FolderIconId.None);

        [MenuItem("Assets/Folder Organizer/Colors and Icons...", true)]
        [MenuItem("Assets/Folder Organizer/Add to Favorites", true)]
        [MenuItem("Assets/Folder Organizer/Remove from Favorites", true)]
        [MenuItem("Assets/Folder Organizer/Reset Color", true)]
        [MenuItem("Assets/Folder Organizer/Reset Icon", true)]
        private static bool HasSelection() => FolderQuickActions.SelectedFolderGuids().Length > 0;
    }
}
