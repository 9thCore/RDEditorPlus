using RDLevelEditor;
using System;
using System.Linq;
using UnityEngine;

namespace RDEditorPlus.Util
{
    public static class RoomUtil
    {
        public static RoomsUsage GetOnTopVariant(this RoomsUsage usage)
        {
            return usage switch
            {
                RoomsUsage.OneRoom => RoomsUsage.OneRoomOrOnTop,
                RoomsUsage.OneRoomOrOnTop => RoomsUsage.OneRoomOrOnTop,
                RoomsUsage.ManyRooms => RoomsUsage.ManyRoomsAndOnTop,
                RoomsUsage.ManyRoomsAndOnTop => RoomsUsage.ManyRoomsAndOnTop,
                _ => RoomsUsage.NotUsed
            };
        }

        public static bool DoSelectedEventsHaveTheSameRoomsUsage()
        {
            return TryGetSelectedEventsRoomsUsage(out _);
        }

        public static bool TryGetSelectedEventsRoomsUsage(out RoomsUsage usage)
        {
            var controls = scnEditor.instance.selectedControls;

            if (controls.Count == 0)
            {
                usage = default;
                return false;
            }
            else if (controls.Count == 1)
            {
                usage = scnEditor.instance.selectedControl.levelEvent.roomsUsage;
                return true;
            }

            RoomsUsage capturedUsage = controls[0].levelEvent.roomsUsage.GetOnTopVariant();
            usage = capturedUsage;
            return controls.All(control => control.levelEvent.roomsUsage.GetOnTopVariant() == capturedUsage);
        }

        public static Usage[] GetSelectedEventsUsage()
        {
            const int RoomCountWithOnTop = RDEditorConstants.RoomCount + 1;

            Usage[] usage = new Usage[RoomCountWithOnTop];

            for (int i = 0; i < RoomCountWithOnTop; i++)
            {
                usage[i] = (Usage) UsedByAllFragment;
            }

            foreach (LevelEventControl_Base eventControl in scnEditor.instance.selectedControls)
            {
                int[] rooms = eventControl.levelEvent.rooms;

                for (int i = 0; i < RoomCountWithOnTop; i++)
                {
                    if (Array.IndexOf(rooms, i) != -1)
                    {
                        usage[i] |= Usage.UsedBySome;
                    }
                    else
                    {
                        usage[i] &= (Usage) UsedByAllRemover;
                    }
                }
            }

            return usage;
        }

        public static Color GetRoomUsageColor(Usage usage)
        {
            return usage switch
            {
                Usage.UsedByAll => InspectorUtil.RoomUsedColor,
                Usage.UsedBySome => InspectorUtil.RoomHalfUsedColor,
                _ => InspectorUtil.RoomUnusedColor,
            };
        }

        [Flags]
        public enum Usage
        {
            UsedByNone = 0,
            UsedBySome = 1,
            UsedByAll = UsedBySome | UsedByAllFragment
        }

        public static Sprite OverlaySprite
        {
            get
            {
                if (overlaySprite == null)
                {
                    Texture2D texture = new(1, 1);
                    texture.SetPixel(0, 0, Color.black.WithAlpha(0.375f));
                    texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);

                    overlaySprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.zero);
                }

                return overlaySprite;
            }
        }

        private static Sprite overlaySprite = null;

        private const int UsedByAllFragment = 2;
        private const int UsedByAllRemover = ~UsedByAllFragment;
    }
}
