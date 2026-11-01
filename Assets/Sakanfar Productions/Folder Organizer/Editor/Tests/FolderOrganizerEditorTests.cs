using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace SakanfarProductions.FolderOrganizer.Tests
{
    public sealed class FolderOrganizerEditorTests
    {
        private string testRoot;
        private UnityEngine.Object[] previousSelection;

        [SetUp]
        public void SetUp()
        {
            previousSelection = Selection.objects;
            testRoot = "Assets/FolderOrganizerTests_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", testRoot.Substring("Assets/".Length));
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            Selection.objects = previousSelection;
            var database = FolderAppearanceDatabase.LoadOrCreate();
            database.Remove(AssetDatabase.AssetPathToGUID(testRoot));
            database.Remove(AssetDatabase.AssetPathToGUID(testRoot + "/Second"));
            database.MarkChanged();
            AssetDatabase.SaveAssets();
            if (AssetDatabase.IsValidFolder(testRoot)) AssetDatabase.DeleteAsset(testRoot);
            AssetDatabase.Refresh();
        }

        [Test]
        public void FolderCreationCreatesNestedFolders()
        {
            var result = FolderStructureService.Create(testRoot, "Root", new[] { new FolderTemplateItem("Art/Textures"), new FolderTemplateItem("Scenes") });
            Assert.IsNull(result.Error);
            Assert.IsTrue(AssetDatabase.IsValidFolder(testRoot + "/Root/Art/Textures"));
            Assert.IsTrue(AssetDatabase.IsValidFolder(testRoot + "/Root/Scenes"));
        }

        [Test]
        public void DuplicateFolderPreventionSkipsExistingFolders()
        {
            AssetDatabase.CreateFolder(testRoot, "Root");
            AssetDatabase.Refresh();
            var result = FolderStructureService.Create(testRoot, "Root", new[] { new FolderTemplateItem("Scenes") });
            Assert.IsNull(result.Error);
            Assert.IsTrue(result.skipped.Contains(testRoot + "/Root"));
            Assert.IsTrue(AssetDatabase.IsValidFolder(testRoot + "/Root/Scenes"));
        }

        [Test]
        public void PathValidationRejectsOutsideAndTraversalPaths()
        {
            Assert.IsTrue(FolderPathUtility.IsValidProjectFolderPath("Assets"));
            Assert.IsFalse(FolderPathUtility.IsValidProjectFolderPath("Packages"));
            Assert.IsFalse(FolderPathUtility.IsSafeRelativeFolderName("../Secrets"));
            Assert.IsFalse(FolderPathUtility.IsSafeRelativeFolderName("Art/Textures"));
        }

        [Test]
        public void AppearanceDataRoundTripsAndResetRemovesEntry()
        {
            var database = ScriptableObject.CreateInstance<FolderAppearanceDatabase>();
            var entry = database.GetOrCreate("test-guid", "Assets/Test");
            entry.hasColor = true; entry.color = Color.cyan; entry.icon = FolderIconId.Scenes; entry.isFavorite = true;
            Assert.AreSame(entry, database.Get("test-guid"));
            var serialized = EditorJsonUtility.ToJson(database);
            var loaded = ScriptableObject.CreateInstance<FolderAppearanceDatabase>();
            EditorJsonUtility.FromJsonOverwrite(serialized, loaded);
            Assert.IsTrue(loaded.Get("test-guid").hasColor);
            Assert.AreEqual(FolderIconId.Scenes, loaded.Get("test-guid").icon);
            database.Remove("test-guid");
            Assert.IsNull(database.Get("test-guid"));
            UnityEngine.Object.DestroyImmediate(loaded);
            UnityEngine.Object.DestroyImmediate(database);
        }

        [Test]
        public void DrawingCacheResetsAndReloadsProjectDataWithoutNativeObjectIds()
        {
            var cacheType = typeof(ProjectWindowDrawer).Assembly.GetType(
                "SakanfarProductions.FolderOrganizer.FolderDrawingState", true);
            Assert.IsFalse(typeof(UnityEngine.Object).IsAssignableFrom(cacheType));
            var cache = Activator.CreateInstance(cacheType, true);
            FolderAppearanceDatabase.LoadOrCreate();
            cacheType.GetMethod("EnsureReady").Invoke(cache, null);
            Assert.IsNotNull(cacheType.GetProperty("Appearance").GetValue(cache));
            cacheType.GetMethod("Reset").Invoke(cache, null);
            Assert.IsNull(cacheType.GetProperty("Appearance").GetValue(cache));
            cacheType.GetMethod("EnsureReady").Invoke(cache, null);
            Assert.IsNotNull(cacheType.GetProperty("Appearance").GetValue(cache));
        }

        [UnityTest]
        public IEnumerator WindowTracksSelectionAndDisablesActionsForFiles()
        {
            var window = ScriptableObject.CreateInstance<FolderOrganizerWindow>();
            try
            {
                Selection.objects = new UnityEngine.Object[0];
                window.Show();
                window.CreateGUI();
                var selectionField = typeof(FolderOrganizerWindow).GetField("selectedGuids", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                yield return null;
                Assert.IsEmpty((string[])selectionField.GetValue(window));
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(testRoot);
                yield return null;
                CollectionAssert.Contains((string[])selectionField.GetValue(window), AssetDatabase.AssetPathToGUID(testRoot));
                typeof(FolderOrganizerWindow).GetField("showCreate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(window, true);
                window.SendEvent(new Event { type = EventType.Layout });
                window.SendEvent(new Event { type = EventType.Repaint });
                Assert.IsNotNull(typeof(FolderOrganizerWindow).GetField("heading", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(window), "The native OnGUI path must execute.");
                Assert.IsNotNull(typeof(FolderOrganizerWindow).GetField("iconButton", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(window));
                Selection.activeObject = MonoScript.FromScriptableObject(window);
                yield return null;
                Assert.IsEmpty((string[])selectionField.GetValue(window));
            }
            finally { UnityEngine.Object.DestroyImmediate(window); }
        }

        [Test]
        public void LegacyNativeCacheIsDisposedWithoutTouchingOtherObjects()
        {
            var type = typeof(ProjectWindowDrawer).Assembly.GetType("SakanfarProductions.FolderOrganizer.FolderProjectWindowDrawerCache", true);
            Assert.IsTrue(typeof(ScriptableObject).IsAssignableFrom(type));
            var legacy = ScriptableObject.CreateInstance(type);
            var unrelated = ScriptableObject.CreateInstance<FolderAppearanceDatabase>();
            try
            {
                legacy.hideFlags = HideFlags.HideAndDontSave;
                typeof(ProjectWindowDrawer).GetMethod("RemoveLegacyCache", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(null, null);
                Assert.IsTrue(legacy == null);
                Assert.IsTrue(unrelated != null);
            }
            finally { if (legacy != null) UnityEngine.Object.DestroyImmediate(legacy); UnityEngine.Object.DestroyImmediate(unrelated); }
        }

        [Test]
        public void IconPresetsResolveToImages()
        {
            foreach (var preset in FolderIconPresets.Choices)
                if (preset != FolderIconId.None) Assert.IsNotNull(FolderIconPresets.GetTexture(preset), preset.ToString());
            Assert.AreNotSame(FolderIconPresets.GetTexture(FolderIconId.Scripts), FolderIconPresets.GetTexture(FolderIconId.Prefabs));
        }

        [Test]
        public void CategoryPresetsHaveDistinctIconsInsteadOfDocumentFallbacks()
        {
            var categories = new[] { FolderIconId.Audio, FolderIconId.VFX, FolderIconId.Settings, FolderIconId.Art, FolderIconId.Plugins };
            var textures = new System.Collections.Generic.HashSet<Texture>();
            var fallback = EditorGUIUtility.ObjectContent(null, typeof(DefaultAsset)).image;
            foreach (var category in categories)
            {
                var texture = FolderIconPresets.GetTexture(category);
                Assert.IsNotNull(texture, category.ToString());
                Assert.AreNotSame(fallback, texture, category.ToString());
                Assert.IsTrue(textures.Add(texture), "Category icons must be distinct.");
                Assert.AreSame(texture, FolderIconPresets.GetTexture(category), "Repeated draws should reuse the texture.");
            }
        }

        [Test]
        public void ColorAndIconResetsAreIndependent()
        {
            var guids = new[] { AssetDatabase.AssetPathToGUID(testRoot) };
            var database = FolderAppearanceDatabase.LoadOrCreate();
            FolderQuickActions.SetIcon(guids, FolderIconId.Scripts);
            FolderQuickActions.SetColor(guids, Color.red);
            Assert.AreEqual(FolderIconId.Scripts, database.Get(guids[0]).icon);
            FolderQuickActions.SetColor(guids, null);
            Assert.AreEqual(FolderIconId.Scripts, database.Get(guids[0]).icon);
            FolderQuickActions.SetFavorite(guids, false);
            Assert.AreEqual(FolderIconId.Scripts, database.Get(guids[0]).icon);
            FolderQuickActions.SetColor(guids, Color.blue);
            FolderQuickActions.SetIcon(guids, FolderIconId.None);
            Assert.AreEqual(Color.blue, database.Get(guids[0]).color);
            Assert.IsTrue(database.Get(guids[0]).hasColor);
        }

        [Test]
        public void BulkColorResetPreservesFavoritesAndUndoRestoresColor()
        {
            AssetDatabase.CreateFolder(testRoot, "Second");
            var guids = new[] { AssetDatabase.AssetPathToGUID(testRoot), AssetDatabase.AssetPathToGUID(testRoot + "/Second") };
            var database = FolderAppearanceDatabase.LoadOrCreate();
            FolderQuickActions.SetColor(guids, Color.cyan);
            FolderQuickActions.SetFavorite(guids, true);
            Undo.FlushUndoRecordObjects();
            Undo.IncrementCurrentGroup();
            FolderQuickActions.SetColor(guids, null);
            foreach (var guid in guids) { Assert.IsFalse(database.Get(guid).hasColor); Assert.IsTrue(database.Get(guid).isFavorite); }
            Undo.FlushUndoRecordObjects();
            Undo.PerformUndo();
            foreach (var guid in guids) { Assert.IsTrue(database.Get(guid).hasColor); Assert.AreEqual(Color.cyan, database.Get(guid).color); }
        }

        [TestCase(100f, 16f)]
        [TestCase(64f, 80f)]
        [TestCase(32f, 48f)]
        public void IconDrawingStaysOutsideFolderLabel(float width, float height)
        {
            var method = typeof(ProjectWindowDrawer).GetMethod("GetIconRect",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var item = new Rect(20, 30, width, height);
            var icon = (Rect)method.Invoke(null, new object[] { item });
            Assert.GreaterOrEqual(icon.xMin, item.xMin);
            Assert.LessOrEqual(icon.xMax, item.xMax);
            Assert.GreaterOrEqual(icon.yMin, item.yMin);
            Assert.LessOrEqual(icon.yMax, height > 30 ? item.yMax - 18 : item.yMax);
            if (height <= 30) Assert.AreEqual(16, icon.width);
        }

        [Test]
        public void FolderGuidResolvesToCurrentPath()
        {
            var guid = AssetDatabase.AssetPathToGUID(testRoot);
            Assert.IsFalse(string.IsNullOrEmpty(guid));
            Assert.AreEqual(testRoot, FolderPathUtility.ResolveGuid(guid));
        }
    }
}
