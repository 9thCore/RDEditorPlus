using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RDEditorPlus.Util
{
    public static class LevelUtil
    {
        public static bool DisableTargetIDWarning { get; private set; } = false;
        public static bool CurrentlyUndoing { get; set; } = false;
        public static bool ForceEventRecull { get; set; } = false;

        public static bool TryLevelLoad(string path,
            out RDLevelSettings settings,
            out List<LevelEvent_MakeRow> rows,
            out List<LevelEvent_MakeSprite> decorations,
            out List<LevelEvent_Base> events,
            out List<Conditional> conditionals,
            out List<BookmarkData> bookmarks,
            out string[] palette)
        {
            if (!File.Exists(path))
            {
                Plugin.LogError($"Error loading level from {path}, as it does not exist");

                settings = default;
                rows = default;
                decorations = default;
                events = default;
                palette = default;
                conditionals = default;
                bookmarks = default;
                return false;
            }

            settings = new(version: 1);
            rows = [];
            decorations = [];
            events = [];
            conditionals = [];
            bookmarks = [];
            palette = RDEditorConstants.defaultColorPalette;

            DisableTargetIDWarning = true;

            try
            {
                var text = RDFile.ReadAllText(path, null);
                var root = Json.Deserialize(text) as Dictionary<string, object>;

                if (root.TryGetValue(RDEditorConstants.SettingsKey, out var settingsObject)
                    && settingsObject is Dictionary<string, object> settingsDict)
                {
                    settings.Decode(settingsDict);
                }

                foreach (var data in GetObjects(root, RDEditorConstants.RowsKey))
                {
                    LevelEvent_MakeRow makeRow = new();
                    makeRow.Decode(data);
                    rows.Add(makeRow);
                }

                foreach (var data in GetObjects(root, RDEditorConstants.DecorationsKey))
                {
                    LevelEvent_MakeSprite makeSprite = new();
                    makeSprite.Decode(data);
                    decorations.Add(makeSprite);
                }

                foreach (var data in GetObjects(root, RDEditorConstants.EventsKey))
                {
                    string type = data[RDEditorConstants.EventsTypeKey] as string;
                    string fullType = RDEditorConstants.LevelEventClassFullName + type;

                    Type eventType = GameAssembly.GetType(fullType);
                    if (eventType == null)
                    {
                        continue;
                    }

                    var instance = Activator.CreateInstance(eventType) as LevelEvent_Base;
                    instance.Decode(data);
                    events.Add(instance);
                }

                foreach (var data in GetObjects(root, RDEditorConstants.ConditionalsKey))
                {
                    conditionals.Add(Conditional.Decode(data));
                }

                foreach (var data in GetObjects(root, RDEditorConstants.BookmarksKey))
                {
                    if (int.TryParse(data["bar"].ToString(), out var bar)
                        && float.TryParse(data["beat"].ToString(), out var beat)
                        && int.TryParse(data["color"].ToString(), out var color))
                    {
                        BarAndBeat position = new(bar, beat);
                        BookmarkData bookmark = new(position, color);
                        bookmarks.Add(bookmark);
                    }
                }

                if (root.TryGetValue(RDEditorConstants.ColorPaletteKey, out var paletteObject)
                    && paletteObject is List<object> paletteList)
                {
                    palette = [.. paletteList.Select(color => color.ToString())];
                }

                DisableTargetIDWarning = false;
                return true;
            }
            catch (Exception ex)
            {
                Plugin.LogError($"Error loading level from {path}:\n{ex}");

                DisableTargetIDWarning = false;
                return false;
            }
        }

        private static IEnumerable<Dictionary<string, object>> GetObjects(Dictionary<string, object> root, string key)
        {
            if (root.TryGetValue(key, out var obj)
                    && obj is List<object> list)
            {
                foreach (var element in list)
                {
                    if (element is Dictionary<string, object> dict)
                    {
                        yield return dict;
                    }
                }
            }
        }

        private static readonly Assembly GameAssembly = typeof(LevelEvent_Base).Assembly;
    }
}
