using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Modifier;
using System;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes.Modifier
{
    public abstract class NodeModifierAttribute : Attribute
    {
        public abstract void Apply(GameObject prefab, Node node);
    }

    public class NodeModifierAttribute<NodeModifierType> : NodeModifierAttribute
        where NodeModifierType : NodeModifier
    {
        public override void Apply(GameObject prefab, Node node)
        {
            prefab.AddComponent<NodeModifierType>().Setup(node);
        }
    }
}
