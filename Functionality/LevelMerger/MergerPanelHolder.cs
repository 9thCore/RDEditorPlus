using RDEditorPlus.Functionality.NodeEditor;
using RDEditorPlus.Functionality.NodeEditor.Nodes;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace RDEditorPlus.Functionality.LevelMerger
{
    public class MergerPanelHolder : NodePanelHolder
    {
        private static MergerPanelHolder instance;
        public static MergerPanelHolder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new();
                }

                return instance;
            }
        }

        public override void HandleDeserialization(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(MergeData));

            if (serializer.Deserialize(stream) is not MergeData result)
            {
                throw new InvalidDataException($"Could not deserialise as {typeof(MergeData).FullName}");
            }

            int index = 0;
            foreach (var nodeData in result.Nodes)
            {
                if (nodeData.id.IsNullOrEmpty())
                {
                    throw new InvalidDataException($"Node #{index} is missing an ID");
                }

                Node node = AddNode(nodeData.name, nodeData.Position ?? Vector2.zero, nodeData.id);
                if (node == null)
                {
                    throw new InvalidDataException();
                }

                if (nodeData.Inputs != null)
                {
                    foreach (var inputData in nodeData.Inputs)
                    {
                        var link = inputData.Link;
                        if (link != null)
                        {
                            if (!TryGetNodeFromID(link.target, out Node other))
                            {
                                throw new InvalidDataException($"Node {link.target} does not exist before {nodeData.id} needs it. Definition order of nodes in the save matters");
                            }

                            var input = node.GetInput(inputData.name);
                            var output = other.GetOutput(link.output);
                            if (input != null && output != null)
                            {
                                input.LinkIfNotYetLinked(output);
                            }
                        }
                    }
                }

                index++;
            }
        }

        protected MergerPanelHolder()
        {
            title.text = "Level Merger Utility";
            title.color = Color.yellow;
        }
    }
}
