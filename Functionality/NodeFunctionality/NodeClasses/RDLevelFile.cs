using System;
using System.IO;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeClasses
{
    public readonly struct RDLevelFile(string path) : IEquatable<RDLevelFile>
    {
        public RDLevelFile() : this(string.Empty) { }

        public readonly string path = path;

        public readonly bool Equals(RDLevelFile other)
        {
            return path == other.path;
        }

        public readonly override string ToString()
        {
            return path ?? string.Empty;
        }

        public readonly string LevelName => Path.GetFileNameWithoutExtension(path);

        public static implicit operator RDLevelFile(string path) => new(path);
    }
}
