using RDLevelEditor;

namespace RDEditorPlus.Util
{
    public static class SpriteUtil
    {
        public static bool TryGetSprite(string target, out LevelEvent_MakeSprite sprite)
        {
            foreach (LevelEvent_MakeSprite makeSprite in scnEditor.instance.spritesData)
            {
                if (makeSprite.target == target)
                {
                    sprite = makeSprite;
                    return true;
                }
            }

            sprite = null;
            return false;
        }

        public static bool TryGetRowInRoom(string target, int room, out int row)
        {
            row = 0;

            foreach (LevelEvent_MakeSprite sprite in scnEditor.instance.spritesData)
            {
                if (sprite.target == target)
                {
                    return true;
                } else if (sprite.room == room)
                {
                    row += 1;
                }
            }

            return false;
        }
    }
}
