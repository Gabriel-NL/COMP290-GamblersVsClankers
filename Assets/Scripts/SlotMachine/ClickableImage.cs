using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickableImage : MonoBehaviour, IPointerClickHandler
{
    [MustBeAssigned] public UnityEvent onClick;
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Hello, world!");
        onClick.Invoke();
        // do your thing here
    }
}
