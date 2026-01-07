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
        }

        // protected readonly PageData[,] rowData = new PageData[RDEditorConstants.RoomCount * RDEditorConstants.MaxRowsPerPage];
        protected readonly Dictionary<string, HeaderData> spriteData = new Dictionary<string, HeaderData>();
        // protected readonly List<PageData>[] roomData = new List<PageData>[RDEditorConstants.RoomCount];
        // protected readonly List<PageData>[] windowData = new List<PageData>[RDEditorConstants.WindowCount];
    }
}
