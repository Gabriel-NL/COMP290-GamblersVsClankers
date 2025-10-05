using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to a Button GameObject to repeat a Shop click while the pointer is held down.
/// The component calls ShopINIT.OnSoldierClickByIndex(slotIndex) immediately on press,
/// waits initialDelay seconds, then repeats every repeatInterval seconds until release.
/// </summary>
public class HoldClickRepeater : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public ShopINIT shop;
    public int slotIndex;

    [Tooltip("Delay (seconds) before repeating after the first immediate click")]
    public float initialDelay = 0.4f;
    [Tooltip("Interval (seconds) between repeated clicks while held")]
    public float repeatInterval = 0.12f;

    Coroutine repeatCoroutine;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (shop == null) return;

        // Stop any running coroutine just in case
        if (repeatCoroutine != null) StopCoroutine(repeatCoroutine);
        repeatCoroutine = StartCoroutine(HoldAndRepeat());
    }

    public void OnPointerUp(PointerEventData eventData) => StopRepeat();

    public void OnPointerExit(PointerEventData eventData) => StopRepeat();

    IEnumerator HoldAndRepeat()
    {
        // Wait initial delay; if pointer is released before this, coroutine will be stopped and nothing happens.
        yield return new WaitForSeconds(initialDelay);

        // Mark to suppress the next single click (so releasing after a hold doesn't trigger a single increment)
        shop.SuppressNextClick(slotIndex);

        // Spawn a draggable copy of the soldier and begin drag so the player can place it.
        GameObject dragObj = shop.SpawnSoldierForDrag(slotIndex);
        if (dragObj != null && EventSystem.current != null)
        {
            // Create a PointerEventData at the current mouse position
            var pe = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition,
                pressPosition = Input.mousePosition
            };

            // Raycast to find the actual UI element under the pointer (should include our new object)
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pe, results);

            if (results.Count > 0)
            {
                // Use the topmost raycast hit
                var top = results[0];

                // Send pointer down to the hit object (this should set pointerPress etc. internally)
                ExecuteEvents.Execute(top.gameObject, pe, ExecuteEvents.pointerDownHandler);

                // Find an object that can handle drag (IDragHandler) starting from the top hit
                GameObject dragHandler = ExecuteEvents.GetEventHandler<IDragHandler>(top.gameObject);
                if (dragHandler != null)
                {
                    // Inform EventSystem which object is being dragged
                    pe.pointerDrag = dragHandler;
                    // Mark as pressed on this object
                    pe.pointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(top.gameObject);

                    // Set selected object
                    EventSystem.current.SetSelectedGameObject(dragHandler);

                    // BeginDrag on the drag handler
                    ExecuteEvents.Execute(dragHandler, pe, ExecuteEvents.beginDragHandler);

                    // Finally, send an initial drag event so OnDrag receives a delta if any
                    ExecuteEvents.Execute(dragHandler, pe, ExecuteEvents.dragHandler);
                }
                else
                {
                    // Fall back: execute begin drag on the top hit's hierarchy
                    ExecuteEvents.ExecuteHierarchy(top.gameObject, pe, ExecuteEvents.beginDragHandler);
                    ExecuteEvents.ExecuteHierarchy(top.gameObject, pe, ExecuteEvents.dragHandler);
                }
            }
        }
    }

    void StopRepeat()
    {
        if (repeatCoroutine != null)
        {
            StopCoroutine(repeatCoroutine);
            repeatCoroutine = null;
        }
    }
}
