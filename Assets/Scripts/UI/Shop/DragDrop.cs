using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    // Made public so runtime-created drag objects can set the Canvas reference.
    public Canvas canvas;
    [Header("Prefab to instantiate when dropped")]
    public GameObject soldierPrefab; // Prefab to instantiate when dropped on a slot
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 pointerOffset;
    private bool isUIElement;
    private Vector3 worldOffset;
    private Camera mainCamera;
    // If this DragDrop is a source (like the reward slot), create a draggable clone instead
    public bool isSource = false;
    public bool oneTimeUse = false; // If true and isSource=true, only allow one clone to be placed
    private bool hasBeenUsed = false; // Track if this one-time source has been used
    private bool isDragging = false; // Track if a drag is currently active
    public DragDrop parentSource = null; // Reference to the source that created this clone
    public UnityEngine.Sprite sourceSprite;
    public Color sourceColor = Color.white; // Color to apply to clones (for rarity tinting)
    public Color soldierColor = Color.white; // Color to apply to the instantiated prefab
    public SoldierTierList.TierEnum soldierTier = SoldierTierList.TierEnum.Common; // Tier to apply to the spawned soldier
    private GameObject activeClone;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        isUIElement = rectTransform != null;
        mainCamera = Camera.main;
        
        if (isUIElement)
        {
            // UI element setup
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        else
        {
            // World GameObject setup - ensure it has a collider for mouse detection
            Collider2D col = GetComponent<Collider2D>();
            if (col == null) 
            {
                BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    boxCol.size = sr.sprite.bounds.size;
                }
            }
        }
    }

    // Helper to create a draggable clone when this DragDrop is a source (so the source stays in place)
    private GameObject CreateDraggableClone(PointerEventData eventData)
    {
        // Find or create DragCanvas
        Canvas targetCanvas = canvas;
        if (targetCanvas == null)
        {
            var existing = GameObject.Find("DragCanvas");
            if (existing != null) targetCanvas = existing.GetComponent<Canvas>();
            else
            {
                GameObject dc = new GameObject("DragCanvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
                targetCanvas = dc.GetComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                targetCanvas.sortingOrder = 1000;
                dc.transform.SetParent(null);
            }
        }

        if (isUIElement)
        {
            var srcImage = GetComponent<UnityEngine.UI.Image>();
            Sprite sprite = srcImage != null ? srcImage.sprite : sourceSprite;
            Color color = srcImage != null ? srcImage.color : sourceColor;
            GameObject go = new GameObject(gameObject.name + "_Clone", typeof(RectTransform), typeof(UnityEngine.CanvasRenderer), typeof(UnityEngine.UI.Image));
            var rt = go.GetComponent<RectTransform>();
            go.transform.SetParent(targetCanvas.transform, false);
            var img = go.GetComponent<UnityEngine.UI.Image>();
            img.sprite = sprite;
            img.color = color; // Apply the color from the source
            img.raycastTarget = true;
            Vector2 localPoint;
            RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera, out localPoint))
            {
                rt.anchoredPosition = localPoint;
            }
            var dd = go.AddComponent<DragDrop>();
            dd.canvas = targetCanvas;
            dd.isSource = false;
            dd.parentSource = this; // Set reference to source
            dd.soldierPrefab = this.soldierPrefab;
            dd.soldierColor = color; // Pass the color to the clone so it can apply to instantiated prefab
            dd.soldierTier = this.soldierTier; // Pass the tier to the clone
            return go;
        }
        else
        {
            var srcSr = GetComponent<SpriteRenderer>();
            Sprite sprite = srcSr != null ? srcSr.sprite : sourceSprite;
            Color color = srcSr != null ? srcSr.color : sourceColor;
            Vector3 spawnWorld = mainCamera.ScreenToWorldPoint(eventData.position);
            spawnWorld.z = transform.position.z;
            GameObject go = new GameObject(gameObject.name + "_Clone", typeof(SpriteRenderer));
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color; // Apply the color from the source
            go.transform.position = spawnWorld;
            if (go.GetComponent<Collider2D>() == null) go.AddComponent<BoxCollider2D>();
            var dd = go.AddComponent<DragDrop>();
            dd.isSource = false;
            dd.parentSource = this; // Set reference to source
            dd.soldierPrefab = this.soldierPrefab;
            dd.soldierColor = color; // Pass the color to the clone so it can apply to instantiated prefab
            return go;
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        
        // If we're not actually dragging (e.g., blocked by oneTimeUse), do nothing
        if (!isDragging)
        {
            return;
        }

        // If we spawned an active clone for dragging, forward the drag event to it and return
        if (activeClone != null)
        {
            ExecuteEvents.Execute<IDragHandler>(activeClone, eventData, ExecuteEvents.dragHandler);
            return;
        }

        if (isUIElement)
        {
            // UI element drag logic
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 localPoint))
            {
                rectTransform.anchoredPosition = localPoint - pointerOffset;
            }
        }
        else
        {
            // World GameObject drag logic
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
            worldPos.z = transform.position.z; // Maintain Z position
            transform.position = worldPos - worldOffset;
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        
        // If this DragDrop is a source (e.g. reward slot), spawn a clone and forward begin-drag to it
        if (isSource)
        {
            // If this is a one-time use source and it's already been used, don't allow dragging
            if (oneTimeUse && hasBeenUsed)
            {
                Debug.Log("This reward has already been claimed - cannot drag again");
                isDragging = false; // Mark that we're NOT dragging
                return;
            }
            
            // create clone on DragCanvas and forward begin-drag
            activeClone = CreateDraggableClone(eventData);
            if (activeClone != null)
            {
                isDragging = true; // Mark that we ARE dragging (via clone)
                // Forward the BeginDrag to the clone
                ExecuteEvents.Execute<IBeginDragHandler>(activeClone, eventData, ExecuteEvents.beginDragHandler);
                return;
            }
            // If clone couldn't be created, fall back to default behavior
        }

        isDragging = true; // Mark that we're dragging this object directly

        if (isUIElement)
        {
            // UI element begin drag logic
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out Vector2 localPoint))
            {
                pointerOffset = localPoint - rectTransform.anchoredPosition;
            }
            else
            {
                pointerOffset = Vector2.zero;
            }
        }
        else
        {
            // World GameObject begin drag logic
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
            worldPos.z = transform.position.z;
            worldOffset = worldPos - transform.position;
            
            // Bring sprite to front by adjusting sorting order
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder += 100; // Bring to front while dragging
            }
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");

        // If we're not actually dragging (e.g., blocked by oneTimeUse), do nothing
        if (!isDragging)
        {
            Debug.Log("OnEndDrag called but isDragging=false, ignoring");
            return;
        }

        // Reset the dragging flag
        isDragging = false;

        // If we have an active clone (spawned by a source), forward the EndDrag to it and destroy the clone
        if (activeClone != null)
        {
            ExecuteEvents.Execute<IEndDragHandler>(activeClone, eventData, ExecuteEvents.endDragHandler);
            Destroy(activeClone);
            activeClone = null;
            return;
        }

        if (isUIElement)
        {
            if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
        }
        else
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder -= 100;
        }

        ItemSlot targetSlot = null;

        // UI detection
        if (isUIElement && EventSystem.current != null)
        {
            PointerEventData pe = new PointerEventData(EventSystem.current) { position = eventData.position };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pe, results);
            Debug.Log($"UI Raycast found {results.Count} results");
            foreach (var r in results)
            {
                Debug.Log($"Raycast hit: {r.gameObject.name} (distance: {r.distance})");
                var slot = r.gameObject.GetComponentInParent<ItemSlot>();
                if (slot != null)
                {
                    targetSlot = slot;
                    Debug.Log($"Found UI ItemSlot: {slot.gameObject.name}");
                    break;
                }
                else Debug.Log($"No ItemSlot found on {r.gameObject.name} or its parents");
            }
        }

        // World detection
        if (!isUIElement)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0f;
            Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);
            if (hitCollider != null)
            {
                var slot = hitCollider.GetComponentInParent<ItemSlot>();
                if (slot != null)
                {
                    targetSlot = slot;
                    Debug.Log($"Found world ItemSlot via OverlapPoint: {slot.gameObject.name}");
                }
            }
            if (targetSlot == null)
            {
                RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
                if (hit.collider != null)
                {
                    var slot = hit.collider.GetComponentInParent<ItemSlot>();
                    if (slot != null)
                    {
                        targetSlot = slot;
                        Debug.Log($"Found world ItemSlot via Raycast: {slot.gameObject.name}");
                    }
                }
            }
            if (targetSlot == null) Debug.Log($"No ItemSlot found at world position: {worldPos}");
        }

        // If we found a slot, check if it's occupied first
        if (targetSlot != null)
        {
            // Check if the slot is already occupied
            if (targetSlot.IsOccupied())
            {
                Debug.Log($"Slot {targetSlot.gameObject.name} is already occupied - cannot place soldier here");
                
                // Return the soldier to shop counter if it came from the shop
                var shopInfo = GetComponent<ShopSpawnInfo>();
                if (shopInfo != null && shopInfo.shop != null)
                {
                    shopInfo.shop.IncrementSoldierByIndex(shopInfo.slotIndex);
                    Destroy(gameObject);
                    return;
                }
                
                // Otherwise just keep the item where it is
                Debug.Log("Item will remain in place (slot occupied)");
                return;
            }
            
            if (soldierPrefab != null)
            {
                Vector3 spawnPosition = targetSlot.transform.position;
                spawnPosition.z = 0f;
                Transform parentTransform = targetSlot.transform;

                GameObject spawnedSoldier = Instantiate(soldierPrefab, spawnPosition, Quaternion.identity);
                spawnedSoldier.transform.SetParent(parentTransform);
                var rb = spawnedSoldier.GetComponent<Rigidbody2D>();
                if (rb != null) { rb.bodyType = RigidbodyType2D.Kinematic; rb.velocity = Vector2.zero; }
                
                // Apply the tier to the spawned soldier BEFORE Start() is called
                var soldierBehaviour = spawnedSoldier.GetComponent<SoldierBehaviour>();
                if (soldierBehaviour != null)
                {
                    soldierBehaviour.tier = soldierTier;
                    Debug.Log($"Applied tier {soldierTier} to spawned soldier '{spawnedSoldier.name}'");
                    // Note: ApplyTierChanges() will be called automatically in Start()
                    
                    // Apply the rarity color to the soldier's sprite renderer
                    if (soldierBehaviour.spriteRenderer != null)
                    {
                        soldierBehaviour.spriteRenderer.color = soldierColor;
                        Debug.Log($"Applied tier color {soldierColor} to soldier's spriteRenderer");
                    }
                }
                
                // Also try to apply color to any other SpriteRenderer or Image components as fallback
                var soldierSr = spawnedSoldier.GetComponent<SpriteRenderer>();
                if (soldierSr != null)
                {
                    soldierSr.color = soldierColor;
                    Debug.Log($"Applied color {soldierColor} to spawned soldier SpriteRenderer (root)");
                }
                var soldierImg = spawnedSoldier.GetComponent<UnityEngine.UI.Image>();
                if (soldierImg != null)
                {
                    soldierImg.color = soldierColor;
                    Debug.Log($"Applied color {soldierColor} to spawned soldier Image");
                }
                
                // Notify difficulty manager that a soldier was placed
                if (DifficultyManager.instance != null)
                {
                    DifficultyManager.instance.OnSoldierPlaced();
                }
                
                // Mark the slot as occupied
                targetSlot.SetOccupied(spawnedSoldier);
                
                // If this clone came from a one-time-use source, mark the source as used
                if (parentSource != null && parentSource.oneTimeUse)
                {
                    parentSource.hasBeenUsed = true;
                    Debug.Log($"Parent source '{parentSource.gameObject.name}' marked as used (one-time only)");
                }
                
                Debug.Log($"Instantiated soldier prefab '{soldierPrefab.name}' at slot: {targetSlot.gameObject.name}");
                Destroy(gameObject);
                return;
            }
            else
            {
                if (isUIElement)
                {
                    RectTransform slotRect = targetSlot.GetComponent<RectTransform>();
                    if (slotRect != null)
                    {
                        transform.SetParent(slotRect, false);
                        rectTransform.anchoredPosition = Vector2.zero;
                        var image = GetComponent<UnityEngine.UI.Image>();
                        if (image != null) { Color color = image.color; color.a = 1.0f; image.color = color; }
                        
                        // Mark the slot as occupied
                        targetSlot.SetOccupied(gameObject);
                        
                        // If this clone came from a one-time-use source, mark the source as used
                        if (parentSource != null && parentSource.oneTimeUse)
                        {
                            parentSource.hasBeenUsed = true;
                            Debug.Log($"Parent source '{parentSource.gameObject.name}' marked as used (one-time only)");
                        }
                        
                        // Deselect before destroying to prevent Unity Editor errors
#if UNITY_EDITOR
                        if (UnityEditor.Selection.activeGameObject == gameObject)
                        {
                            UnityEditor.Selection.activeObject = null;
                        }
#endif
                        Destroy(gameObject);
                        Debug.Log("Locked UI element to slot: " + targetSlot.gameObject.name);
                        return;
                    }
                }
                else
                {
                    Vector3 slotWorldPos = targetSlot.transform.position;
                    slotWorldPos.z = transform.position.z;
                    transform.position = slotWorldPos;
                    transform.SetParent(targetSlot.transform);
                    var spriteRenderer = GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null) { Color color = spriteRenderer.color; color.a = 1.0f; spriteRenderer.color = color; }
                    
                    // Mark the slot as occupied
                    targetSlot.SetOccupied(gameObject);
                    
                    // If this clone came from a one-time-use source, mark the source as used
                    if (parentSource != null && parentSource.oneTimeUse)
                    {
                        parentSource.hasBeenUsed = true;
                        Debug.Log($"Parent source '{parentSource.gameObject.name}' marked as used (one-time only)");
                    }
                    
                    // Deselect before destroying to prevent Unity Editor errors
#if UNITY_EDITOR
                    if (UnityEditor.Selection.activeGameObject == gameObject)
                    {
                        UnityEditor.Selection.activeObject = null;
                    }
#endif
                    Destroy(gameObject);
                    Debug.Log("Locked world GameObject to slot: " + targetSlot.gameObject.name);
                    return;
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

        Debug.Log("Item was not dropped on a valid slot and will remain in place");
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
        
        if (isUIElement)
        {
            // Bring UI element to front
            transform.SetAsLastSibling();
        }
        else
        {
            // For world GameObjects, sorting order is handled in OnBeginDrag
            // Could add additional logic here if needed
        }
    }
}