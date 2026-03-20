using UnityEngine;
using UnityEngine.EventSystems;

public class ShopDragStarter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ShopINIT shop;
    public int slotIndex;

    private GameObject activeDraggedObject;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (shop == null)
        {
            Debug.LogWarning("ShopDragStarter: shop reference is null!");
            return;
        }

        activeDraggedObject = shop.TrySpawnOwnedSoldierForDrag(slotIndex, eventData);

        if (activeDraggedObject == null)
        {
            return;
        }

        GameObject dragHandler = ExecuteEvents.GetEventHandler<IDragHandler>(activeDraggedObject);
        if (dragHandler != null)
        {
            eventData.pointerDrag = dragHandler;
            EventSystem.current.SetSelectedGameObject(dragHandler);

            ExecuteEvents.Execute(dragHandler, eventData, ExecuteEvents.beginDragHandler);
            ExecuteEvents.Execute(dragHandler, eventData, ExecuteEvents.dragHandler);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (activeDraggedObject == null)
        {
            return;
        }

        GameObject dragHandler = ExecuteEvents.GetEventHandler<IDragHandler>(activeDraggedObject);
        if (dragHandler != null)
        {
            ExecuteEvents.Execute(dragHandler, eventData, ExecuteEvents.dragHandler);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (activeDraggedObject == null)
        {
            return;
        }

        GameObject dragHandler = ExecuteEvents.GetEventHandler<IEndDragHandler>(activeDraggedObject);
        if (dragHandler != null)
        {
            ExecuteEvents.Execute(dragHandler, eventData, ExecuteEvents.endDragHandler);
        }

        activeDraggedObject = null;
    }
}