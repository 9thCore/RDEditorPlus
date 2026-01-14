using RDLevelEditor;
using System.Collections.Generic;

namespace RDEditorPlus.ExtraData
{
    public abstract class PagedStorage<EventData, HeaderData> : BaseStorage<EventData> where EventData : class where HeaderData : class, new()
    {
        public HeaderData GetOrCreatePageDataForSprite(string target)
        {
            if (!spriteData.ContainsKey(target))
            {
                spriteData.Add(target, new HeaderData());
            }

            return spriteData[target];
        }

        public override void Clear()
        {
            base.Clear();
            spriteData.Clear();

            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                roomData[i] = new();
            }
        }

        public PagedStorage()
        {
            for (int i = 0; i < RDEditorConstants.RoomCount; i++)
            {
                roomData[i] = new();
            }
        }

        // protected readonly PageData[,] rowData = new PageData[RDEditorConstants.RoomCount * RDEditorConstants.MaxRowsPerPage];
        public readonly Dictionary<string, HeaderData> spriteData = new Dictionary<string, HeaderData>();
        public readonly HeaderData[] roomData = new HeaderData[RDEditorConstants.RoomCount];
        // protected readonly List<PageData>[] windowData = new List<PageData>[RDEditorConstants.WindowCount];
    }
}
