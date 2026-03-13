using RDLevelEditor;
using System.Collections.Generic;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeClasses
{
    public readonly struct RDLevelEvents
    {
        public RDLevelEvents(List<LevelEvent_Base> events) : this(events.AsReadOnly(), new(), 0) { }

        public readonly List<LevelEvent_Base> Apply()
        {
            if (events == null)
            {
                return [];
            }

            List<LevelEvent_Base> copy = [.. events];

            for (int i = 0; i < changesToPerform; i++)
            {
                changes[i].Apply(ref copy);
            }

            return copy;
        }

        public readonly RDLevelEvents WithBarFilter(int start, int end)
        {
            changes.Add(new BarFilterEventsChange(start, end));
            return new(events, changes, changesToPerform + 1);
        }

        private RDLevelEvents(IReadOnlyList<LevelEvent_Base> events, List<IEventsChange> changes, int changesToPerform)
        {
            this.events = events;
            this.changes = changes;
            this.changesToPerform = changesToPerform;
        }

        private readonly IReadOnlyList<LevelEvent_Base> events;
        private readonly List<IEventsChange> changes;
        private readonly int changesToPerform;

        private interface IEventsChange
        {
            public void Apply(ref List<LevelEvent_Base> events);
        }

        private readonly struct BarFilterEventsChange(int start, int end) : IEventsChange
        {
            private readonly int start = start;
            private readonly int end = end;

            public void Apply(ref List<LevelEvent_Base> events)
            {
                int start = this.start;
                int end = this.end;
                events.RemoveAll(ev => ev.bar < start || ev.bar > end);
            }
        }
    }
}
