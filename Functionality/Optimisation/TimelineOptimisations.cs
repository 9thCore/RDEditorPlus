using RDLevelEditor;
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

        public void PreChangePage(TabSection section) => Unparent(section, entireTab: false);
        public void PostChangePage(TabSection section) => Parent(section, entireTab: false);

        public void Parent(TabSection section, bool entireTab) => SetParent(section, scrollViewVertContent, entireTab);
        public void Unparent(TabSection section, bool entireTab) => SetParent(section, null, entireTab);

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

        private Transform scrollViewVertContent;
    }
}
