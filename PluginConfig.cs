using BepInEx;
using BepInEx.Configuration;
using RDEditorPlus.ExtraData;
using System;
using System.IO;
using UnityEngine;

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
            "If set to " + nameof(CustomMethodAutocompleteBehaviour.RequestFromWeb) + ", will request from " + CustomMethodStorage.CustomMethodsSpreadsheetURL + ", the public database of known custom methods by the community.\n" +
            "If set to " + nameof(CustomMethodAutocompleteBehaviour.FetchFromFile) + ", will fetch from the file located at \"BepInEx/plugins/RDEditorPlus/" + CustomMethodStorage.CustomMethodsSpreadsheetFile + "\", assuming it exists. This file must be supplied by the user (the mod will not create it) and should be in TSV (tab-separated values) format.";

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

        public static bool RowsEnabled => Instance.rows.Value;
        public static RowBeatSwitchBehaviour RowBeatSwitch => Instance.rowBeatSwitch.Value;

        public static bool SelectionEnabled => Instance.selection.Value;
        public static bool SelectionMultiEditEnabled => Instance.selectionMultiEdit.Value;
        public static MultiEditColorBehaviour SelectionMultiEditColorBehaviour => Instance.selectionMultiEditColor.Value;

        public readonly ConfigEntry<bool> subRows;
        public readonly ConfigEntry<bool> spriteSubRows;
        public readonly ConfigEntry<bool> patientSubRows;
        public readonly ConfigEntry<bool> roomSubRows;
        public readonly ConfigEntry<bool> windowSubRows;
        public readonly ConfigEntry<SubRowTallEventBehaviour> subRowTallEventBehaviour;
        public readonly ConfigEntry<bool> alternatingColorSubRows;
        public readonly ConfigEntry<bool> previewScaleSubRows;
        public readonly ConfigEntry<int> previewScaleMinimumSubRows;

        public readonly ConfigEntry<bool> customMethods;
        public readonly ConfigEntry<CustomMethodAutocompleteBehaviour> customMethodsAutocomplete;

        public readonly ConfigEntry<bool> rows;
        public readonly ConfigEntry<RowBeatSwitchBehaviour> rowBeatSwitch;

        public readonly ConfigEntry<bool> selection;
        public readonly ConfigEntry<bool> selectionMultiEdit;
        public readonly ConfigEntry<MultiEditColorBehaviour> selectionMultiEditColor;

        public PluginConfig()
        {
            subRows = config.Bind(
                CATEGORY_SUBROWS,
                nameof(subRows),
                false,
                PATCH_SUB_ROWS_TOGGLE);

            spriteSubRows = config.Bind(
                CATEGORY_SUBROWS,
                nameof(spriteSubRows),
                true,
                PATCH_SUB_ROWS_SPRITE);

            patientSubRows = config.Bind(
                CATEGORY_SUBROWS,
                nameof(patientSubRows),
                true,
                PATCH_SUB_ROWS_PATIENT);

            roomSubRows = config.Bind(
                CATEGORY_SUBROWS,
                nameof(roomSubRows),
                true,
                PATCH_SUB_ROWS_ROOM);

            windowSubRows = config.Bind(
                CATEGORY_SUBROWS,
                nameof(windowSubRows),
                true,
                PATCH_SUB_ROWS_WINDOW);

            subRowTallEventBehaviour = config.Bind(
                CATEGORY_SUBROWS,
                nameof(subRowTallEventBehaviour),
                SubRowTallEventBehaviour.ExpandToTimelineHeight,
                PATCH_SUB_ROWS_TALL_EVENTS);

            alternatingColorSubRows = config.Bind(
                CATEGORY_SUBROWS,
                nameof(alternatingColorSubRows),
                true,
                PATCH_SUB_ROWS_ALTERNATING_COLORS);

            previewScaleSubRows = config.Bind(
                CATEGORY_SUBROWS,
                nameof(previewScaleSubRows),
                false,
                PATCH_SUB_ROWS_SCALE_PREVIEW);

            previewScaleMinimumSubRows = config.Bind(
                CATEGORY_SUBROWS,
                nameof(previewScaleMinimumSubRows),
                2,
                PATCH_SUB_ROWS_SCALE_PREVIEW_MINIMUM);


            customMethods = config.Bind(
                CATEGORY_CUSTOMMETHODS,
                nameof(customMethods),
                false,
                PATCH_CUSTOM_METHODS_TOGGLE);

            customMethodsAutocomplete = config.Bind(
                CATEGORY_CUSTOMMETHODS,
                nameof(customMethodsAutocomplete),
                CustomMethodAutocompleteBehaviour.Disabled,
                PATCH_CUSTOM_METHODS_AUTOCOMPLETE);

            rows = config.Bind(
                CATEGORY_ROWS,
                nameof(rows),
                false,
                PATCH_ROW_TOGGLE);

            rowBeatSwitch = config.Bind(
                CATEGORY_ROWS,
                nameof(rowBeatSwitch),
                RowBeatSwitchBehaviour.Disabled,
                PATCH_ROW_BEAT_SWITCH);

            selection = config.Bind(
                CATEGORY_SELECT,
                nameof(selection),
                false,
                PATCH_SELECT_TOGGLE);

            selectionMultiEdit = config.Bind(
                CATEGORY_SELECT,
                nameof(selectionMultiEdit),
                false,
                PATCH_SELECT_MULTI);

            selectionMultiEditColor = config.Bind(
                CATEGORY_SELECT,
                nameof(selectionMultiEditColor),
                MultiEditColorBehaviour.JustSetToWhite,
                PATCH_SELECT_MULTI_COLOR);
        }

        public void Noop()
        {
            // Just to create the singleton lol
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
    }
}
