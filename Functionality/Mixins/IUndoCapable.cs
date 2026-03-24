using System.Collections.Generic;

namespace RDEditorPlus.Functionality.Mixins
{
    public interface IUndoCapable
    {
        public Stack<IAction> UndoStack { get; init; }
        public Stack<IAction> RedoStack { get; init; }

        public void Undo();
        public void Redo();
        public void ClearUndo();
        public void RegisterAction(IAction action);

        public interface IAction
        {
            public void OnUndo();
            public void OnRedo();
        }
    }
}
