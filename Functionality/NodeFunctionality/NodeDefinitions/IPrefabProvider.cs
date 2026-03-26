using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Connector;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions
{
    public interface IPrefabProvider
    {
        public NodeInput.Data[] Inputs { get; }
        public NodeOutput.Data[] Outputs { get; }
    }
}
