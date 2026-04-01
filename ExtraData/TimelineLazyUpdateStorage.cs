using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.ExtraData
{
    public struct TimelineLazyUpdateStorage
    {
        public TimelineLazyUpdateStorage()
        {
            lastTab = Tab.None;
        }

        public bool ShouldUpdate(Timeline timeline)
        {
            if (timeline.shouldUpdateUI || lastTab != CurrentTab
                || lastPage != CurrentPage || lastPosition != CurrentPosition(timeline))
            {
                lastPosition = CurrentPosition(timeline);
                lastTab = CurrentTab;
                lastPage = CurrentPage;
                return true;
            }

            return false;
        }

        private Vector2 lastPosition;
        private Tab lastTab;
        private int lastPage;

        private static Vector2 CurrentPosition(Timeline timeline) => timeline.scrollviewContent.anchoredPosition;
        private static Tab CurrentTab => scnEditor.instance.currentTab;
        private static int CurrentPage => scnEditor.instance.currentTabSection.pageIndex;
    }
}
