using System.Xml.Serialization;
using UnityEngine;

namespace RDEditorPlus.Functionality.LevelMerger
{
    public class MergeData
    {
        public Node[] Nodes;

        public class Node
        {
            [XmlAttribute]
            public string id, name;

            public Position Position;

            public Input[] Inputs;
        }

        public class Position
        {
            [XmlAttribute]
            public float x, y;

            public static implicit operator Vector2(Position p) => new(p.x, p.y);
        }

        public class Input
        {
            [XmlAttribute]
            public string name;

            public Link Link;
        }

        public class Link
        {
            [XmlAttribute]
            public string target, output;
        }
    }
}
