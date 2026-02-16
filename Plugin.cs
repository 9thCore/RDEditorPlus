using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using RDEditorPlus.Patch;
using RDEditorPlus.Util;

namespace RDEditorPlus
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(RDModificationsGUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string RDModificationsGUID = "com.rhythmdr.randommodifications";
        public const string RDModificationsMaskName = "RDM";

        public static Plugin Instance;
        internal static new ManualLogSource Logger;

        public static void LogWarn(object message)
        {
            Logger.LogWarning(message);
        }

        public static void LogError(object message)
        {
            Logger.LogError(message);
        }

        public static void LogInfo(object message)
        {
            Logger.LogInfo(message);
        }

        private void Awake()
        {
            // Plugin startup logic
            Instance = this;
            Logger = base.Logger;
            PluginConfig.Instance.Noop();

            Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);
            PatchUtil.PatchNested(harmony, typeof(Patch_LevelEvent_Base)); // We want this patching regardless
            PatchUtil.PatchNested(harmony, typeof(Patch_scnEditor));

            Patch.SubRows.PatchHandler.Instance.Patch(harmony);
            Patch.CustomMethod.PatchHandler.Instance.Patch(harmony);
            Patch.Rows.BeatSwitch.PatchHandler.Instance.Patch(harmony);
            Patch.Select.MultiEdit.PatchHandler.Instance.Patch(harmony);

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} successfully loaded.");
        }

        public static bool RDModificationsRowPatchEnabled = false;
    }
}
