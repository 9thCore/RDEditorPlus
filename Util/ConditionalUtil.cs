using RDLevelEditor;
using UnityEngine;

namespace RDEditorPlus.Util
{
    public static class ConditionalUtil
    {
        public static MultiEditUsageType IsUsedMultiEdit(int id, out UsageType usage)
        {
            usage = UsageType.Unused;

            bool usedByNone = true;
            bool usedByAll = true;

            foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
            {
                bool? negated = eventControl.levelEvent.HasConditional(id);

                if (negated.HasValue)
                {
                    usedByNone = false;
                    usage |= negated.Value ? UsageType.Negated : UsageType.Normal;
                }
                else
                {
                    usedByAll = false;
                }
            }

            return usedByNone ? MultiEditUsageType.UsedByNone : usedByAll ? MultiEditUsageType.UsedByAll : MultiEditUsageType.UsedBySome;
        }

        public static MultiEditUsageType IsUsedMultiEdit(string globalID, out UsageType usage)
        {
            usage = UsageType.Unused;

            bool usedByNone = true;
            bool usedByAll = true;

            foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
            {
                bool? negated = eventControl.levelEvent.HasGlobalConditional(globalID);

                if (negated.HasValue)
                {
                    usedByNone = false;
                    usage |= negated.Value ? UsageType.Negated : UsageType.Normal;
                }
                else
                {
                    usedByAll = false;
                }
            }

            return usedByNone ? MultiEditUsageType.UsedByNone : (usedByAll ? MultiEditUsageType.UsedByAll : MultiEditUsageType.UsedBySome);
        }

        public static Sprite UsedBySomeMaskSprite
        {
            get
            {
                if (usedBySomeMaskSprite == null)
                {
                    Texture2D texture = new(UsedBySomeMaskSize, UsedBySomeMaskSize);
                    Color visible = Color.white;
                    Color invisible = visible.WithAlpha(0f);

                    for (int i = 0; i < UsedBySomeMaskSize; i++)
                    {
                        for (int j = 0; j < UsedBySomeMaskSize / 2; j++)
                        {
                            texture.SetPixel(j, i, invisible);
                            texture.SetPixel(j + UsedBySomeMaskSize / 2, i, visible);
                        }
                    }

                    texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);

                    usedBySomeMaskSprite = Sprite.Create(texture, new Rect(0, 0, UsedBySomeMaskSize, UsedBySomeMaskSize), Vector2.zero);
                }

                return usedBySomeMaskSprite;
            }
        }

        public enum MultiEditUsageType
        {
            UsedByNone,
            UsedBySome,
            UsedByAll
        }

        public enum UsageType
        {
            Unused = 0,
            Normal = 1,
            Negated = 2,
            Mixed = Normal | Negated
        }

        public const int UsedBySomeMaskSize = 8;

        private static Sprite usedBySomeMaskSprite = null;
    }
}
