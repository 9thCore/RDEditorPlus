using RDEditorPlus.Util;
using RDLevelEditor;
using System;
using System.Collections.Generic;

namespace RDEditorPlus.Functionality.NodeFunctionality.NodeClasses
{
    public readonly struct RDLevelEvents
    {
        public RDLevelEvents() : this(null, [], 0) { }

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
            return Next;
        }

        public readonly RDLevelEvents WithTabFilter(bool sounds, bool rows, bool actions, bool sprites, bool rooms, bool windows)
        {
            changes.Add(new TabFilterEventsChange(TabFilterEventsChange.GetFlagFrom(sounds, rows, actions, sprites, rooms, windows)));
            return Next;
        }

        public readonly RDLevelEvents WithRowMapping(int[] map)
        {
            changes.Add(new RowMapEventsChange(map));
            return Next;
        }

        public readonly RDLevelEvents WithRowOffset(int offset)
        {
            changes.Add(new RowOffsetEventsChange(offset));
            return Next;
        }

        public readonly RDLevelEvents WithConditionalOffset(int offset)
        {
            changes.Add(new ConditionalOffsetEventsChange(offset));
            return Next;
        }

        public static implicit operator RDLevelEvents(List<LevelEvent_Base> events) => new(events);
        public static implicit operator List<LevelEvent_Base>(RDLevelEvents instance) => instance.Apply();

        private RDLevelEvents(IReadOnlyList<LevelEvent_Base> events, List<IEventsChange> changes, int changesToPerform)
        {
            this.events = events;
            this.changes = changes;
            this.changesToPerform = changesToPerform;
        }

        private readonly IReadOnlyList<LevelEvent_Base> events;
        private readonly List<IEventsChange> changes;
        private readonly int changesToPerform;

        private readonly RDLevelEvents Next => new(events, changes, changesToPerform + 1);

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

        private readonly struct TabFilterEventsChange(TabFilterEventsChange.TabFlag flag) : IEventsChange
        {
            private readonly TabFlag flag = flag;

            public void Apply(ref List<LevelEvent_Base> events)
            {
                TabFlag flag = this.flag;
                events.RemoveAll(ev => !flag.HasFlag(GetFlagFromTab(ev.defaultTab)));
            }

            [Flags]
            public enum TabFlag
            {
                None = 0,
                Sounds = 1 << 0,
                Rows = 1 << 1,
                Actions = 1 << 2,
                Sprites = 1 << 3,
                Rooms = 1 << 4,
                Windows = 1 << 5
            }

            public static TabFlag GetFlagFrom(bool sounds, bool rows, bool actions, bool sprites, bool rooms, bool windows)
                => (TabFlag)((int)(sounds ? TabFlag.Sounds : 0) + (int)(rows ? TabFlag.Rows : 0)
                 + (int)(actions ? TabFlag.Actions : 0) + (int)(sprites ? TabFlag.Sprites : 0)
                 + (int)(rooms ? TabFlag.Rooms : 0) + (int)(windows ? TabFlag.Windows : 0));

            private static TabFlag GetFlagFromTab(Tab tab)
            {
                return tab switch
                {
                    Tab.Song => TabFlag.Sounds,
                    Tab.Rows => TabFlag.Rows,
                    Tab.Actions => TabFlag.Actions,
                    Tab.Sprites => TabFlag.Sprites,
                    Tab.Rooms => TabFlag.Rooms,
                    Tab.Windows => TabFlag.Windows,
                    _ => TabFlag.None
                };
            }
        }

        private readonly struct RowMapEventsChange(int[] map) : IEventsChange
        {
            public void Apply(ref List<LevelEvent_Base> events)
            {
                foreach (var levelEvent in events)
                {
                    if (levelEvent.info.usesRow)
                    {
                        levelEvent.row = map[levelEvent.row];
                    }
                }
            }
        }

        private readonly struct RowOffsetEventsChange(int offset) : IEventsChange
        {
            public void Apply(ref List<LevelEvent_Base> events)
            {
                foreach (var levelEvent in events)
                {
                    if (levelEvent.info.usesRow)
                    {
                        levelEvent.row += offset;
                    }
                }
            }
        }

        private readonly struct ConditionalOffsetEventsChange(int offset) : IEventsChange
        {
            public void Apply(ref List<LevelEvent_Base> events)
            {
                foreach (var levelEvent in events)
                {
                    if (levelEvent.conditionals != null)
                    {
                        for (int i = levelEvent.conditionals.Count - 1; i >= 0; i--)
                        {
                            levelEvent.conditionals[i] += Math.Sign(levelEvent.conditionals[i]) * offset;
                        }
                    }
                }
            }
        }
    }
}
