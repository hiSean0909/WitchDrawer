# WitchDrawer Product Manual

This manual describes the features of the latest portable package, version 1.4.7. WitchDrawer is a native Windows desktop file organizer. Extract the complete Windows x64 ZIP and run `WitchDrawer.App.exe`; a separate .NET runtime is not required. A SHA-256 file accompanies the ZIP.

## Box types and purpose

The creation menu explains the four box types:

- **Normal box:** Manages files and folders stored in the application data directory. Modern and pixel icon styles are available.
- **Mapping box:** Organizes absolute references to files and folders that remain in their original locations.
- **Drawer box:** Manages files like a normal box, with an expandable panel for access.
- **Task box:** Provides a desktop to-do list rather than file storage.

## Functions shared across box types

Create, rename, reorder, show, hide, and delete boxes from the application. Move boxes on the desktop, lock their positions, and align them using snapping guides. Box positions and display settings persist between sessions. The app saves and restores layouts using backup slots, and boxes can be brought back to the main display. Auto-hide can reveal all boxes together or reveal a hovered box.

The three file box types support drag and drop from Windows, between boxes, and out to the desktop. Items can be selected individually or as a group, opened, reordered, copied, pasted, renamed, and removed. Context menus can copy a path, open a file location or shortcut target, and run eligible items as administrator. Use `Ctrl+C`, `Ctrl+V`, `Ctrl+Shift+C`, `F2`, and `Delete` for the corresponding file actions.

## Functions that differ by box type

### File storage and removal

Dropping into a normal or drawer box moves the source into managed storage; pasting creates a copy and leaves the source in place. Deleting a stored item or its box restores the file to its original directory, or to the desktop if that directory is missing. Renamed stored files retain their new names when restored. Name conflicts receive numeric suffixes.

Mapping boxes only add or remove references, and renaming a reference changes its display name rather than the source. References do not follow external moves or renames. Missing mapping sources cannot be opened but their references can be removed. Task boxes do not accept files.

In normal and mapping box grid views, a red trash button on each icon removes the item without confirmation or restoration. Normal box files and folders go to the Windows Recycle Bin; mapping boxes remove only the reference and leave the source untouched.

### Display settings

Display Settings are available for every box. Each box can use a custom background color or follow the theme background. File display options depend on type.

| Setting | Normal | Mapping | Drawer | Task |
| --- | --- | --- | --- | --- |
| Icon style | Yes | Yes | No | No |
| Icon size | Yes | Yes | Yes | No |
| Expand on hover | Yes | Yes | No | No |
| Show or hide box title | Yes | Yes | Yes | No |
| Show or hide file names | Yes | Yes | Yes | No |
| Automatic size or fixed grid | Yes | Yes | No | No |
| Background color | Yes | Yes | Yes | Yes |

Normal and mapping boxes support modern and pixel styles. A fixed grid has configurable rows and columns and stops accepting imports when full. Normal and mapping boxes can be rolled up and optionally expanded on hover. Mapping boxes offer grid and list views; grid file names are shown by default and can be hidden, while the list always shows names. Drawer boxes use an expandable file panel. Drawer covers, mapping lists, and task panels can be resized using their desktop handles.

### Tasks and archive

Task boxes allow users to add and edit tasks, mark them complete or incomplete, and archive completed tasks. They display remaining tasks and completion progress; the completion rate includes all unarchived tasks regardless of date. The archive page displays archived tasks and supports restoring or deleting them. A deleted individual task can be undone within 10 seconds while the application remains running. Deleting a task box also deletes its archived history and cannot be undone.

## Desktop appearance

Desktop icon visibility can be toggled, optionally by double-clicking empty desktop space.

Choose the Light, Obsidian, or Crystal theme and customize colors, corner shapes, hover and selection appearance, and opacity for boxes, borders, and icon backgrounds. Custom styles are saved per theme, can be previewed, and can be reset to the preset. The editor can follow desktop box opacity.

## Search and access

The quick panel searches items across boxes by file name, path, or box name and opens matching items. Its default global shortcut is `Ctrl+Alt+W`; a different shortcut can be set in Settings. The system tray provides access to the main window, boxes, and exit. Startup at sign-in is configurable.

## Plugins

The plugin center supports optional native desktop widgets. Refresh the catalog, install an available plugin or import a local ZIP, and enable, disable, configure, update, or uninstall it. Installed plugins can work offline; updates require the plugin to be disabled. Plugin permissions are descriptions of requested capabilities, not a sandbox: plugins run with the application's process permissions. Widgets can remember their position and size. Sample widgets are optional and are not automatically installed with the application.

## Settings, updates, and diagnostics

The interface supports English and Simplified Chinese. The language can be changed immediately and is saved; first launch follows the system UI language, falling back to English.

The About page can check for updates and download an available update, with cancellation support. Diagnostic logs can be exported as a ZIP containing recent logs and basic diagnostic information, without the database or user file contents.

The default data directory is `%LocalAppData%\WitchDrawer\`, including for the portable package. It contains the SQLite database, managed box files, logs, and plugin data. Settings can migrate data to an empty directory; restart applies the change and the original directory remains as a backup. `WITCHDRAWER_DATA_DIR` overrides the configured data location.

For desktop drag and drop from ordinary Windows applications, run WitchDrawer without administrator privileges. Windows restricts drops from non-elevated applications into an elevated WitchDrawer process.
