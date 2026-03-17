using UnityEngine;
using RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Connector;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Modifier
{
    public class MathNodeModifier : NodeModifier
    {
        public void Awake()
        {
            input1 = node.GetInput("value1");
            input2 = node.GetInput("value2");
            output = node.GetOutput("value");

            node.SubscribeToConnect(OnConnect);
            node.SubscribeToDisconnect(OnDisconnect);
            node.SubscribeToReplace(OnReplace);
        }

        private void OnConnect()
        {
            if (BothInputsHaveValue)
            {
                UpdateOutputOverrides();
            }
        }

        private void OnDisconnect()
        {
            if (BothInputsHaveValue)
            {
                return;
            }

            output.Unlink(dontRaiseDisconnectEvent: false);
            output.ResetUsageOverrideType();
            output.ResetColorOverrideType();
        }

        private void OnReplace()
        {
            Node.Type? currentOverrideType = output.ColorOverrideType;

            UpdateOutputOverrides();

            if (output.ColorOverrideType != currentOverrideType)
            {
                output.Unlink(dontRaiseDisconnectEvent: false);
            }
        }

        private void UpdateOutputOverrides()
        {
            var type = Node.GetBestFitFor(input1.ColorOverrideType.Value, input2.ColorOverrideType.Value);

            output.SetUsageOverrideType(type);
            output.SetColorOverrideType(type);
        }

        private bool BothInputsHaveValue => input1.ColorOverrideType.HasValue && input2.ColorOverrideType.HasValue;

        [SerializeField]
        private NodeInput input1, input2;

        [SerializeField]
        private NodeOutput output;
    }
}
