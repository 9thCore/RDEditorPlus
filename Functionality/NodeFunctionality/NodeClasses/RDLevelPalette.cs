using RDLevelEditor;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeClasses
{
    public readonly struct RDLevelPalette(string[] palette)
    {
        public RDLevelPalette() : this(RDEditorConstants.defaultColorPalette) { }

        public readonly string[] Palette = palette;

        public static implicit operator string[](RDLevelPalette instance) => instance.Palette;
        public static implicit operator RDLevelPalette(string[] palette) => new(palette);
    }
}
