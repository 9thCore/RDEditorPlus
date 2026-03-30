using HarmonyLib;
using RDEditorPlus.Functionality.Components;
using RDLevelEditor;
using System.Collections.Generic;
using UnityEngine;

namespace RDEditorPlus.Patch.Optimisation.OptTimeline.ChangeParents
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.AddNewEventControl))]
        private static class AddNewEventControl
        {
            private static void Postfix(LevelEventControl_Base eventControl)
            {
                eventControl.gameObject.AddComponent<ArbitraryTransformHolder>().Transform = eventControl.transform.parent;
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.DeleteAllData))]
        private static class DeleteAllData
        {
            private static void Prefix()
            {
                foreach (var control in scnEditor.instance.eventControls)
                {
                    GameObject.Destroy(control.gameObject);
                }
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.ShowTabSection))]
        private static class ShowTabSection
        {
            private static void Postfix(Tab tab)
            {
                var editor = scnEditor.instance;

                switch (tab)
                {
                    case Tab.Song:
                        ParentAllFrom(editor.eventControls_sounds);
                        UnparentAllFrom(editor.eventControls_rows);
                        UnparentAllFrom(editor.eventControls_actions);
                        UnparentAllFrom(editor.eventControls_rooms);
                        UnparentAllFrom(editor.eventControls_sprites);
                        UnparentAllFrom(editor.eventControls_windows);
                        break;
                    case Tab.Rows:
                        UnparentAllFrom(editor.eventControls_sounds);
                        ParentAllFrom(editor.eventControls_rows);
                        UnparentAllFrom(editor.eventControls_actions);
                        UnparentAllFrom(editor.eventControls_rooms);
                        UnparentAllFrom(editor.eventControls_sprites);
                        UnparentAllFrom(editor.eventControls_windows);
                        break;
                    case Tab.Actions:
                        UnparentAllFrom(editor.eventControls_sounds);
                        UnparentAllFrom(editor.eventControls_rows);
                        ParentAllFrom(editor.eventControls_actions);
                        UnparentAllFrom(editor.eventControls_rooms);
                        UnparentAllFrom(editor.eventControls_sprites);
                        UnparentAllFrom(editor.eventControls_windows);
                        break;
                    case Tab.Rooms:
                        UnparentAllFrom(editor.eventControls_sounds);
                        UnparentAllFrom(editor.eventControls_rows);
                        UnparentAllFrom(editor.eventControls_actions);
                        ParentAllFrom(editor.eventControls_rooms);
                        UnparentAllFrom(editor.eventControls_sprites);
                        UnparentAllFrom(editor.eventControls_windows);
                        break;
                    case Tab.Sprites:
                        UnparentAllFrom(editor.eventControls_sounds);
                        UnparentAllFrom(editor.eventControls_rows);
                        UnparentAllFrom(editor.eventControls_actions);
                        UnparentAllFrom(editor.eventControls_rooms);
                        ParentAllFrom(editor.eventControls_sprites);
                        UnparentAllFrom(editor.eventControls_windows);
                        break;
                    case Tab.Windows:
                        UnparentAllFrom(editor.eventControls_sounds);
                        UnparentAllFrom(editor.eventControls_rows);
                        UnparentAllFrom(editor.eventControls_actions);
                        UnparentAllFrom(editor.eventControls_rooms);
                        UnparentAllFrom(editor.eventControls_sprites);
                        ParentAllFrom(editor.eventControls_windows);
                        break;
                }
            }

            private static void UnparentAllFrom(List<List<LevelEventControl_Base>> nestedList)
            {
                if (nestedList == null)
                {
                    return;
                }

                foreach (var list in nestedList)
                {
                    UnparentAllFrom(list);
                }
            }

            private static void UnparentAllFrom(List<LevelEventControl_Base> list)
            {
                if (list == null)
                {
                    return;
                }

                foreach (var control in list)
                {
                    control.transform.SetParent(null, worldPositionStays: false);
                }
            }

            private static void ParentAllFrom(List<List<LevelEventControl_Base>> nestedList)
            {
                if (nestedList == null)
                {
                    return;
                }

                foreach (var list in nestedList)
                {
                    ParentAllFrom(list);
                }
            }

            private static void ParentAllFrom(List<LevelEventControl_Base> list)
            {
                foreach (var control in list)
                {
                    control.transform.SetParent(control.GetComponent<ArbitraryTransformHolder>().Transform, worldPositionStays: false);
                }
            }
        }
    }
}
