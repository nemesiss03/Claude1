# Architecture

- FolderOrganizerWindow uses native IMGUI editor controls and follows Project selection changes.
- FolderQuickActions provides independent, undoable multi-folder color, icon, and favorite operations.
- FolderIconPresets resolves Unity editor images where available. FolderCategoryIcons generates original Audio, VFX, Settings, Art and Plugins symbols once, caches them, and disposes them before assembly reload or editor shutdown. No Unity artwork is redistributed.
- FolderAppearanceDatabase stores styles by GUID and invalidates its lookup after serialization and Undo.
- ProjectWindowDrawer draws inside the native icon area. FolderDrawingState is a managed cache. FolderProjectWindowDrawerCache retains its original ScriptableObject base only for disposal of legacy hidden objects.
- FolderStructureService creates missing folders without moving or deleting existing assets.

Everything compiles in an Editor-only assembly. Appearance data is created on first import and excluded from release archives.
