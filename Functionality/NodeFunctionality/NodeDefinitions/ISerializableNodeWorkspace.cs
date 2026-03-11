using UnityEngine;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions
{
    public interface ISerializableNodeWorkspace
    {
        public INode[] GetDependencyOrderedNodes();

        public interface INode
        {
            public IInput[] Inputs { get; }
            public IVariable[] Variables { get; }
            public string Id { get; }
            public string Name { get; }
            public Vector2 Position { get; }

            public interface IVariable
            {
                public string Name { get; }
                public object Value { get; }
            }

            public interface IInput
            {
                public string Name { get; }
                public bool IsLinked { get; }
                public string Target { get; }
                public string Output { get; }
            }
        }
    }
}
