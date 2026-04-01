using HarmonyLib;
using RDEditorPlus.Functionality.Optimisation;
using RDLevelEditor;

namespace RDEditorPlus.Patch.Optimisation.OptTimeline.ChangeParents.Partition
{
    internal static class Patch_LevelEventControl_Base
    {
        [HarmonyPatch(typeof(LevelEventControl_Base), nameof(LevelEventControl_Base.UpdateUIInternal))]
        private static class UpdateUIInternal
        {
            private static void Postfix(LevelEventControl_Base __instance)
            {
                var section = __instance.tabSection;
                int page = 0;

                var editor = scnEditor.instance;
                var controlRow = __instance.levelEvent.row;

                Tab tab = __instance.levelEvent.type == LevelEventType.None ? editor.currentTab : __instance.levelEvent.defaultTab;

                switch (tab)
                {
                    case Tab.Rows:
                        foreach (var row in editor.rowsData)
                        {
                            if (row.row == controlRow)
                            {
                                page = row.room;
                                break;
                            }
                        }
                        break;
                    case Tab.Sprites:
                        foreach (var sprite in editor.spritesData)
                        {
                            if (sprite.row == controlRow)
                            {
                                page = sprite.room;
                                break;
                            }
                        }
                        break;
                }

                var partition = TimelineOptimisations.Instance.EnsurePartition(section, page, __instance.bar);
                if (__instance.transform.parent != partition)
                {
                    Plugin.LogInfo($"{__instance.transform.parent} -> {partition}");
                    __instance.transform.SetParent(partition, worldPositionStays: false);
                }
            }
        }
    }
}
