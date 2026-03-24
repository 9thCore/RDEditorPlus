using RDEditorPlus.Functionality.Mixins;

namespace RDEditorPlus.Util
{
    public static class UndoCapableUtil
    {
        // Whatever it's single-threaded who cares
        public static bool CurrentlyUndoingOrRedoing { get; private set; } = false;

        public static void DefaultUndo(this IUndoCapable instance)
        {
            if (instance.UndoStack.Count == 0)
            {
                return;
            }

            CurrentlyUndoingOrRedoing = true;
            IUndoCapable.IAction action = instance.UndoStack.Pop();
            action.OnUndo();
            instance.RedoStack.Push(action);
            CurrentlyUndoingOrRedoing = false;
        }

        public static void DefaultRedo(this IUndoCapable instance)
        {
            if (instance.RedoStack.Count == 0)
            {
                return;
            }

            CurrentlyUndoingOrRedoing = true;
            IUndoCapable.IAction action = instance.RedoStack.Pop();
            action.OnRedo();
            instance.UndoStack.Push(action);
            CurrentlyUndoingOrRedoing = false;
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
