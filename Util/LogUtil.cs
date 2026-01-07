using RDLevelEditor;

namespace RDEditorPlus.Util
{
    public static class LogUtil
    {
        public static string Event(LevelEvent_Base levelEvent)
        {
            if (levelEvent.usesY)
            {
                return $"{levelEvent.type}@(Bar: {levelEvent.bar}, Beat: {levelEvent.beat}, y: {levelEvent.y})";
            }
            return $"{levelEvent.type}@(Bar: {levelEvent.bar}, Beat: {levelEvent.beat}, target: {levelEvent.target})";
        }
    }
}
