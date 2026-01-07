using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public const string PATCH_SUB_ROWS_SPRITE = PATCH_SUB_ROWS_BASE_LEFT + "decorations" + PATCH_SUB_ROWS_BASE_RIGHT;
        public const string PATCH_SUB_ROWS_PATIENT = PATCH_SUB_ROWS_BASE_LEFT + "patients" + PATCH_SUB_ROWS_BASE_RIGHT;
        public const string PATCH_SUB_ROWS_ROOM = PATCH_SUB_ROWS_BASE_LEFT + "rooms" + PATCH_SUB_ROWS_BASE_RIGHT;
        public const string PATCH_SUB_ROWS_WINDOW = PATCH_SUB_ROWS_BASE_LEFT + "windows" + PATCH_SUB_ROWS_BASE_RIGHT;

        public static bool SpriteSubRowsEnabled => Instance.spriteSubRows.Value;
        public static bool PatientSubRowsEnabled => Instance.patientSubRows.Value;
        public static bool RoomSubRowsEnabled => Instance.roomSubRows.Value;
        public static bool WindowSubRowsEnabled => Instance.windowSubRows.Value;

        public ConfigEntry<bool> spriteSubRows;
        public ConfigEntry<bool> patientSubRows;
        public ConfigEntry<bool> roomSubRows;
        public ConfigEntry<bool> windowSubRows;

        public PluginConfig()
        {
            spriteSubRows = config.Bind(
                CATEGORY_SUBROWS,
                nameof(spriteSubRows),
                false,
                PATCH_SUB_ROWS_SPRITE);

            patientSubRows = config.Bind(
                CATEGORY_SUBROWS,
                nameof(patientSubRows),
                false,
                PATCH_SUB_ROWS_PATIENT);

            roomSubRows = config.Bind(
                CATEGORY_SUBROWS,
                nameof(roomSubRows),
                false,
                PATCH_SUB_ROWS_ROOM);

            windowSubRows = config.Bind(
                CATEGORY_SUBROWS,
                nameof(windowSubRows),
                false,
                PATCH_SUB_ROWS_WINDOW);
        }

        public void Noop()
        {
            // Just to create the singleton lol
        }
    }
}
