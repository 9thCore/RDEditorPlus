using HarmonyLib;
using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Patch.Windows.MoreWindows.SubRowDisabled
{
    internal static class Patch_TabSection_Windows
    {
        [HarmonyPatch(typeof(TabSection_Windows), nameof(TabSection_Windows.Update))]
        private static class Update
        {
            private static void Postfix(TabSection_Windows __instance)
            {
                float y = scnEditor.instance.timeline.scrollViewVertContent.anchoredPosition.y;
                if (y == scrollPosition)
                {
                    return;
                }

                var transform = (RectTransform) scnEditor.instance.tabSection_windows.listRect.GetChild(0);
                transform.anchoredPosition = transform.anchoredPosition.WithY(transform.anchoredPosition.y + y - scrollPosition);

                scrollPosition = y;
            }

            private static float scrollPosition = 0f;
        }
    }
}
