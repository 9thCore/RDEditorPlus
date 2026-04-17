using RDEditorPlus.Functionality.Mixins;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RDEditorPlus.Util
{
    public static class ExtensibleUIListUtil
    {
        public static void EnsureListElements<ListElement>(this IExtensibleUIList<ListElement> instance, int count)
            where ListElement : IExtensibleUIList<ListElement>.IListElement
        {
            if (instance.VariableList.Count < count)
            {
                for (int i = instance.VariableList.Count; i < count; i++)
                {
                    instance.VariableList.Add(instance.CreateElement(instance.VariableList.Count));
                }
            }

            for (int i = 0; i < count; i++)
            {
                instance.VariableList[i].SetActive(true);
                instance.VariableList[i].SetDownArrowVisibility(true);
            }

            for (int i = count; i < instance.VariableList.Count; i++)
            {
                instance.VariableList[i].SetActive(false);
            }

            if (instance.TryFindLastActiveElement(out var element))
            {
                element.SetDownArrowVisibility(false);
            }
        }

        public static bool TryFindLastActiveElement<ListElement, T>(this IExtensibleUIList<ListElement> instance, out T element)
            where ListElement : IExtensibleUIList<ListElement>.IListElement
            where T : ListElement
        {
            if (!instance.VariableList.Any(element => element.Active))
            {
                element = default;
                return false;
            }

            element = (T) instance.VariableList.Last(element => element.Active);
            return true;
        }

        public static bool TryFindLastActiveElement<ListElement>(this IExtensibleUIList<ListElement> instance, out ListElement element)
            where ListElement : IExtensibleUIList<ListElement>.IListElement
        {
            if (!instance.VariableList.Any(element => element.Active))
            {
                element = default;
                return false;
            }

            element = instance.VariableList.Last(element => element.Active);
            return true;
        }

        public static void UpdateContentSize<ListElement>(this IExtensibleUIList<ListElement> instance, float startY,
            float creatorOffset, float scrollStart)
            where ListElement : IExtensibleUIList<ListElement>.IListElement
        {
            if (!instance.TryFindLastActiveElement(out var element))
            {
                instance.ContentRectTransform.SizeDeltaY(0f);
                instance.CreatorElement.MoveTo(startY + creatorOffset);
                return;
            }

            var position = element.Position;
            instance.CreatorElement.MoveTo(position + creatorOffset);

            float size = Math.Max(0f, scrollStart - position);
            instance.ContentRectTransform.SizeDeltaY(size);
        }

        public static void CreateButtons<ListElement>(this IExtensibleUIList<ListElement> instance, Transform parent, int index,
            float leftElementPadding, float rightElementPadding, float elementHeight, float buttonElementSpacing, float deleteIconPadding,
            float orderIconPadding, ColorBlock deleteColorBlock, ColorBlock orderColorBlock, UnityAction onDelete, UnityAction onDown, UnityAction onUp,
            out Button delete, out Button up, out Button down)
            where ListElement : IExtensibleUIList<ListElement>.IListElement
        {
            Vector2 size = new(elementHeight, elementHeight);
            Vector2 deleteAnchor = Vector2.zero;
            Vector2 orderAnchor = new(1f, 0f);
            Vector4 deletePadding = new(deleteIconPadding, deleteIconPadding, -deleteIconPadding, -deleteIconPadding);
            Vector4 orderPadding = new(orderIconPadding, orderIconPadding, -orderIconPadding, -orderIconPadding);

            UnityUtil.CreateButton(parent, AssetUtil.PulseTrashSprite, new Vector2(leftElementPadding / 2f, elementHeight / 2f), size,
                deleteAnchor, deleteAnchor, deletePadding, onDelete, out delete, out _);
            delete.colors = deleteColorBlock;

            UnityUtil.CreateButton(parent, AssetUtil.RowDownArrowSprite,
                new Vector2(-rightElementPadding + buttonElementSpacing + elementHeight / 2f, elementHeight / 2f), size, orderAnchor,
                orderAnchor, orderPadding, onDown, out down, out _);
            down.colors = orderColorBlock;

            up = null;
            if (index > 0)
            {
                UnityUtil.CreateButton(parent, AssetUtil.RowDownArrowSprite,
                    new Vector2(-rightElementPadding + buttonElementSpacing * 2f + elementHeight * 3f / 2f, elementHeight / 2f), size, orderAnchor,
                    orderAnchor, orderPadding, onUp, out up, out var image);
                up.colors = orderColorBlock;

                image.transform.Rotate(0f, 0f, 180f);
            }
        }
    }
}
