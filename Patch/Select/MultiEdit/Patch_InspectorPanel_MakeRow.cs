using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDLevelEditor;
using System;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_InspectorPanel_MakeRow
    {
        [HarmonyPatch(typeof(InspectorPanel_MakeRow), nameof(InspectorPanel_MakeRow.Awake))]
        private static class Awake
        {
            private static void Postfix(InspectorPanel_MakeRow __instance)
            {
                __instance.character.onEndEdit = (CharacterPicker.EndHandler)
                    Delegate.Combine(
                        __instance.character.onEndEdit,
                        new CharacterPicker.EndHandler((Character _, string _, string _) =>
                        {
                            PropertyStorage.Instance.UpdateRowPropertyControls();
                        }));
            }
        }

        [HarmonyPatch(typeof(InspectorPanel_MakeRow), nameof(InspectorPanel_MakeRow.RoomDropdownWasUpdated))]
        private static class RoomDropdownWasUpdated
        {
            private static void Postfix()
            {
                PropertyStorage.Instance.UpdateRowPropertyControls();
            }
        }
    }
}
