using RDEditorPlus.Functionality.NodeEditor.Nodes.Connector;

namespace RDEditorPlus.Functionality.NodeDefinitions
{
    public interface IPrefabProvider
    {
        public NodeInput.Data[] Inputs { get; }
        public NodeOutput.Data[] Outputs { get; }
    }
}
