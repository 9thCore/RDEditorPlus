using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;
using RDLevelEditor;
using System.IO;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.FileIO
{
    public class Node_SaveRDLevel : Node_Base<Node_SaveRDLevel>
    {
        public override void PostDeserialise()
        {
            string path = file.LevelPath;

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Plugin.LogError($"Tried to save to {path}, but path does not exist");
                return;
            }

            RDLevelData data = new(
                settings: settings,
                rows: rows,
                levelEvents: events,
                conditionals: [],
                sprites: sprites,
                bookmarks: [],
                colorPalette: palette);

            string text = data.Encode();

            RDFile.WriteAllText(path, text, RDEditorConstants.DefaultLevelEncoding);
        }

        [Variable]
        public RDLevelSaveFile file;

        [Input]
        public RDLevelSettings settings;

        [Input]
        public RDLevelRows rows;

        [Input]
        public RDLevelSprites sprites;

        [Input]
        public RDLevelEvents events;

        [Input]
        public RDLevelPalette palette;
    }
}
