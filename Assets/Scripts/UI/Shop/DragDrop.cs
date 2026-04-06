using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private const float SlotPaddingFactor = 0.95f;

    public Canvas canvas;

    [Header("Prefab to instantiate when dropped")]
    public GameObject soldierPrefab;

    [Header("Placed Visual Fit")]
    [Tooltip("Placed soldier visual will fit within this multiplier of the cell size. 1.0 = cell size, 1.1 = 110% of cell size.")]
    public float placedVisualFitMultiplier = 1.1f;

    [Header("Mobile Input Settings")]
    [Tooltip("Enable debug logging for mobile touch input")]
    public bool debugMobileInput = true;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 pointerOffset;
    private bool isUIElement;
    private Vector3 worldOffset;
    private Camera mainCamera;
    private SpriteRenderer cachedSpriteRenderer;
    private Image cachedImage;
    private ShopSpawnInfo cachedShopSpawnInfo;

    private readonly Vector3[] worldCornersBuffer = new Vector3[4];
    private readonly List<RaycastResult> raycastResultsBuffer = new List<RaycastResult>(16);
    private PointerEventData reusablePointerEventData;
    private EventSystem reusablePointerEventSystem;

    public bool isSource = false;
    public bool oneTimeUse = false;
    private bool hasBeenUsed = false;
    private bool isDragging = false;

    public DragDrop parentSource = null;

    public Sprite sourceSprite;
    public Color sourceColor = Color.white;
    public Color soldierColor = Color.white;
    public SoldierTierList.TierEnum soldierTier = SoldierTierList.TierEnum.Common;

    private GameObject activeClone;

    // Touch input tracking for mobile support
    private int activeTouchId = -1;
    private bool isTouchDragging = false;
    private Vector2 lastTouchPosition = Vector2.zero;

    public Action onBeginDragFromSource;
    public Action onCancelledOrInvalidDrop;
    public Action onSuccessfulDrop;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        isUIElement = rectTransform != null;
        mainCamera = Camera.main;
        cachedSpriteRenderer = GetComponent<SpriteRenderer>();
        cachedImage = GetComponent<Image>();
        cachedShopSpawnInfo = GetComponent<ShopSpawnInfo>();

        if (isUIElement)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        else
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
                if (cachedSpriteRenderer != null && cachedSpriteRenderer.sprite != null)
                {
                    boxCol.size = cachedSpriteRenderer.sprite.bounds.size;
                }
            }
        }

        if (debugMobileInput)
        {
            Debug.Log($"[DragDrop] Initialized on {gameObject.name}. Platform: {Application.platform}, UI Element: {isUIElement}");
        }
    }

    private void Update()
    {
        // Handle touch input for mobile (Android, iOS, etc.)
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        // Only process touch input if we have touches
        if (Input.touchCount == 0)
        {
            return;
        }

        Touch touch = Input.GetTouch(0);

        // Check if this is our active touch or if we should start tracking a new one
        if (activeTouchId == -1 && touch.phase == TouchPhase.Began)
        {
            // Check if the touch is over this object
            if (IsPointerOverObject(touch.position))
            {
                activeTouchId = touch.fingerId;
                lastTouchPosition = touch.position;

                if (debugMobileInput)
                {
                    Debug.Log($"[DragDrop] Touch began on {gameObject.name} at position {touch.position}");
                }

                // Trigger OnPointerDown equivalent
                HandleTouchBegin(touch.position);
            }
        }

        // Handle active touch
        if (activeTouchId == touch.fingerId)
        {
            switch (touch.phase)
            {
                case TouchPhase.Moved:
                    if (!isTouchDragging)
                    {
                        // Start dragging if touch has moved enough
                        float dragDistance = Vector2.Distance(touch.position, lastTouchPosition);
                        if (dragDistance > 5f) // Minimum drag threshold
                        {
                            isTouchDragging = true;
                            if (debugMobileInput)
                            {
                                Debug.Log($"[DragDrop] Touch drag started on {gameObject.name}");
                            }
                            HandleTouchDragBegin(touch.position);
                        }
                    }

                    if (isTouchDragging)
                    {
                        HandleTouchDrag(touch.position);
                        lastTouchPosition = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isTouchDragging)
                    {
                        if (debugMobileInput)
                        {
                            Debug.Log($"[DragDrop] Touch drag ended on {gameObject.name}");
                        }
                        HandleTouchDragEnd(touch.position);
                        isTouchDragging = false;
                    }
                    else if (debugMobileInput)
                    {
                        Debug.Log($"[DragDrop] Touch released without dragging on {gameObject.name}");
                    }

                    activeTouchId = -1;
                    break;
            }
        }
    }

    private bool IsPointerOverObject(Vector2 screenPosition)
    {
        if (isUIElement)
        {
            // For UI elements, use RectTransformUtility
            return RectTransformUtility.RectangleContainsScreenPoint(
                rectTransform,
                screenPosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera
            );
        }
        else
        {
            // For world objects, use physics raycast
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0f;

            Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);
            if (hitCollider != null && (hitCollider.gameObject == gameObject || hitCollider.transform.IsChildOf(transform)))
            {
                return true;
            }

            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
            return hit.collider != null && (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform));
        }
    }

    private void HandleTouchBegin(Vector2 touchPosition)
    {
        if (isUIElement)
        {
            transform.SetAsLastSibling();
        }
    }

    private void HandleTouchDragBegin(Vector2 touchPosition)
    {
        if (isSource)
        {
            if (oneTimeUse && hasBeenUsed)
            {
                if (debugMobileInput) Debug.Log("[DragDrop] This reward has already been claimed - cannot drag again");
                isTouchDragging = false;
                activeTouchId = -1;
                return;
            }

            // Create a pointer event data from touch position
            PointerEventData touchEventData = CreatePointerEventDataFromTouch(touchPosition);
            activeClone = CreateDraggableClone(touchEventData);
            if (activeClone != null)
            {
                isDragging = true;
                onBeginDragFromSource?.Invoke();
                ExecuteEvents.Execute<IBeginDragHandler>(activeClone, touchEventData, ExecuteEvents.beginDragHandler);
                return;
            }
        }

        isDragging = true;

        if (isUIElement)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                touchPosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPoint))
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
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(touchPosition);
            worldPos.z = transform.position.z;
            worldOffset = worldPos - transform.position;

            if (cachedSpriteRenderer != null)
            {
                cachedSpriteRenderer.sortingOrder += 100;
            }
        }
    }

    private void HandleTouchDrag(Vector2 touchPosition)
    {
        if (!isDragging)
        {
            return;
        }

        if (activeClone != null)
        {
            PointerEventData touchEventData = CreatePointerEventDataFromTouch(touchPosition);
            ExecuteEvents.Execute<IDragHandler>(activeClone, touchEventData, ExecuteEvents.dragHandler);
            return;
        }

        if (isUIElement)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                touchPosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPoint))
            {
                rectTransform.anchoredPosition = localPoint - pointerOffset;
            }
        }
        else
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(touchPosition);
            worldPos.z = transform.position.z;
            transform.position = worldPos - worldOffset;
        }
    }

    private void HandleTouchDragEnd(Vector2 touchPosition)
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;

        if (activeClone != null)
        {
            PointerEventData touchEventData = CreatePointerEventDataFromTouch(touchPosition);
            ExecuteEvents.Execute<IEndDragHandler>(activeClone, touchEventData, ExecuteEvents.endDragHandler);
            Destroy(activeClone);
            activeClone = null;
            return;
        }

        RestorePostDragState();

        PointerEventData endEventData = CreatePointerEventDataFromTouch(touchPosition);
        if (!TryGetTargetSlot(endEventData, out ItemSlot targetSlot))
        {
            HandleInvalidDrop();
            return;
        }

        if (targetSlot.IsOccupied())
        {
            HandleOccupiedSlot();
            return;
        }

        if (soldierPrefab != null)
        {
            SpawnSoldierInSlot(targetSlot);
            return;
        }

        PlaceExistingObjectInSlot(targetSlot);
    }

    private PointerEventData CreatePointerEventDataFromTouch(Vector2 touchPosition)
    {
        if (EventSystem.current == null)
        {
            Debug.LogError("[DragDrop] EventSystem.current is null! Cannot create PointerEventData.");
            return new PointerEventData(EventSystem.current);
        }

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = touchPosition;
        pointerEventData.pointerId = activeTouchId;
        return pointerEventData;
    }

    private Transform FindVisualChild(GameObject root)
    {
        if (root == null)
            return null;

        Transform[] allChildren = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child == root.transform)
                continue;

            if (string.Equals(child.name, "sprite", StringComparison.OrdinalIgnoreCase))
                return child;
        }

        SpriteRenderer sr = root.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            return sr.transform;

        return null;
    }

    private Vector2 GetRectTransformWorldSize(RectTransform rectTransformToMeasure)
    {
        rectTransformToMeasure.GetWorldCorners(worldCornersBuffer);

        float width = Vector3.Distance(worldCornersBuffer[0], worldCornersBuffer[3]);
        float height = Vector3.Distance(worldCornersBuffer[0], worldCornersBuffer[1]);

        return new Vector2(width, height);
    }

    private void FitVisualChildToSlot(GameObject spawnedSoldier, ItemSlot targetSlot)
    {
        if (spawnedSoldier == null || targetSlot == null)
            return;

        RectTransform slotRect = targetSlot.GetComponent<RectTransform>();
        if (slotRect == null)
        {
            Debug.LogWarning($"Target slot '{targetSlot.name}' has no RectTransform. Cannot fit visual child.");
            return;
        }

        Transform visualChild = FindVisualChild(spawnedSoldier);
        if (visualChild == null)
        {
            Debug.LogWarning($"No visual child named 'sprite' or SpriteRenderer found on '{spawnedSoldier.name}'.");
            return;
        }

        SpriteRenderer visualSprite = visualChild.GetComponent<SpriteRenderer>();
        if (visualSprite == null || visualSprite.sprite == null)
        {
            Debug.LogWarning($"Visual child '{visualChild.name}' has no SpriteRenderer/sprite on '{spawnedSoldier.name}'.");
            return;
        }

        Vector2 slotWorldSize = GetRectTransformWorldSize(slotRect);
        Vector2 targetWorldSize = slotWorldSize * placedVisualFitMultiplier;

        Vector3 originalVisualScale = visualChild.localScale;

        Bounds currentBounds = visualSprite.bounds;
        Vector2 currentWorldSize = new Vector2(currentBounds.size.x, currentBounds.size.y);

        if (currentWorldSize.x <= 0f || currentWorldSize.y <= 0f)
        {
            Debug.LogWarning($"Current rendered size invalid on '{visualChild.name}'.");
            return;
        }

        float scaleX = targetWorldSize.x / currentWorldSize.x;
        float scaleY = targetWorldSize.y / currentWorldSize.y;
        float scaleFactor = Mathf.Min(scaleX, scaleY);

        visualChild.localScale = originalVisualScale * scaleFactor;

        Bounds finalBounds = visualSprite.bounds;
        Vector2 finalWorldSize = new Vector2(finalBounds.size.x, finalBounds.size.y);

        Debug.Log(
            $"Fitted visual child '{visualChild.name}' of '{spawnedSoldier.name}' to slot '{targetSlot.name}'. " +
            $"slotWorld={slotWorldSize}, targetWorld={targetWorldSize}, before={currentWorldSize}, after={finalWorldSize}, scaleFactor={scaleFactor}"
        );
    }

    private GameObject CreateDraggableClone(PointerEventData eventData)
    {
        Canvas targetCanvas = canvas;
        if (targetCanvas == null)
        {
            GameObject existing = GameObject.Find("DragCanvas");
            if (existing != null)
            {
                targetCanvas = existing.GetComponent<Canvas>();
            }
            else
            {
                GameObject dc = new GameObject(
                    "DragCanvas",
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster)
                );

                targetCanvas = dc.GetComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                targetCanvas.sortingOrder = 1000;
                dc.transform.SetParent(null);
            }
        }

        if (isUIElement)
        {
            Sprite sprite = cachedImage != null ? cachedImage.sprite : sourceSprite;
            Color color = cachedImage != null ? cachedImage.color : sourceColor;

            GameObject go = new GameObject(
                gameObject.name + "_Clone",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            RectTransform rt = go.GetComponent<RectTransform>();
            go.transform.SetParent(targetCanvas.transform, false);

            Image img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = true;

            RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                eventData.position,
                targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
                out Vector2 localPoint))
            {
                rt.anchoredPosition = localPoint;
            }

            DragDrop dd = go.AddComponent<DragDrop>();
            dd.canvas = targetCanvas;
            dd.isSource = false;
            dd.parentSource = this;
            dd.soldierPrefab = soldierPrefab;
            dd.soldierColor = color;
            dd.soldierTier = soldierTier;

            return go;
        }
        else
        {
            Sprite sprite = cachedSpriteRenderer != null ? cachedSpriteRenderer.sprite : sourceSprite;
            Color color = cachedSpriteRenderer != null ? cachedSpriteRenderer.color : sourceColor;

            Vector3 spawnWorld = mainCamera.ScreenToWorldPoint(eventData.position);
            spawnWorld.z = transform.position.z;

            GameObject go = new GameObject(gameObject.name + "_Clone", typeof(SpriteRenderer));
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            go.transform.position = spawnWorld;

            if (go.GetComponent<Collider2D>() == null)
            {
                go.AddComponent<BoxCollider2D>();
            }

            DragDrop dd = go.AddComponent<DragDrop>();
            dd.isSource = false;
            dd.parentSource = this;
            dd.soldierPrefab = soldierPrefab;
            dd.soldierColor = color;
            dd.soldierTier = soldierTier;

            return go;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");

        if (isUIElement)
        {
            transform.SetAsLastSibling();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");

        if (isSource)
        {
            if (oneTimeUse && hasBeenUsed)
            {
                Debug.Log("This reward has already been claimed - cannot drag again");
                isDragging = false;
                return;
            }

            activeClone = CreateDraggableClone(eventData);
            if (activeClone != null)
            {
                isDragging = true;
                onBeginDragFromSource?.Invoke();
                ExecuteEvents.Execute<IBeginDragHandler>(activeClone, eventData, ExecuteEvents.beginDragHandler);
                return;
            }
        }

        isDragging = true;

        if (isUIElement)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                eventData.position,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPoint))
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
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
            worldPos.z = transform.position.z;
            worldOffset = worldPos - transform.position;

            if (cachedSpriteRenderer != null)
            {
                cachedSpriteRenderer.sortingOrder += 100;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("OnDrag");

        if (!isDragging)
        {
            return;
        }

        if (activeClone != null)
        {
            ExecuteEvents.Execute<IDragHandler>(activeClone, eventData, ExecuteEvents.dragHandler);
            return;
        }

        if (isUIElement)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                eventData.position,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out Vector2 localPoint))
            {
                rectTransform.anchoredPosition = localPoint - pointerOffset;
            }
        }
        else
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
            worldPos.z = transform.position.z;
            transform.position = worldPos - worldOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");

        if (!isDragging)
        {
            Debug.Log("OnEndDrag called but isDragging=false, ignoring");
            return;
        }

        isDragging = false;

        if (activeClone != null)
        {
            ExecuteEvents.Execute<IEndDragHandler>(activeClone, eventData, ExecuteEvents.endDragHandler);
            Destroy(activeClone);
            activeClone = null;
            return;
        }

        RestorePostDragState();

        if (!TryGetTargetSlot(eventData, out ItemSlot targetSlot))
        {
            HandleInvalidDrop();
            return;
        }

        if (targetSlot.IsOccupied())
        {
            HandleOccupiedSlot();
            return;
        }

        if (soldierPrefab != null)
        {
            SpawnSoldierInSlot(targetSlot);
            return;
        }

        PlaceExistingObjectInSlot(targetSlot);
    }

    private void RestorePostDragState()
    {
        if (isUIElement)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
            }
            return;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (cachedSpriteRenderer != null)
        {
            cachedSpriteRenderer.sortingOrder -= 100;
        }
    }

    private bool TryGetTargetSlot(PointerEventData eventData, out ItemSlot targetSlot)
    {
        return isUIElement
            ? TryGetTargetSlotFromUI(eventData, out targetSlot)
            : TryGetTargetSlotFromWorld(eventData, out targetSlot);
    }

    private bool TryGetTargetSlotFromUI(PointerEventData eventData, out ItemSlot targetSlot)
    {
        targetSlot = null;
        if (EventSystem.current == null)
        {
            return false;
        }

        if (reusablePointerEventData == null || reusablePointerEventSystem != EventSystem.current)
        {
            reusablePointerEventData = new PointerEventData(EventSystem.current);
            reusablePointerEventSystem = EventSystem.current;
        }

        reusablePointerEventData.Reset();
        reusablePointerEventData.position = eventData.position;

        raycastResultsBuffer.Clear();
        EventSystem.current.RaycastAll(reusablePointerEventData, raycastResultsBuffer);

        foreach (RaycastResult result in raycastResultsBuffer)
        {
            ItemSlot slot = result.gameObject.GetComponentInParent<ItemSlot>();
            if (slot != null)
            {
                targetSlot = slot;
                return true;
            }
        }

        return false;
    }

    private bool TryGetTargetSlotFromWorld(PointerEventData eventData, out ItemSlot targetSlot)
    {
        targetSlot = null;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0f;

        Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);
        if (hitCollider != null)
        {
            targetSlot = hitCollider.GetComponentInParent<ItemSlot>();
            if (targetSlot != null)
            {
                return true;
            }
        }

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            targetSlot = hit.collider.GetComponentInParent<ItemSlot>();
            if (targetSlot != null)
            {
                return true;
            }
        }

        return false;
    }

    private void HandleOccupiedSlot()
    {
        Debug.Log($"Slot is already occupied - cannot place item here");
        parentSource?.onCancelledOrInvalidDrop?.Invoke();

        ShopSpawnInfo shopInfo = cachedShopSpawnInfo;
        if (shopInfo != null && shopInfo.shop != null)
        {
            shopInfo.shop.IncrementSoldierByIndex(shopInfo.slotIndex);
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    private void SpawnSoldierInSlot(ItemSlot targetSlot)
    {
        Vector3 spawnPosition = targetSlot.transform.position;
        spawnPosition.z = 0f;

        GameObject spawnedSoldier = Instantiate(soldierPrefab, spawnPosition, Quaternion.identity);
        spawnedSoldier.transform.SetParent(targetSlot.transform, true);
        spawnedSoldier.transform.localRotation = Quaternion.identity;
        spawnedSoldier.transform.localScale = Vector3.one;

        FitVisualChildToSlot(spawnedSoldier, targetSlot);

        Rigidbody2D rb = spawnedSoldier.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        SoldierBehaviour soldierBehaviour = spawnedSoldier.GetComponent<SoldierBehaviour>();
        if (soldierBehaviour != null)
        {
            soldierBehaviour.tier = soldierTier;
            if (soldierBehaviour.spriteRenderer != null)
            {
                soldierBehaviour.spriteRenderer.color = Color.white;
            }
        }

        SpriteOutlineMapRenderer enchantController = spawnedSoldier.GetComponent<SpriteOutlineMapRenderer>();
        if (enchantController == null)
        {
            enchantController = spawnedSoldier.GetComponentInChildren<SpriteOutlineMapRenderer>();
        }

        if (enchantController != null)
        {
            bool shouldEnchant = soldierTier != SoldierTierList.TierEnum.Common;
            enchantController.SetEnchantColor(soldierColor);
            enchantController.SetEnchantEnabled(shouldEnchant);
        }
        else
        {
            Debug.LogWarning($"No SpriteOutlineMapRenderer found on '{spawnedSoldier.name}'. No rarity enchant was applied.");
        }

        if (DifficultyManager.instance != null)
        {
            DifficultyManager.instance.OnSoldierPlaced();
        }

        targetSlot.SetOccupied(spawnedSoldier);
        MarkParentSourceAsUsed();
        parentSource?.onSuccessfulDrop?.Invoke();

        Destroy(gameObject);
    }

    private void PlaceExistingObjectInSlot(ItemSlot targetSlot)
    {
        if (isUIElement)
        {
            PlaceUIObjectInSlot(targetSlot);
            return;
        }

        PlaceWorldObjectInSlot(targetSlot);
    }

    private void PlaceUIObjectInSlot(ItemSlot targetSlot)
    {
        RectTransform slotRect = targetSlot.GetComponent<RectTransform>();
        if (slotRect == null)
        {
            HandleInvalidDrop();
            return;
        }

        transform.SetParent(slotRect, false);
        rectTransform.anchoredPosition = Vector2.zero;

        Image image = cachedImage;
        if (image != null && image.sprite != null)
        {
            Vector2 slotSize = slotRect.rect.size;
            Vector2 spriteSize = new Vector2(image.sprite.rect.width, image.sprite.rect.height);

            float scaleX = (slotSize.x * SlotPaddingFactor) / spriteSize.x;
            float scaleY = (slotSize.y * SlotPaddingFactor) / spriteSize.y;
            float scaleFactor = Mathf.Min(scaleX, scaleY);

            rectTransform.localScale = Vector3.one * scaleFactor;
        }

        SetImageAlphaToOpaque(image);

        targetSlot.SetOccupied(gameObject);
        MarkParentSourceAsUsed();
        parentSource?.onSuccessfulDrop?.Invoke();

#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject == gameObject)
        {
            UnityEditor.Selection.activeObject = null;
        }
#endif
        Destroy(gameObject);
    }

    private void PlaceWorldObjectInSlot(ItemSlot targetSlot)
    {
        Vector3 slotWorldPos = targetSlot.transform.position;
        slotWorldPos.z = transform.position.z;
        transform.position = slotWorldPos;
        transform.SetParent(targetSlot.transform);

        RectTransform slotRect = targetSlot.GetComponent<RectTransform>();
        SpriteRenderer spriteRenderer = cachedSpriteRenderer;

        if (slotRect != null && spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Vector2 slotSize = slotRect.rect.size;
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

            float scaleX = (slotSize.x * SlotPaddingFactor) / spriteSize.x;
            float scaleY = (slotSize.y * SlotPaddingFactor) / spriteSize.y;
            float scaleFactor = Mathf.Min(scaleX, scaleY);

            transform.localScale = Vector3.one * scaleFactor;
        }

        SetSpriteAlphaToOpaque(spriteRenderer);

        targetSlot.SetOccupied(gameObject);
        MarkParentSourceAsUsed();
        parentSource?.onSuccessfulDrop?.Invoke();

#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject == gameObject)
        {
            UnityEditor.Selection.activeObject = null;
        }
#endif
        Destroy(gameObject);
    }

    private void HandleInvalidDrop()
    {
        ShopSpawnInfo info = cachedShopSpawnInfo;
        if (info != null && info.shop != null)
        {
            info.shop.IncrementSoldierByIndex(info.slotIndex);
            Destroy(info.gameObject);
            return;
        }

        parentSource?.onCancelledOrInvalidDrop?.Invoke();
        Destroy(gameObject);
    }

    private void MarkParentSourceAsUsed()
    {
        if (parentSource != null && parentSource.oneTimeUse)
        {
            parentSource.hasBeenUsed = true;
            Debug.Log($"Parent source '{parentSource.gameObject.name}' marked as used (one-time only)");
        }
    }

    private static void SetImageAlphaToOpaque(Image image)
    {
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = 1f;
        image.color = color;
    }

    private static void SetSpriteAlphaToOpaque(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;
    }
}