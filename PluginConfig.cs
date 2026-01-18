using BepInEx;
using BepInEx.Configuration;
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

        public static bool SubRowsEnabled => Instance.subRows.Value;
        public static bool SpriteSubRowsEnabled => Instance.spriteSubRows.Value;
        public static bool PatientSubRowsEnabled => Instance.patientSubRows.Value;
        public static bool RoomSubRowsEnabled => Instance.roomSubRows.Value;
        public static bool WindowSubRowsEnabled => Instance.windowSubRows.Value;
        public static SubRowTallEventBehaviour TallEventSubRowsBehaviour => Instance.subRowTallEventBehaviour.Value;
        public static bool AlternatingColorSubRowsEnabled => Instance.alternatingColorSubRows.Value;

        public readonly ConfigEntry<bool> subRows;
        public readonly ConfigEntry<bool> spriteSubRows;
        public readonly ConfigEntry<bool> patientSubRows;
        public readonly ConfigEntry<bool> roomSubRows;
        public readonly ConfigEntry<bool> windowSubRows;
        public readonly ConfigEntry<SubRowTallEventBehaviour> subRowTallEventBehaviour;
        public readonly ConfigEntry<bool> alternatingColorSubRows;

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
    }
}
