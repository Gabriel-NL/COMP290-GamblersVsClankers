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

        // Perform a single decrement and finish (no repeated calls)
        shop.DecrementSoldierByIndex(slotIndex);
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
