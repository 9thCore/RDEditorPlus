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
                __instance.listRect.SetupWithScrollMaskIntermediary("Windows");
                __instance.listRect.offsetMin = Vector2.zero;
                __instance.listRect.offsetMax = Vector2.zero;

                if (!scnEditor.instance.tabSection_rows.rowHeaders[0].TryGetComponent(out RowHeader header))
                {
                    Plugin.LogError($"{scnEditor.instance.tabSection_rows.rowHeaders[0].name} does not have a {nameof(RowHeader)}??");
                    return;
                }

                GameObject template = header.addButton.gameObject;
                GameObject clone = GameObject.Instantiate(template);

                GameObject root = new($"mod_{MyPluginInfo.PLUGIN_GUID}_AddButtonRoot");
                root.SetActive(false);

                root.AddComponent<Image>();
                root.AddComponent<Mask>().showMaskGraphic = false;
                root.AddComponent<LayoutElement>().preferredHeight = 8f;

                root.transform.SetParent(__instance.GetComponentInChildren<VerticalLayoutGroup>().transform);
                clone.transform.SetParent(root.transform);

                root.transform.SetAsLastSibling();
                root.transform.localScale = Vector3.one;
                root.transform.localPosition = Vector3.zero;

                clone.transform.localScale = Vector3.one;
                clone.transform.localPosition = Vector3.zero;

                var rt = clone.transform as RectTransform;
                rt.anchorMin = rt.anchorMin.WithX(2f / 3f);
                rt.anchorMax = rt.anchorMax.WithX(2f / 3f);

                var button = clone.GetComponent<Button>().ReplaceWithDerivative<Button>();
                button.onClick.AddListener(MoreWindowManager.Instance.AddButtonClick);

                root.SetActive(true);
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
                    .EmitDelegate((int index) => index % RDEditorConstants.windowColors.Length);
            }
        }
    }
}
