using RDEditorPlus.Functionality.NodeFunctionality.NodeClasses;
using RDEditorPlus.Functionality.NodeFunctionality.NodeDefinitions.Attributes;
using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                out List<LevelEvent_MakeSprite> sprites,
                out List<LevelEvent_Base> events,
                out List<Conditional> conditionals,
                out List<BookmarkData> bookmarks,
                out string[] palette))
            {
                return;
            }

            RemapFloatingText(events);

            this.settings = settings;
            this.rows = rows;
            this.sprites = sprites;
            this.events = events;
            this.conditionals = conditionals;
            this.bookmarks = bookmarks;
            this.palette = palette;

            var data = new RDLevelData(settings, rows, events, conditionals, sprites, bookmarks, palette);

            List<string> optionalFilesName = [];

            string directory = Path.GetDirectoryName(path);
            var files = RDPublishPopup.GetFilesForLevel(directory, ref optionalFilesName, data)
                .Where(file => File.Exists(Path.Combine(directory, file))).ToArray();

            assets = RDLevelAssets.FromFiles(directory, files);
        }

        [Variable]
        public RDLevelFile file;

        [Output]
        public RDLevelSettings settings;

        [Output]
        public RDLevelRows rows;

        [Output]
        public RDLevelSprites sprites;

        [Output]
        public RDLevelEvents events;

        [Output]
        public RDLevelConditionals conditionals;

        [Output]
        public RDLevelBookmarks bookmarks;

        [Output]
        public RDLevelPalette palette;

        [Output]
        public RDLevelAssets assets;

        private void RemapFloatingText(List<LevelEvent_Base> events)
        {
            int offset = simulator.FloatingTextIndexOffset;
            int maxIndexHere = 0;

            foreach (var levelEvent in events)
            {
                if (levelEvent is LevelEvent_FloatingText text)
                {
                    maxIndexHere = Math.Max(maxIndexHere, text.id + 1);
                    text.id += offset;
                }
                else if (levelEvent is LevelEvent_AdvanceText advanceText)
                {
                    maxIndexHere = Math.Max(maxIndexHere, advanceText.id + 1);
                    advanceText.id += offset;
                }
            }

            simulator.FloatingTextIndexOffset += maxIndexHere;
        }
    }
}
