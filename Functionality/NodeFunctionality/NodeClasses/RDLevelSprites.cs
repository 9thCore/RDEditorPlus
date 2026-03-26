using RDLevelEditor;
using System.Collections.Generic;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeClasses
{
    public readonly struct RDLevelSprites(List<LevelEvent_MakeSprite> sprites)
    {
        public RDLevelSprites() : this(null) { }

        public List<LevelEvent_MakeSprite> Collect() => this;

        public readonly IReadOnlyList<LevelEvent_MakeSprite> Sprites = sprites.AsReadOnly();

        public static implicit operator List<LevelEvent_MakeSprite>(RDLevelSprites instance)
            => instance.Sprites != null ? [.. instance.Sprites] : [];

        public static implicit operator RDLevelSprites(List<LevelEvent_MakeSprite> list) => new(list);
    }
}
