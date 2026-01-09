using RDLevelEditor;

namespace RDEditorPlus.Util
{
    public static class SpriteUtil
    {
        public static bool TryGetRowInCurrentTab(string target, out int row)
        {
            row = 0;

            foreach (LevelEvent_MakeSprite sprite in scnEditor.instance.currentPageSpritesData)
            {
                if (sprite.target == target)
                {
                    return true;
                }

                row++;
            }

            return false;
        }
    }
}
