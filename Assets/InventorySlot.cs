using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {   if (transform.childCount == 0)
        {
        GameObject dropped = eventData.pointerDrag;
        DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
        draggableItem.parentAfterDrag = transform; 
        dropped.transform.SetParent(transform); 
        dropped.transform.localPosition = Vector3.zero; 
        RectTransform slotRectTransform = GetComponent<RectTransform>();
            LayoutElement layoutElement = dropped.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = dropped.AddComponent<LayoutElement>();
            }
            
            // Set the preferred dimensions to match the slot's dimensions
            layoutElement.preferredWidth = slotRectTransform.rect.width;
            layoutElement.preferredHeight = slotRectTransform.rect.height;

            // Optionally, reset the local scale of the dropped item to ensure it's not scaled
            dropped.transform.localScale = Vector3.one;
        }
    }
}
