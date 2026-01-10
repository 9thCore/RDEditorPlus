using HarmonyLib;
using System;
using System.Reflection;
using System.Linq;

namespace RDEditorPlus.Util
{
    public static class PatchUtil
    {
        public static void PatchNested(Harmony harmony, Type patchType)
        {
            foreach (Type nestedType in patchType.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Static))
            {
                harmony.PatchAll(nestedType);
            }
        }

        public static void PatchAllFromCurrentNamespace(Harmony harmony, Type patchHandlerType)
        {
            string currentNamespace = patchHandlerType.Namespace;

            var query = from type in patchHandlerType.Assembly.GetTypes()
                        where type.IsClass && type.Namespace == currentNamespace && type != patchHandlerType
                        select type;

            foreach (Type type in query)
            {
                PatchNested(harmony, type);
            }
        }
    }
}
