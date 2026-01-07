using RDLevelEditor;
using System.Collections.Generic;

namespace RDEditorPlus.ExtraData
{
    public abstract class BaseStorage<EventData> where EventData : class
    {
        public abstract void DecodeEvent(LevelEvent_Base levelEvent, Dictionary<string, object> properties);
        public abstract bool TryConstructJSONData(LevelEvent_Base levelEvent, out string data);
        public abstract bool TryRetrieveEventData(LevelEvent_Base levelEvent, out EventData info);
        public abstract EventData GetOrCreateEventData(LevelEvent_Base levelEvent);

        public virtual void Clear()
        {
            storage.Clear();
        }

        protected readonly Dictionary<int, EventData> storage = new();
    }
}
