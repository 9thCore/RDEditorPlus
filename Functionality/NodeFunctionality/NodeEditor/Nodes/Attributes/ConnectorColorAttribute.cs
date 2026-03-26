using System;
using UnityEngine;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeEditor.Nodes.Attributes
{
    public class ConnectorColorAttribute(string validColor, string invalidColor = null) : Attribute
    {
        public readonly Color ValidControl = validColor.HexToColor();

        public readonly Color InvalidControl = invalidColor?.HexToColor()
            ?? Color.Lerp(validColor.HexToColor(), Color.black, 0.5f);
    }
}
