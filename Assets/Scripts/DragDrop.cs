using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    // Made public so runtime-created drag objects can set the Canvas reference.
    public Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 pointerOffset;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Ensure there's a CanvasGroup available so we can toggle raycasts during drag.
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        // Convert screen point to local point on the canvas RectTransform and apply offset so the
        // object follows the pointer exactly where it was grabbed.
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint - pointerOffset;
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
        // compute the initial offset between the pointer and the rectTransform pivot so drag is smooth
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 localPoint))
        {
            pointerOffset = localPoint - rectTransform.anchoredPosition;
        }
        else
        {
            pointerOffset = Vector2.zero;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
        // Perform a UI raycast at the release point to see if we dropped onto an ItemSlot
        if (EventSystem.current != null)
        {
            PointerEventData pe = new PointerEventData(EventSystem.current)
            {
                position = eventData.position
            };
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pe, results);

            foreach (var r in results)
            {
                var slot = r.gameObject.GetComponentInParent<ItemSlot>();
                if (slot != null)
                {
                    // Lock this object to the slot: make it a child and snap to the slot rect
                    RectTransform slotRect = slot.GetComponent<RectTransform>();
                    if (slotRect != null)
                    {
                        transform.SetParent(slotRect, false);
                        rectTransform.anchoredPosition = Vector2.zero;
                        // Optionally disable further dragging by removing this component
                        Destroy(this);
                        Debug.Log("Locked spawned soldier to slot: " + slot.gameObject.name);
                        return;
                    }
                }
            }
        }

        // Not dropped on an ItemSlot — if this came from the shop, return one to the counter then destroy
        var info = GetComponent<ShopSpawnInfo>();
        if (info != null && info.shop != null)
        {
            info.shop.IncrementSoldierByIndex(info.slotIndex);
            Destroy(info.gameObject);
            return;
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
        // Bring this object to front so it renders above other UI while dragging
        transform.SetAsLastSibling();
    }
}
