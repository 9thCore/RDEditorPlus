using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Types.FileIO
{
    public class Node_LoadRDLevel : Node_Base<Node_LoadRDLevel>
    {
        public override void PostDeserialise()
        {
            var path = file.ToString();

            if (!LevelUtil.TryLevelLoad(path,
                out RDLevelSettings settings,
                out List<LevelEvent_MakeRow> rows,
                out List<LevelEvent_MakeSprite> decorations,
                out List<LevelEvent_Base> events,
                out List<Conditional> conditionals,
                out List<BookmarkData> bookmarks,
                out string[] palette))
            {
                return;
            }

            this.settings = settings;
            this.events = new(events);
            this.palette = new(palette);
        }

        [Variable]
        public RDLevelFile file;

        [Output]
        public RDLevelSettings settings;

        [Output]
        public RDLevelEvents events;

        [Output]
        public RDLevelPalette palette;
    }
}
