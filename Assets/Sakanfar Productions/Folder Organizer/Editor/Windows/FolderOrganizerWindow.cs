using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SakanfarProductions.FolderOrganizer
{
    /// <summary>Native editor controls for colors, icon presets, favorites and folder creation.</summary>
    public sealed class FolderOrganizerWindow : EditorWindow
    {
        private const string GuidePath = "Assets/Sakanfar Productions/Folder Organizer/README.md";
        private static readonly string[] StarterFolders = { "Scenes", "Scripts", "Prefabs", "Art", "Audio", "UI" };
        private readonly bool[] included = { true, true, true, true, true, true };
        private string rootName = "_Project";
        private string message = "Select folders, then choose a color or icon. Changes apply instantly.";
        private string[] selectedGuids = new string[0];
        private Color customColor = new Color(0.36f, 0.64f, 0.96f);
        private FolderAppearanceDatabase appearance;
        private Vector2 scroll;
        private bool showCreate;
        private GUIStyle heading;
        private GUIStyle iconButton;

        public static FolderOrganizerWindow ShowWindow()
        {
            var window = GetWindow<FolderOrganizerWindow>();
            window.titleContent = new GUIContent("Folder Organizer");
            window.minSize = new Vector2(390, 520);
            window.Show();
            return window;
        }

        public static void OpenForFolder(string guid, int tab = 0)
        {
            var folder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid));
            if (folder != null) Selection.activeObject = folder;
            ShowWindow().RefreshSelection();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Folder Organizer");
            minSize = new Vector2(390, 520);
            appearance = FolderAppearanceDatabase.LoadOrCreate();
            Selection.selectionChanged += RefreshSelection;
            Undo.undoRedoPerformed += RefreshSelection;
            EditorApplication.projectChanged += RefreshSelection;
            RefreshSelection();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= RefreshSelection;
            Undo.undoRedoPerformed -= RefreshSelection;
            EditorApplication.projectChanged -= RefreshSelection;
        }

        // Clear any retained UI Toolkit tree from earlier versions during a live upgrade.
        public void CreateGUI() => rootVisualElement.Clear();

        private void RefreshSelection()
        {
            selectedGuids = FolderQuickActions.SelectedFolderGuids();
            if (appearance == null) appearance = FolderAppearanceDatabase.LoadOrCreate();
            var entry = selectedGuids.Length > 0 ? appearance.Get(selectedGuids[0]) : null;
            if (entry != null && entry.hasColor) customColor = entry.color;
            Repaint();
        }

        private void OnGUI()
        {
            if (appearance == null) appearance = FolderAppearanceDatabase.LoadOrCreate();
            // Use the editor's own font and styles. No external stylesheet or font asset.
            if (heading == null) heading = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18 };
            if (iconButton == null) iconButton = new GUIStyle(GUI.skin.button) { imagePosition = ImagePosition.ImageAbove, fontSize = 11 };
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Folder Organizer", heading, GUILayout.Height(32));
                if (GUILayout.Button("Help", GUILayout.Width(48), GUILayout.Height(24)))
                    EditorUtility.OpenWithDefaultApp(System.IO.Path.GetFullPath(GuidePath));
            }
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawSelection();
            using (new EditorGUI.DisabledScope(selectedGuids.Length == 0))
            {
                DrawColors();
                DrawIcons();
            }
            DrawFavorites();
            DrawCreateFolders();
            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox(message, MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        private void DrawSelection()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                var rect = GUILayoutUtility.GetRect(58, 58, GUILayout.Width(58));
                var path = selectedGuids.Length > 0 ? AssetDatabase.GUIDToAssetPath(selectedGuids[0]) : "Assets";
                var entry = selectedGuids.Length > 0 ? appearance.Get(selectedGuids[0]) : null;
                var folder = AssetDatabase.GetCachedIcon(path);
                if (folder != null)
                {
                    var old = GUI.color;
                    GUI.color = entry != null && entry.hasColor ? entry.color : Color.white;
                    GUI.DrawTexture(new Rect(rect.x + 5, rect.y + 5, 48, 48), folder, ScaleMode.ScaleToFit);
                    GUI.color = old;
                }
                var badge = entry == null ? null : FolderIconPresets.GetTexture(entry.icon);
                if (badge != null) GUI.DrawTexture(new Rect(rect.x + 31, rect.y + 29, 25, 25), badge, ScaleMode.ScaleToFit);
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.Space(7);
                    GUILayout.Label(selectedGuids.Length == 0 ? "Select a folder" : selectedGuids.Length == 1 ? FolderPathUtility.FolderName(path) : selectedGuids.Length + " folders selected", EditorStyles.boldLabel);
                    GUILayout.Label(selectedGuids.Length == 0 ? "Choose one or more folders in the Project window." : path, EditorStyles.wordWrappedMiniLabel);
                    if (selectedGuids.Length > 1) GUILayout.Label("Changes apply to all selected folders.", EditorStyles.miniLabel);
                }
            }
        }

        private void DrawColors()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Folder color", EditorStyles.boldLabel);
                var presets = FolderQuickActions.Palette().ToArray();
                for (var row = 0; row < 2; row++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        for (var column = 0; column < 8; column++)
                        {
                            var preset = presets[row * 8 + column];
                            var rect = GUILayoutUtility.GetRect(24, 28, GUILayout.ExpandWidth(true));
                            if (GUI.Button(rect, new GUIContent("", preset.name))) ApplyColor(preset.color);
                            EditorGUI.DrawRect(new Rect(rect.x + 4, rect.y + 4, rect.width - 8, rect.height - 8), preset.color);
                        }
                    }
                }
                EditorGUI.BeginChangeCheck();
                var chosen = EditorGUILayout.ColorField(new GUIContent("Custom color", "Open Unity's color picker or use its eyedropper."), customColor, true, false, false);
                if (EditorGUI.EndChangeCheck()) ApplyColor(chosen);
                EditorGUI.BeginChangeCheck();
                var hex = EditorGUILayout.DelayedTextField("Hex color", "#" + ColorUtility.ToHtmlStringRGB(customColor));
                if (EditorGUI.EndChangeCheck())
                {
                    if (ColorUtility.TryParseHtmlString(hex.StartsWith("#") ? hex : "#" + hex, out var parsed)) ApplyColor(parsed);
                    else message = "Enter a hex color such as #5C9FF5.";
                }
                if (GUILayout.Button("Reset color", GUILayout.Height(24)))
                {
                    FolderQuickActions.SetColor(selectedGuids, null);
                    message = "Color reset. Your icon and favorites are kept.";
                }
            }
        }

        private void ApplyColor(Color color)
        {
            customColor = color;
            FolderQuickActions.SetColor(selectedGuids, color);
            message = "Color applied. Use Ctrl/Cmd+Z to undo.";
        }

        private void DrawIcons()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Folder icon", EditorStyles.boldLabel);
                var entry = selectedGuids.Length == 1 ? appearance.Get(selectedGuids[0]) : null;
                var choices = FolderIconPresets.Choices;
                const int columns = 4;
                var previousIconSize = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(new Vector2(20, 20));
                for (var row = 0; row < (choices.Length + columns - 1) / columns; row++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        for (var column = 0; column < columns; column++)
                        {
                            var index = row * columns + column;
                            if (index >= choices.Length) break;
                            var id = choices[index];
                            var label = FolderIconPresets.Label(id);
                            var content = new GUIContent(label, FolderIconPresets.GetTexture(id), label);
                            var active = entry != null && entry.icon == id;
                            if (GUILayout.Toggle(active, content, iconButton, GUILayout.Height(48), GUILayout.Width((position.width - 40) / columns)) != active)
                            {
                                FolderQuickActions.SetIcon(selectedGuids, id);
                                message = id == FolderIconId.None ? "Icon removed. Your color is kept." : label + " icon applied. Your color is kept.";
                            }
                        }
                    }
                }
                EditorGUIUtility.SetIconSize(previousIconSize);
            }
        }

        private void DrawFavorites()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Favorites", EditorStyles.boldLabel);
                    using (new EditorGUI.DisabledScope(selectedGuids.Length == 0))
                    {
                        var allFavorites = selectedGuids.Length > 0 && selectedGuids.All(guid => appearance.Get(guid)?.isFavorite == true);
                        if (GUILayout.Button(allFavorites ? "Unfavorite selection" : "Favorite selection", GUILayout.Width(150)))
                            FolderQuickActions.SetFavorite(selectedGuids, !allFavorites);
                    }
                }
                var favorites = appearance.Favorites().ToArray();
                if (favorites.Length == 0) GUILayout.Label("Pin a folder to keep it one click away.", EditorStyles.wordWrappedMiniLabel);
                foreach (var entry in favorites)
                {
                    var path = AssetDatabase.GUIDToAssetPath(entry.guid);
                    var exists = AssetDatabase.IsValidFolder(path);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(!exists))
                            if (GUILayout.Button(new GUIContent(exists ? FolderPathUtility.FolderName(path) : "Missing folder", path)))
                            {
                                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                                Selection.activeObject = asset;
                                EditorGUIUtility.PingObject(asset);
                            }
                        if (GUILayout.Button("Remove", GUILayout.Width(65))) FolderQuickActions.SetFavorite(new[] { entry.guid }, false);
                    }
                }
            }
        }

        private void DrawCreateFolders()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                showCreate = EditorGUILayout.Foldout(showCreate, "Create a project folder structure", true);
                if (!showCreate) return;
                rootName = EditorGUILayout.TextField("Root folder", rootName);
                for (var row = 0; row < 2; row++)
                    using (new EditorGUILayout.HorizontalScope())
                        for (var col = 0; col < 3; col++)
                        {
                            var index = row * 3 + col;
                            included[index] = GUILayout.Toggle(included[index], StarterFolders[index], GUILayout.ExpandWidth(true));
                        }
                GUILayout.Label("Creates missing folders under Assets. Existing files are kept.", EditorStyles.wordWrappedMiniLabel);
                if (GUILayout.Button("Create folders", GUILayout.Height(30))) CreateFolders();
            }
        }

        private void CreateFolders()
        {
            var items = StarterFolders.Where((_, index) => included[index]).Select(path => new FolderTemplateItem(path)).ToArray();
            if (items.Length == 0) { message = "Choose at least one folder to create."; return; }
            var result = FolderStructureService.Create("Assets", rootName, items);
            if (!string.IsNullOrEmpty(result.Error)) { message = result.Error; return; }
            var root = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(result.rootPath);
            if (root != null) { Selection.activeObject = root; EditorGUIUtility.PingObject(root); }
            message = result.created.Count == 0 ? "These folders already exist. Nothing changed." : "Created " + result.created.Count + " folders in " + result.rootPath + ".";
        }
    }
}
