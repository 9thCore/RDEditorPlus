using HarmonyLib;
using MonoMod.Cil;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Reflection;

namespace RDEditorPlus.Patch
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Awake))]
        private static class Awake
        {
            private static void Postfix()
            {
                AssetUtil.FetchEditorAssets();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Start))]
        private static class Start
        {
            private static void Postfix(scnEditor __instance)
            {
                AssetUtil.FetchEditorAssetsManual(__instance);
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.UndoOrRedo))]
        private static class UndoOrRedo
        {
            private static void Prefix()
            {
                LevelUtil.CurrentlyUndoing = true;
            }

            private static void Postfix()
            {
                LevelUtil.CurrentlyUndoing = false;
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.SetLevelEventControlType))]
        private static class SetLevelEventControlType
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.Before, instruction => instruction.MatchCall<Type>(nameof(Type.GetType)))
                    .EmitDelegate(FetchLevelEventClass);

                cursor
                    .GotoNext(MoveType.After, instruction => true)
                    .EmitDelegate(FixType);
            }

            private static string FetchLevelEventClass(string text)
            {
                levelEventClass = text;
                return text;
            }

            private static Type FixType(Type type)
            {
                if (type == null)
                {
                    // Might have loaded from the wrong assembly?
                    return GameAssembly.GetType(levelEventClass, throwOnError: false, ignoreCase: true);
                }

                return type;
            }

            private static string levelEventClass;
            private static readonly Assembly GameAssembly = typeof(scnEditor).Assembly;
        }
    }
}
