using HarmonyLib;
using RDEditorPlus.Functionality.Audio;
using RDEditorPlus.Util;
using RDLevelEditor;
using System.Linq;

namespace RDEditorPlus.Patch.Audio
{
    internal static class Patch_scnEditor
    {
        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Awake))]
        private static class Awake
        {
            private static void Postfix()
            {
                AudioOneTrueNameAutocompletion.Instance.StartFetch();
            }
        }

        [HarmonyPatch(typeof(scnEditor), nameof(scnEditor.Start))]
        private static class Start
        {
            private static void Postfix(scnEditor __instance)
            {
                var ipm = __instance.ipm;

                AudioOneTrueNameAutocompletion.Instance.Setup(GetProperties<InspectorPanel_PlaySound, PropertyControl_Sound>(ipm));
                AudioOneTrueNameAutocompletion.Instance.Setup(GetProperties<InspectorPanel_SetGameSound, PropertyControl_SetGameSound>(ipm));
                AudioOneTrueNameAutocompletion.Instance.Setup(GetProperties<InspectorPanel_SetCountingSound, PropertyControl_SetCountingSound>(ipm));
                AudioOneTrueNameAutocompletion.Instance.Setup(GetProperties<InspectorPanel_SetBeatSound, PropertyControl_Sound>(ipm));
                AudioOneTrueNameAutocompletion.Instance.Setup(GetProperties<InspectorPanel_SetClapSounds, PropertyControl_Sound>(ipm));
                AudioOneTrueNameAutocompletion.Instance.Setup(GetProperties<InspectorPanel_PlaySong, PropertyControl_Sound>(ipm));
            }
        }

        private static PropertyType[] GetProperties<InspectorType, PropertyType>(RDInspectorPanelManager inspectorPanelManager)
            where InspectorType : InspectorPanel where PropertyType : PropertyControl
            => [.. inspectorPanelManager.Get<InspectorType>().properties
                .Select(property => property.control as PropertyType).Where(control => control != null)];
    }
}
