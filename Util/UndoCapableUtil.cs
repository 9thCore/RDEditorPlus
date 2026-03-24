using RDEditorPlus.Functionality.Mixins;

namespace RDEditorPlus.Util
{
    public static class UndoCapableUtil
    {
        public static void DefaultUndo(this IUndoCapable instance)
        {
            if (instance.UndoStack.Count == 0)
            {
                return;
            }

            IUndoCapable.IAction action = instance.UndoStack.Pop();
            action.OnUndo();
            instance.RedoStack.Push(action);
        }

        public static void DefaultRedo(this IUndoCapable instance)
        {
            if (instance.RedoStack.Count == 0)
            {
                return;
            }

            IUndoCapable.IAction action = instance.RedoStack.Pop();
            action.OnRedo();
            instance.UndoStack.Push(action);
        }

        public static void DefaultClearUndo(this IUndoCapable instance)
        {
            instance.UndoStack.Clear();
            instance.RedoStack.Clear();
        }

        public static void DefaultRegisterAction(this IUndoCapable instance, IUndoCapable.IAction action)
        {
            instance.UndoStack.Push(action);
            instance.RedoStack.Clear();
        }
    }
}
