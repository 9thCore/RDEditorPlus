using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RDLevelEditor;

namespace RDEditorPlus.Patch.BugFix.WrongEventList
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.DecodeData))]
        private static class DecodeData
        {
            private static void ILManipulator(ILContext il)
            {
                const int RowFlagIndex = 1;
                const int SpriteFlagIndex = 2;

                ILCursor cursor = new(il);

                var force = AccessTools.Method(typeof(DecodeData), nameof(ForceTrue));

                cursor
                    .GotoNext(MoveType.Before, instruction => instruction.MatchStloc(RowFlagIndex))
                    .Emit(OpCodes.Call, force)
                    .GotoNext(MoveType.Before, instruction => instruction.MatchStloc(SpriteFlagIndex))
                    .Emit(OpCodes.Call, force);
            }

            private static bool ForceTrue(bool _) => true;
        }
    }
}
