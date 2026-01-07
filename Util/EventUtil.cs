using RDLevelEditor;

namespace RDEditorPlus.Util
{
    public static class EventUtil
    {
        public static bool IsSpriteEvent(this LevelEvent_Base levelEvent)
        {
            return levelEvent.isSpriteTabEvent;
        }

        // Dummy
        public static bool IsSoundEvent(this LevelEvent_Base levelEvent)
        {
            return false;
        }

        public static bool IsRowEvent(this LevelEvent_Base levelEvent)
        {
            return levelEvent.isClassicRowEvent || levelEvent.isOneshotRowEvent;
        }

        // Dummy
        public static bool IsRoomEvent(this LevelEvent_Base levelEvent)
        {
            return false;
        }

        // Dummy
        public static bool IsActionEvent(this LevelEvent_Base levelEvent)
        {
            return false;
        }

        // Dummy
        public static bool IsWindowEvent(this LevelEvent_Base levelEvent)
        {
            return false;
        }
    }
}
