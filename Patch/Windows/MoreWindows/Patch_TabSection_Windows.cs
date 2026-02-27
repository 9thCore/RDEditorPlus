using HarmonyLib;
using MonoMod.Cil;
using RDEditorPlus.Functionality.Windows;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Windows.MoreWindows
{
    internal static class Patch_TabSection_Windows
    {
        [HarmonyPatch(typeof(TabSection_Windows), nameof(TabSection_Windows.Setup))]
        private static class Setup
        {
            private static void Postfix(TabSection_Windows __instance)
            {
                if (!scnEditor.instance.tabSection_sprites.spriteHeaders[0].TryGetComponent(out SpriteHeader header))
                {
                    Plugin.LogError($"{scnEditor.instance.tabSection_sprites.spriteHeaders[0].name} does not have a {nameof(SpriteHeader)}??");
                    return;
                }

                GameObject template = header.addButton.gameObject;
                GameObject clone = GameObject.Instantiate(template);

                clone.AddComponent<LayoutElement>().preferredHeight = 8f;

                clone.transform.SetParent(__instance.GetComponentInChildren<VerticalLayoutGroup>().transform);
                clone.transform.SetAsLastSibling();
                clone.transform.localScale = Vector3.one;

                var button = clone.GetComponent<Button>().ReplaceWithDerivative<Button>();
                button.onClick.AddListener(MoreWindowManager.Instance.AddWindow);
            }
        }

        [HarmonyPatch(typeof(TabSection_Windows), nameof(TabSection_Windows.TurnColoredWindows))]
        private static class TurnColoredWindows
        {
            private static void ILManipulator(ILContext il)
            {
                ILCursor cursor = new(il);

                cursor
                    .GotoNext(MoveType.After, instruction => instruction.MatchLdloc(0))
                    .EmitDelegate((int index) => index % 4);
            }
        }
    }
}
