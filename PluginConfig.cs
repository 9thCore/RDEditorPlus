using BepInEx;
using BepInEx.Configuration;
using RDEditorPlus.ExtraData;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RDEditorPlus
{
    internal class PluginConfig
    {
        private static PluginConfig instance;
        public static PluginConfig Instance
        {
            get
            {
                instance ??= new PluginConfig();
                return instance;
            }
        }

        internal static readonly ConfigFile config = new(Path.Combine(Paths.ConfigPath, "RDEditorPlus.cfg"), true);

        public const string CATEGORY_SUBROWS = "SubRows";
        public const string CATEGORY_CUSTOMMETHODS = "CustomMethods";
        public const string CATEGORY_ROWS = "Rows";
        public const string CATEGORY_SELECT = "Select";
        public const string CATEGORY_WINDOWS = "Windows";

        public const string PATCH_SUB_ROWS_BASE_LEFT = "Patch a lot of things to allow multiple timeline rows for ";
        public const string PATCH_SUB_ROWS_BASE_RIGHT = ".\nMay cause incompatibilies with other mods, and is not guaranteed to be stable.";

        public const string PATCH_SUB_ROWS_TOGGLE = "Toggle for all multiple timeline row functionality.\nIf disabled, none of the patches below will be applied.";
        public const string PATCH_SUB_ROWS_SPRITE = PATCH_SUB_ROWS_BASE_LEFT + "decorations" + PATCH_SUB_ROWS_BASE_RIGHT;
        public const string PATCH_SUB_ROWS_PATIENT = PATCH_SUB_ROWS_BASE_LEFT + "patients" + PATCH_SUB_ROWS_BASE_RIGHT;
        public const string PATCH_SUB_ROWS_ROOM = PATCH_SUB_ROWS_BASE_LEFT + "rooms" + PATCH_SUB_ROWS_BASE_RIGHT;
        public const string PATCH_SUB_ROWS_WINDOW = PATCH_SUB_ROWS_BASE_LEFT + "windows" + PATCH_SUB_ROWS_BASE_RIGHT;
        public const string PATCH_SUB_ROWS_TALL_EVENTS = "What to do in the case of a tall event, like Show Rooms or Reorder Windows.\n" +
            "Will only take effect if there is a tall event in the selected tabs.\n" +
            nameof(SubRowTallEventBehaviour.KeepInSpecialRow) + " will add a special, non-expandable row at the top of the timeline where they will be put, scaled down to fit on a single row.";
        public const string PATCH_SUB_ROWS_ALTERNATING_COLORS = "If the timeline should have alternating colors to help with lining up events on a specific element.";
        public const string PATCH_SUB_ROWS_SCALE_PREVIEW = "If the room previews should be moved and scaled to take up more space the more rows it has.";
        public const string PATCH_SUB_ROWS_SCALE_PREVIEW_MINIMUM = "The minimum amount of rows the preview must have to be visible.\n" +
            "If set to 1 or less, the preview will be visible at all times.";

        public const string PATCH_CUSTOM_METHODS_TOGGLE = "Toggle for all custom method mod functionality.\nIf disabled, none of the patches below will be applied.";
        public const string PATCH_CUSTOM_METHODS_AUTOCOMPLETE = "Toggle for the additional custom method autocompletion.\n" +
            "If enabled, there will be more custom methods available for autocompletion, but they will have a deep red color and will not have descriptions.\n" +
            "If set to " + nameof(CustomMethodAutocompleteBehaviour.Disabled) + ", will not be enabled.\n" +
            "If set to " + nameof(CustomMethodAutocompleteBehaviour.RequestFromWeb) + ", will request from " + CustomMethodStorage.CustomMethodsSpreadsheetURL + ", the public database of known custom methods by the community, then save to a temporary file which is occassionally updated.\n" +
            "If set to " + nameof(CustomMethodAutocompleteBehaviour.FetchFromFile) + ", will fetch from the file located at \"BepInEx/plugins/RDEditorPlus/" + CustomMethodStorage.CustomMethodsSpreadsheetFile + "\", assuming it exists. This file must be supplied by the user (the mod will not create it) and should be in TSV (tab-separated values) format.";
        public const string PATCH_CUSTOM_METHODS_AUTOCOMPLETE_REFRESH_TIME = "How many days old the temporary download file (at \"BepInEx/plugins/RDEditorPlus/" + CustomMethodStorage.CustomMethodsSpreadsheetDownloadedFile + "\") should be, before trying to request another.\n" +
            "Only does something if " + nameof(customMethodsAutocomplete) + " is set to " + nameof(CustomMethodAutocompleteBehaviour.RequestFromWeb) + ".\n" +
            "The file will only be requested once the editor is loaded, even if more than the specified amount of days have passed.\n" +
            "The file will be re-downloaded if it does not already exist.";

        public const string PATCH_ROW_TOGGLE = "Toggle for all row tab (patients) functionality.\nIf disabled, none of the patches below will be applied.";
        public const string PATCH_ROW_BEAT_SWITCH = "Whether a button that switches the selected beat from a classic beat to a oneshot beat (or vice-versa) should be added to their respective inspectors.";

        public const string PATCH_SELECT_TOGGLE = "Toggle for all selection mod functionality.\nIf disabled, none of the patches below will be applied.";
        public const string PATCH_SELECT_MULTI = "Whether the inspector should work with multiple selected events.\n" +
            "All events must be of the same type, and some events may have other conditions.";
        public const string PATCH_SELECT_MULTI_COLOR = "How the color property should be mixed if it differs between selected events.\n" +
            "If set to " + nameof(MultiEditColorBehaviour.JustSetToWhite) + ", it will be simply set to white.\n" +
            "If set to " + nameof(MultiEditColorBehaviour.AverageRGB) + ", it will be the average of all the color's red, green and blue components, while keeping its alpha component 1.\n" +
            "If set to " + nameof(MultiEditColorBehaviour.AverageRGBA) + ", it will be the average of all the color's red, green, blue and alpha components.\n" +
            "If set to " + nameof(MultiEditColorBehaviour.AverageHSV) + ", it will be the average of all the color's hue, saturation and value, while keeping its alpha component 1.\n" +
            "If set to " + nameof(MultiEditColorBehaviour.AverageHSVA) + ", it will be the average of all the color's hue, saturation, value and alpha components.";

        public const string PATCH_WINDOWS_TOGGLE = "Toggle for all window functionality.\nIf disabled, none of the patches below will be applied.";
        public const string PATCH_WINDOWS_MORE = "If it should be possible to add new windows using a new plus button, similar to the patient and sprite tabs.\n" +
            "Extra windows can, afterwards, be removed with a button added to the window.";
        public const string PATCH_WINDOWS_REORDER = "How the reorder event should look in the timeline.\n" +
            "This option is ignored if the the window multiple timeline row system is enabled and " + nameof(subRowTallEventBehaviour) + " is " + nameof(SubRowTallEventBehaviour.KeepFourRowsHigh) + " or " + nameof(SubRowTallEventBehaviour.KeepInSpecialRow) + ".\n" +
            "If set to " + nameof(MoreWindowsReorderBehaviour.ShowOrder) + ", it will show the order of the given window, where the window with \"1\" will be in front of the others.\n" +
            "If set to " + nameof(MoreWindowsReorderBehaviour.ShowData) + ", it will show the window indices as they are placed in the event, where the window with the topmost index will be in front of the others.";

        public static bool SubRowsEnabled => Instance.subRows.Value;
        public static bool SpriteSubRowsEnabled => Instance.spriteSubRows.Value;
        public static bool PatientSubRowsEnabled => Instance.patientSubRows.Value;
        public static bool RoomSubRowsEnabled => Instance.roomSubRows.Value;
        public static bool WindowSubRowsEnabled => Instance.windowSubRows.Value;
        public static SubRowTallEventBehaviour TallEventSubRowsBehaviour => Instance.subRowTallEventBehaviour.Value;
        public static bool AlternatingColorSubRowsEnabled => Instance.alternatingColorSubRows.Value;
        public static bool PreviewScaleSubRowsEnabled => Instance.previewScaleSubRows.Value;
        public static int PreviewScaleSubRowsMinimum => Instance.previewScaleMinimumSubRows.Value;

        public static bool CustomMethodsEnabled => Instance.customMethods.Value;
        public static CustomMethodAutocompleteBehaviour CustomMethodsAutocomplete => Instance.customMethodsAutocomplete.Value;
        public static int CustomMethodsAutocompleteRefreshTime => Instance.customMethodsAutocompleteRefreshTime.Value;

        public static bool RowsEnabled => Instance.rows.Value;
        public static RowBeatSwitchBehaviour RowBeatSwitch => Instance.rowBeatSwitch.Value;

        public static bool SelectionEnabled => Instance.selection.Value;
        public static bool SelectionMultiEditEnabled => Instance.selectionMultiEdit.Value;
        public static MultiEditColorBehaviour SelectionMultiEditColorBehaviour => Instance.selectionMultiEditColor.Value;

        public static bool WindowsEnabled => Instance.windows.Value;
        public static bool WindowsMoreEnabled => Instance.windowsMore.Value;
        public static MoreWindowsReorderBehaviour WindowsReorderBehaviour => Instance.windowsReorderBehaviour.Value;


#pragma warning disable 0649
        [Category(CATEGORY_SUBROWS)]
        [Config<bool>(PATCH_SUB_ROWS_TOGGLE, false)]
        public readonly ConfigEntry<bool> subRows;

        [Config<bool>(PATCH_SUB_ROWS_SPRITE, true)]
        public readonly ConfigEntry<bool> spriteSubRows;

        [Config<bool>(PATCH_SUB_ROWS_PATIENT, true)]
        public readonly ConfigEntry<bool> patientSubRows;

        [Config<bool>(PATCH_SUB_ROWS_ROOM, true)]
        public readonly ConfigEntry<bool> roomSubRows;

        [Config<bool>(PATCH_SUB_ROWS_WINDOW, true)]
        public readonly ConfigEntry<bool> windowSubRows;

        [Config<SubRowTallEventBehaviour>(PATCH_SUB_ROWS_TALL_EVENTS, SubRowTallEventBehaviour.ExpandToTimelineHeight)]
        public readonly ConfigEntry<SubRowTallEventBehaviour> subRowTallEventBehaviour;

        [Config<bool>(PATCH_SUB_ROWS_ALTERNATING_COLORS, true)]
        public readonly ConfigEntry<bool> alternatingColorSubRows;

        [Config<bool>(PATCH_SUB_ROWS_SCALE_PREVIEW, false)]
        public readonly ConfigEntry<bool> previewScaleSubRows;

        [Config<int>(PATCH_SUB_ROWS_SCALE_PREVIEW_MINIMUM, 2)]
        public readonly ConfigEntry<int> previewScaleMinimumSubRows;


        [Category(CATEGORY_CUSTOMMETHODS)]
        [Config<bool>(PATCH_CUSTOM_METHODS_TOGGLE, false)]
        public readonly ConfigEntry<bool> customMethods;

        [Config<CustomMethodAutocompleteBehaviour>(PATCH_CUSTOM_METHODS_AUTOCOMPLETE, CustomMethodAutocompleteBehaviour.Disabled)]
        public readonly ConfigEntry<CustomMethodAutocompleteBehaviour> customMethodsAutocomplete;

        [Config<int>(PATCH_CUSTOM_METHODS_AUTOCOMPLETE_REFRESH_TIME, 30)]
        public readonly ConfigEntry<int> customMethodsAutocompleteRefreshTime;


        [Category(CATEGORY_ROWS)]
        [Config<bool>(PATCH_ROW_TOGGLE, false)]
        public readonly ConfigEntry<bool> rows;

        [Config<RowBeatSwitchBehaviour>(PATCH_ROW_BEAT_SWITCH, RowBeatSwitchBehaviour.Disabled)]
        public readonly ConfigEntry<RowBeatSwitchBehaviour> rowBeatSwitch;


        [Category(CATEGORY_SELECT)]
        [Config<bool>(PATCH_SELECT_TOGGLE, false)]
        public readonly ConfigEntry<bool> selection;

        [Config<bool>(PATCH_SELECT_MULTI, false)]
        public readonly ConfigEntry<bool> selectionMultiEdit;

        [Config<MultiEditColorBehaviour>(PATCH_SELECT_MULTI_COLOR, MultiEditColorBehaviour.JustSetToWhite)]
        public readonly ConfigEntry<MultiEditColorBehaviour> selectionMultiEditColor;


        [Category(CATEGORY_WINDOWS)]
        [Config<bool>(PATCH_WINDOWS_TOGGLE, false)]
        public readonly ConfigEntry<bool> windows;

        [Config<bool>(PATCH_WINDOWS_MORE, true)]
        public readonly ConfigEntry<bool> windowsMore;

        [Config<MoreWindowsReorderBehaviour>(PATCH_WINDOWS_REORDER, MoreWindowsReorderBehaviour.ShowOrder)]
        public readonly ConfigEntry<MoreWindowsReorderBehaviour> windowsReorderBehaviour;
#pragma warning restore 0649


        public void Register()
        {
            string category = string.Empty;

            var pairs = typeof(PluginConfig).GetFields()
                .Select(info => new FieldAttributePair(info, info.GetCustomAttribute<ConfigAttribute>()))
                .Where(pair => pair.Attribute != null)
                .OrderBy(pair => pair.Attribute.Order);

            foreach (var pair in pairs)
            {
                var categoryFetch = pair.Field.GetCustomAttribute<CategoryAttribute>();
                if (categoryFetch != null)
                {
                    category = categoryFetch.Category;
                }

                pair.Bind(config, category);
            }
        }

        public enum SubRowTallEventBehaviour
        {
            KeepFourRowsHigh,
            ExpandToTimelineHeight,
            KeepInSpecialRow
        }

        public enum CustomMethodAutocompleteBehaviour
        {
            Disabled,
            RequestFromWeb,
            FetchFromFile
        }

        public enum RowBeatSwitchBehaviour
        {
            Disabled,
            KeepTotalBeatLength,
            KeepTickLengthOnly,
            ResetToDefault
        }

        public enum MultiEditColorBehaviour
        {
            JustSetToWhite = 0,
            AverageRGB = 2,
            AverageRGBA = AverageRGB | AverageAlphaFragment,
            AverageHSV = 4,
            AverageHSVA = AverageHSV | AverageAlphaFragment
        }

        public const MultiEditColorBehaviour AverageAlphaFragment = (MultiEditColorBehaviour) 1;

        public enum MoreWindowsReorderBehaviour
        {
            ShowOrder,
            ShowData
        }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
        private abstract class ConfigAttribute(int order) : Attribute
        {
            public readonly int Order = order;

            public abstract void Bind(ConfigFile config, FieldInfo field, string category);
        }

        private class ConfigAttribute<T>(string description, T defaultValue, [CallerLineNumber] int order = 0) : ConfigAttribute(order)
        {
            public readonly string Description = description;
            public readonly T Default = defaultValue;

            public override void Bind(ConfigFile config, FieldInfo field, string category)
            {
                field.SetValue(instance, config.Bind(category, field.Name, Default, Description));
            }
        }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
        private class CategoryAttribute(string category) : Attribute
        {
            public readonly string Category = category;
        }

        private record FieldAttributePair(FieldInfo Field, ConfigAttribute Attribute)
        {
            public void Bind(ConfigFile config, string category) => Attribute.Bind(config, Field, category);
        }
    }
}
