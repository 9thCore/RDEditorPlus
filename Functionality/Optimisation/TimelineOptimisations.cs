using RDLevelEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RDEditorPlus.Functionality.Optimisation
{
    public class TimelineOptimisations
    {
        private static TimelineOptimisations instance;
        public static TimelineOptimisations Instance => instance ??= new();

        public void FetchData(Timeline timeline)
        {
            scrollViewVertContent = timeline.scrollViewVertContent;
        }

        public void ClearPartitionData(scnEditor editor)
        {
            foreach (var section in editor.tabSections)
            {
                Data(section).Clear();
            }
        }

        public void PreparePartitionData(scnEditor editor)
        {
            foreach (var section in editor.tabSections)
            {
                int length = section.container.Length;
                partitionHolders[(int)section.tab] = new PartitionHolderData[length];

                for (int i = 0; i < length; i++)
                {
                    Data(section.tab, i) = new(section.container[i].transform, []);
                }
            }
        }

        public Transform EnsurePartition(TabSection section, int page, int bar)
        {
            var partitionHolder = Data(section.tab, page);
            var hash = new PartitionHash(bar);

            if (partitionHolder.Map.TryGetValue(hash, out var result))
            {
                return result;
            }

            GameObject partition = new($"Mod_{MyPluginInfo.PLUGIN_GUID}_TimelineEventPartition_Tab{section.tab}+Page{page}+Bar{bar}");

            var transform = partition.transform;
            transform.SetParent(section.container[page].transform);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;

            partitionHolder.Map[hash] = transform;
            return transform;
        }

        public void PreChangePage(TabSection section) => Unparent(section, entireTab: false);
        public void PostChangePage(TabSection section) => Parent(section, entireTab: false);

        public void Parent(TabSection section, bool entireTab) => SetParent(section, scrollViewVertContent, entireTab);
        public void Unparent(TabSection section, bool entireTab) => SetParent(section, null, entireTab);

        public void HandlePartitionParents(TabSection section, int topLeftBar, int bottomRightBar)
            => Data(section).ParentPartitionsIfInside(new(topLeftBar), new(bottomRightBar));

        private void SetParent(TabSection section, Transform parent, bool entireTab)
        {
            if (section == null)
            {
                return;
            }

            if (entireTab)
            {
                foreach (var holder in section.container)
                {
                    if (holder.transform.parent != parent)
                    {
                        holder.transform.SetParent(parent, worldPositionStays: false);
                    }
                }
            }
            else
            {
                var holder = section.container[section.pageIndex];
                if (holder.transform.parent != parent)
                {
                    holder.transform.SetParent(parent, worldPositionStays: false);
                }
            }
        }

        private ref PartitionHolderData Data(TabSection section) => ref Data(section.tab, section.pageIndex);
        private ref PartitionHolderData Data(Tab tab, int page) => ref partitionHolders[(int)tab][page];

        private Transform scrollViewVertContent;
        private readonly PartitionHolderData[][] partitionHolders = new PartitionHolderData[Enum.GetValues(typeof(Tab)).Length][];

        private readonly struct PartitionHash(int bar)
        {
            public bool Inside(PartitionHash topLeft, PartitionHash bottomRight)
            {
                return topLeft.bar <= bar && bar <= bottomRight.bar;
            }

            public override int GetHashCode() => bar;

            private readonly int bar = bar;
        }

        private readonly record struct PartitionHolderData(Transform Container, Dictionary<PartitionHash, Transform> Map)
        {
            public void Clear()
            {
                foreach (var kvp in Map)
                {
                    GameObject.Destroy(kvp.Value.gameObject);
                }
            }

            public void ParentPartitionsIfInside(PartitionHash topLeft, PartitionHash bottomRight)
            {
                foreach (var kvp in Map)
                {
                    var parent = kvp.Key.Inside(topLeft, bottomRight) ? Container : null;
                    if (kvp.Value.parent != parent)
                    {
                        kvp.Value.SetParent(parent, worldPositionStays: false);
                    }
                }
            }
        }
    }
}
