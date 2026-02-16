using HarmonyLib;
using RDEditorPlus.Util;
using RDLevelEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RDEditorPlus.Patch.Rows.BeatSwitch
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Start))]
        private static class Start
        {
            private static void Postfix(scnEditor __instance)
            {
                InspectorPanel_AddClassicBeat classicPanel = __instance.GetPanel<InspectorPanel_AddClassicBeat>();

                Transform classicTemplate = classicPanel.propertiesContainer.transform.Find("breakIntoFreeTime");

                var classicClickEvent = new Button.ButtonClickedEvent();
                classicClickEvent.AddListener(SetToOneshot);
                InspectorUtil.CopyButton(classicTemplate, "SwitchBeatToOneshot", "Switch to <color=#2C4EDB>Add Oneshot Beat</color>").onClick = classicClickEvent;
                    
                InspectorPanel_AddOneshotBeat oneshotPanel = __instance.GetPanel<InspectorPanel_AddOneshotBeat>();

                var oneshotClickEvent = new Button.ButtonClickedEvent();
                oneshotClickEvent.AddListener(SetToClassic);

                // Attempt some form of future-proofing
                if (oneshotPanel.propertiesContainer != null)
                {
                    // Presumably, this will use the same structure when moved to the new UI system
                    // (Though I don't know what name it may have, so opting for a by-component search and hoping for the best)
                    Transform oneshotTemplate = oneshotPanel.propertiesContainer.GetComponentInChildren<Button>().transform.parent.parent;

                    InspectorUtil.CopyButton(oneshotTemplate, "SwitchBeatToClassic", "Switch to <color=#2C4EDB>Add Classic Beat</color>").onClick = oneshotClickEvent;
                }
                else
                {
                    const float Spacing = 4f;

                    Transform parent = oneshotPanel.transform.Find("breakWaveHolder");
                    RectTransform oneshotTemplate = (RectTransform) parent.Find("switchToSetWave");

                    Button button = InspectorUtil.CopyButton(oneshotTemplate, "SwitchBeatToClassic", "Switch to <color=#2C4EDB>Add Classic Beat</color>");
                    button.onClick = oneshotClickEvent;

                    float lowestOffset = 0f;
                    foreach (Transform child in parent)
                    {
                        if (child.gameObject.activeInHierarchy
                            && child.gameObject.activeSelf)
                        {
                            lowestOffset = Mathf.Min(lowestOffset, ((RectTransform) child).offsetMin.y);
                        }
                    }

                    float size = oneshotTemplate.sizeDelta.y;

                    RectTransform transform = (RectTransform) button.transform;
                    transform.offsetMin = new Vector2(oneshotTemplate.offsetMin.x, lowestOffset - Spacing - size * 2);
                    transform.offsetMax = new Vector2(oneshotTemplate.offsetMax.x, lowestOffset - Spacing - size);
                }
            }

            private static void SetToOneshot()
            {
                bool keepTotalBeatLength = PluginConfig.RowBeatSwitch == PluginConfig.RowBeatSwitchBehaviour.KeepTotalBeatLength;
                bool keepBeatTick = PluginConfig.RowBeatSwitch == PluginConfig.RowBeatSwitchBehaviour.KeepTickLengthOnly;
                bool doAnyLengthStuff = keepTotalBeatLength || keepBeatTick;

                float[] lengths = null;

                if (doAnyLengthStuff)
                {
                    int index = 0;
                    lengths = new float[scnEditor.instance.selectedControls.Count];

                    foreach (var control in scnEditor.instance.selectedControls)
                    {
                        LevelEvent_AddClassicBeat levelEvent = (LevelEvent_AddClassicBeat)control.levelEvent;

                        if (keepTotalBeatLength)
                        {
                            float baseLength = levelEvent.tick * (levelEvent.GetLength() - 1);
                            float synco = ((LevelEventControl_AddClassicBeat)control).syncoOffset * levelEvent.tick;

                            lengths[index++] = baseLength + synco;
                        }
                        else if (keepBeatTick)
                        {
                            lengths[index++] = levelEvent.tick;
                        }
                        else
                        {
                            lengths[index++] = 1; // Fallback
                        }
                    }
                }

                SetToType(LevelEventType.AddOneshotBeat);

                if (doAnyLengthStuff)
                {
                    int index = 0;
                    foreach (var control in scnEditor.instance.selectedControls)
                    {
                        ((LevelEvent_AddOneshotBeat)control.levelEvent).tick = lengths[index++];
                    }

                    var levelEvent = scnEditor.instance.selectedControls[0].levelEvent;
                    levelEvent.inspectorPanel.UpdateUI(levelEvent);
                }
            }

            private static void SetToClassic()
            {
                bool keepTotalBeatLength = PluginConfig.RowBeatSwitch == PluginConfig.RowBeatSwitchBehaviour.KeepTotalBeatLength;
                bool keepBeatTick = PluginConfig.RowBeatSwitch == PluginConfig.RowBeatSwitchBehaviour.KeepTickLengthOnly;
                bool doAnyLengthStuff = keepTotalBeatLength || keepBeatTick;

                float[] lengths = null;

                if (doAnyLengthStuff)
                {
                    int index = 0;
                    lengths = new float[scnEditor.instance.selectedControls.Count];

                    foreach (var control in scnEditor.instance.selectedControls)
                    {
                        LevelEvent_AddOneshotBeat levelEvent = (LevelEvent_AddOneshotBeat)control.levelEvent;

                        if (keepTotalBeatLength)
                        {
                            lengths[index++] = levelEvent.tick + levelEvent.actualDelay;
                        }
                        else if (keepBeatTick)
                        {
                            lengths[index++] = levelEvent.tick;
                        }
                        else
                        {
                            lengths[index++] = 1; // Fallback
                        }
                    }
                }

                SetToType(LevelEventType.AddClassicBeat);

                if (doAnyLengthStuff)
                {
                    int index = 0;

                    foreach (var control in scnEditor.instance.selectedControls)
                    {
                        var cast = (LevelEvent_AddClassicBeat)control.levelEvent;

                        if (keepTotalBeatLength)
                        {
                            int length = cast.GetLength() - 1;
                            cast.tick = length == 0 ? 0 : lengths[index] / length;
                        }
                        else if (keepBeatTick)
                        {
                            cast.tick = lengths[index];
                        }

                        index++;
                    }

                    var levelEvent = scnEditor.instance.selectedControls[0].levelEvent;
                    levelEvent.inspectorPanel.UpdateUI(levelEvent);
                }
            }

            private static void SetToType(LevelEventType type)
            {
                using (new SaveStateScope(clearRedo: true, skipSaving: false, skipTimelinePos: false))
                {
                    scnEditor.instance.LevelEditorPlaySound("sndEditorEventCreate", "LevelEditorActive", 1f, 1f, 0f);
                    scnEditor.instance.SetLevelEventControlType(type, true);
                }
            }
        }
    }
}
