using UnityEngine;

namespace RDEditorPlus.Functionality.NodeDefinitions
{
    public interface ISerializableNodeWorkspace
    {
        public INode[] GetDependencyOrderedNodes();

        public interface INode
        {
            public IInput[] Inputs { get; }
            public string Id { get; }
            public string Name { get; }
            public Vector2 Position { get; }

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
