using HarmonyLib;
using RDEditorPlus.ExtraData;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Select.MultiEdit
{
    internal static class Patch_PropertyControl_BeatModifiers
    {
        [HarmonyPatch(typeof(PropertyControl_BeatModifiers), nameof(PropertyControl_BeatModifiers.Setup))]
        private static class Setup
        {
            private static void Postfix(PropertyControl_BeatModifiers __instance)
            {
                if (__instance.GetComponent<UpdateInformation>() == null)
                {
                    __instance.gameObject.AddComponent<UpdateInformation>();
                }
            }
        }

        [HarmonyPatch(typeof(PropertyControl_BeatModifiers), nameof(PropertyControl_BeatModifiers.UpdateUI))]
        private static class UpdateUI
        {
            private static void Prefix(PropertyControl_BeatModifiers __instance)
            {
                __instance.gameObject.GetComponent<UpdateInformation>().index = DontUpdate;
            }

            private static void Postfix(PropertyControl_BeatModifiers __instance)
            {
                int index = 0;

                foreach (var button in __instance.buttons)
                {
                    if (!__instance.EqualValueForSelectedEvents(index))
                    {
                        button.GetComponent<Image>().sprite = InspectorUtil.MixedBeatModifierSprite;
                    }

                    index++;
                }

                if (!InspectorUtil.SyncoBeatEqualValueForSelectedEvents())
                {
                    __instance.syncoBeat = -1;
                    foreach (var button in __instance.syncoButtons)
                    {
                        button.GetComponent<Image>().sprite = __instance.syncoOff;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PropertyControl_BeatModifiers), nameof(PropertyControl_BeatModifiers.Save))]
        private static class Save
        {
            private static bool Prefix(PropertyControl_BeatModifiers __instance, LevelEvent_Base levelEvent)
            {
                if (!InspectorUtil.CanMultiEdit())
                {
                    return true;
                }

                int index = __instance.gameObject.GetComponent<UpdateInformation>().index;
                if (index != DontUpdate)
                {
                    string value = __instance.GetEventValue(levelEvent).ToString();
                    var builder = new StringBuilder(value);
                    builder[index] = __instance.value[index];
                    __instance.SetEventValue(levelEvent, builder.ToString());
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(PropertyControl_BeatModifiers), nameof(PropertyControl_BeatModifiers.AddListeners))]
        private static class AddListeners
        {
            private static void Postfix(PropertyControl_BeatModifiers __instance)
            {
                UpdateInformation component = __instance.gameObject.GetComponent<UpdateInformation>();
                int index = 0;

                foreach (var button in __instance.buttons)
                {
                    int collected = index;

                    button.onClick = (RDPointerEventData)Delegate.Combine((RDPointerEventData)(data =>
                    {
                        component.index = collected;
                    }), button.onClick);

                    index++;
                }

                foreach (var button in __instance.syncoButtons)
                {
                    button.onClick = (RDPointerEventData)Delegate.Combine((RDPointerEventData)(data =>
                    {
                        PropertyStorage.Instance.beatModifierSyncoChanged = true;
                    }), button.onClick);
                }
            }
        }

        private class UpdateInformation : MonoBehaviour
        {
            public int index = DontUpdate;
        }

        private const int DontUpdate = -1;
    }
}
