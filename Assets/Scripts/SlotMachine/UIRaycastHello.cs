using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRaycastHello : MonoBehaviour
{
    [Header("Required scene references")]
    [SerializeField] private GraphicRaycaster graphicRaycaster; // On your Canvas
    [SerializeField] private EventSystem eventSystem;            // In your scene
    [SerializeField] private Image targetImage;                  // The UI Image to match

    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
    private PointerEventData pointerData;

    private void Reset()
    {
        // Auto-fill common references when adding the component
        if (graphicRaycaster == null)
            graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
        if (eventSystem == null)
            eventSystem = FindObjectOfType<EventSystem>();
    }

    private void Update()
    {
        // Trigger on left mouse button down (you can change this as needed)
        if (!Input.GetMouseButtonDown(0))
            return;

        if (graphicRaycaster == null || eventSystem == null || targetImage == null)
        {
            Debug.LogWarning("Missing reference(s). Please assign GraphicRaycaster, EventSystem, and targetImage.");
            return;
        }

        // 1) Prepare a PointerEventData at the current mouse position
        pointerData ??= new PointerEventData(eventSystem);
        pointerData.position = Input.mousePosition;

        // 2) Raycast into the Canvas
        raycastResults.Clear();
        graphicRaycaster.Raycast(pointerData, raycastResults);

        // 3) Check if any hit is exactly the targetImage's GameObject
        for (int i = 0; i < raycastResults.Count; i++)
        {
            if (raycastResults[i].gameObject == targetImage.gameObject)
            {
                Debug.Log("Hello, world!");
                return; // Stop at first match
            }
        }
    }
}
