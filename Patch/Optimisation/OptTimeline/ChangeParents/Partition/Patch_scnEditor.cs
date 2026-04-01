using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDEditorPlus.Functionality.Optimisation;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.Optimisation.OptTimeline.ChangeParents.Partition
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.DeleteAllData))]
        private static class DeleteAllData
        {
            private static void Prefix(scnEditor __instance)
            {
                TimelineOptimisations.Instance.ClearPartitionData(__instance);
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.DecodeData))]
        private static class DecodeData
        {
            private static void Prefix(scnEditor __instance)
            {
                TimelineOptimisations.Instance.PreparePartitionData(__instance);
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.AddNewEventControl))]
        private static class AddNewEventControl
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(instruction => instruction.MatchCallOrCallvirt<Transform>(nameof(Transform.SetParent)))
                    .GotoPrev(MoveType.After, instruction => instruction.MatchLdarg(1))
                    .MoveAfterLabels()
                    .Emit(OpCodes.Ldarg_1)
                    .Emit(OpCodes.Ldloc_1)
                    .Emit(OpCodes.Call, AccessTools.Method(typeof(AddNewEventControl), nameof(TransformTransformer)))
                    .Emit(OpCodes.Stloc_1);
            }

            private static Transform TransformTransformer(LevelEventControl_Base eventControl, Transform transform)
            {
                var gameObject = transform.gameObject;
                int page = 0;
                var section = eventControl.tabSection;

                if (eventControl.levelEvent.type == LevelEventType.None)
                {
                    page = section.pageIndex;
                }
                else 
                {
                    while (section.container[page] != gameObject)
                    {
                        page++;
                    }
                }

                Transform partition = TimelineOptimisations.Instance.EnsurePartition(section, page, eventControl.bar);
                return partition;
            }
        }
    }
}
