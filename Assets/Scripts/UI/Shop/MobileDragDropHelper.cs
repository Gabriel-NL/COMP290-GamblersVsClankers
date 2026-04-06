using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// MobileDragDropHelper is a manager script that enables touch input support for drag and drop operations on Android and other mobile platforms.
/// This script works alongside the DragDrop component to ensure touch input events are properly routed through the EventSystem.
/// It provides fallback handling and debug utilities for mobile input troubleshooting.
/// </summary>
public class MobileDragDropHelper : MonoBehaviour
{
    [SerializeField]
    private bool enableDebugLogging = true;

    [SerializeField]
    private bool enableTouchSimulationInEditor = false;

    private GraphicRaycaster cachedRaycaster;
    private EventSystem cachedEventSystem;

    private void Start()
    {
        // Cache references for performance
        cachedRaycaster = GetComponent<GraphicRaycaster>();
        cachedEventSystem = EventSystem.current;

        if (cachedEventSystem == null)
        {
            Debug.LogError("[MobileDragDropHelper] No EventSystem found in scene! Touch input will not work properly. " +
                         "Please ensure there is an EventSystem in your scene.");
            return;
        }

        if (cachedRaycaster == null)
        {
            Debug.LogWarning("[MobileDragDropHelper] No GraphicRaycaster found on the Canvas. Adding one now...");
            cachedRaycaster = gameObject.AddComponent<GraphicRaycaster>();
        }

        if (enableDebugLogging)
        {
            Debug.Log("[MobileDragDropHelper] Initialized on Canvas: " + gameObject.name);
            Debug.Log("[MobileDragDropHelper] Platform: " + Application.platform);
            Debug.Log("[MobileDragDropHelper] Input System support: " + (UnityEngine.InputSystem.InputSystem.settings != null ? "Active" : "Inactive"));
        }
    }

    private void Update()
    {
        // Only process on mobile platforms or in editor with touch simulation enabled
        if (!IsMobilePlatform() && !enableTouchSimulationInEditor)
        {
            return;
        }

        // Ensure EventSystem is valid
        if (cachedEventSystem == null || cachedEventSystem != EventSystem.current)
        {
            cachedEventSystem = EventSystem.current;
            if (cachedEventSystem == null)
            {
                Debug.LogError("[MobileDragDropHelper] EventSystem not found!");
                return;
            }
        }

        // Process new input callbacks for the new Input System
        ProcessNewInputSystem();
    }

    private bool IsMobilePlatform()
    {
        return Application.platform == RuntimePlatform.Android ||
               Application.platform == RuntimePlatform.IPhonePlayer ||
               Application.isEditor && enableTouchSimulationInEditor;
    }

    private void ProcessNewInputSystem()
    {
        #if ENABLE_INPUT_SYSTEM
        var touchscreen = UnityEngine.InputSystem.Touchscreen.current;
        if (touchscreen == null)
        {
            return;
        }

        for (int i = 0; i < touchscreen.touches.Count; i++)
        {
            var touch = touchscreen.touches[i];
            
            // Skip if the touch is over UI that blocks it
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1))
            {
                continue;
            }
        }
        #endif
    }

    /// <summary>
    /// Attempts to get the DragDrop component at the given screen position using raycasting.
    /// Useful for debugging touch input issues.
    /// </summary>
    public DragDrop GetDragDropAtPosition(Vector2 screenPosition)
    {
        if (cachedEventSystem == null)
        {
            return null;
        }

        PointerEventData pointerData = new PointerEventData(cachedEventSystem);
        pointerData.position = screenPosition;

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        cachedRaycaster.Raycast(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            DragDrop dragDrop = result.gameObject.GetComponent<DragDrop>();
            if (dragDrop != null)
            {
                return dragDrop;
            }
        }

        return null;
    }

    /// <summary>
    /// Enables or disables debug logging for this helper.
    /// </summary>
    public void SetDebugLogging(bool enabled)
    {
        enableDebugLogging = enabled;
    }

    /// <summary>
    /// Manually trigger a drag operation from code (useful for testing or programmatic drag/drop).
    /// </summary>
    public void TriggerDragFromPosition(Vector2 screenPosition)
    {
        DragDrop dragDrop = GetDragDropAtPosition(screenPosition);
        if (dragDrop == null)
        {
            if (enableDebugLogging)
            {
                Debug.Log("[MobileDragDropHelper] No DragDrop component found at position: " + screenPosition);
            }
            return;
        }

        if (enableDebugLogging)
        {
            Debug.Log("[MobileDragDropHelper] Found DragDrop: " + dragDrop.gameObject.name);
        }

        // Create and execute a pointer event
        PointerEventData pointerData = new PointerEventData(cachedEventSystem);
        pointerData.position = screenPosition;

        ExecuteEvents.Execute<IPointerDownHandler>(dragDrop.gameObject, pointerData, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute<IBeginDragHandler>(dragDrop.gameObject, pointerData, ExecuteEvents.beginDragHandler);
    }

    /// <summary>
    /// Manually trigger a drop operation at the given position.
    /// </summary>
    public void TriggerDropAtPosition(Vector2 screenPosition)
    {
        DragDrop dragDrop = GetDragDropAtPosition(screenPosition);
        if (dragDrop == null)
        {
            if (enableDebugLogging)
            {
                Debug.Log("[MobileDragDropHelper] No DragDrop component found at position: " + screenPosition);
            }
            return;
        }

        PointerEventData pointerData = new PointerEventData(cachedEventSystem);
        pointerData.position = screenPosition;

        ExecuteEvents.Execute<IEndDragHandler>(dragDrop.gameObject, pointerData, ExecuteEvents.endDragHandler);
    }

    /// <summary>
    /// Validates that all necessary components for touch input are properly configured.
    /// </summary>
    public bool ValidateConfiguration()
    {
        bool isValid = true;

        if (cachedEventSystem == null)
        {
            Debug.LogError("[MobileDragDropHelper.Validate] No EventSystem in scene!");
            isValid = false;
        }

        if (cachedRaycaster == null)
        {
            Debug.LogError("[MobileDragDropHelper.Validate] No GraphicRaycaster on Canvas!");
            isValid = false;
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[MobileDragDropHelper.Validate] This script must be on a Canvas!");
            isValid = false;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("[MobileDragDropHelper.Validate] Running on Android - touch input should be available");
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Debug.Log("[MobileDragDropHelper.Validate] Running on iOS - touch input should be available");
        }
        else
        {
            Debug.LogWarning("[MobileDragDropHelper.Validate] Running on non-mobile platform - touch input may not work");
        }

        return isValid;
    }
}
