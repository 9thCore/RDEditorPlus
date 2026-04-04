using HarmonyLib;
using RDEditorPlus.Util;

namespace RDEditorPlus.Patch
{
    internal abstract class BasePatchHandler
    {
        protected abstract bool CanApply { get; }

        public virtual void Patch(Harmony harmony)
        {
            if (!CanApply)
            {
                return;
            }

            PatchUtil.PatchAllFromCurrentNamespace(harmony, GetType());
        }
    }

    internal abstract class BasePatchHandler<T> : BasePatchHandler where T : BasePatchHandler<T>, new()
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                instance ??= new();
                return instance;
            }
        }        
    }
}
