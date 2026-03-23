using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public Canvas canvas;

    [Header("Prefab to instantiate when dropped")]
    public GameObject soldierPrefab;

    [Header("Placed Visual Fit")]
    [Tooltip("Placed soldier visual will fit within this multiplier of the cell size. 1.0 = cell size, 1.1 = 110% of cell size.")]
    public float placedVisualFitMultiplier = 1.1f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 pointerOffset;
    private bool isUIElement;
    private Vector3 worldOffset;
    private Camera mainCamera;

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

    public Action onBeginDragFromSource;
    public Action onCancelledOrInvalidDrop;
    public Action onSuccessfulDrop;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        isUIElement = rectTransform != null;
        mainCamera = Camera.main;

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
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    boxCol.size = sr.sprite.bounds.size;
                }
            }
        }
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
        Vector3[] corners = new Vector3[4];
        rectTransformToMeasure.GetWorldCorners(corners);

        float width = Vector3.Distance(corners[0], corners[3]);
        float height = Vector3.Distance(corners[0], corners[1]);

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
            Image srcImage = GetComponent<Image>();
            Sprite sprite = srcImage != null ? srcImage.sprite : sourceSprite;
            Color color = srcImage != null ? srcImage.color : sourceColor;

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
            SpriteRenderer srcSr = GetComponent<SpriteRenderer>();
            Sprite sprite = srcSr != null ? srcSr.sprite : sourceSprite;
            Color color = srcSr != null ? srcSr.color : sourceColor;

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

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder += 100;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");

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

        if (isUIElement)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
            }
        }
        else
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder -= 100;
            }
        }

        ItemSlot targetSlot = null;

        if (isUIElement && EventSystem.current != null)
        {
            PointerEventData pe = new PointerEventData(EventSystem.current)
            {
                position = eventData.position
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pe, results);

            Debug.Log($"UI Raycast found {results.Count} results");

            foreach (RaycastResult r in results)
            {
                Debug.Log($"Raycast hit: {r.gameObject.name} (distance: {r.distance})");

                ItemSlot slot = r.gameObject.GetComponentInParent<ItemSlot>();
                if (slot != null)
                {
                    targetSlot = slot;
                    Debug.Log($"Found UI ItemSlot: {slot.gameObject.name}");
                    break;
                }
                else
                {
                    Debug.Log($"No ItemSlot found on {r.gameObject.name} or its parents");
                }
            }
        }

        if (!isUIElement)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0f;

            Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);
            if (hitCollider != null)
            {
                ItemSlot slot = hitCollider.GetComponentInParent<ItemSlot>();
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
                    ItemSlot slot = hit.collider.GetComponentInParent<ItemSlot>();
                    if (slot != null)
                    {
                        targetSlot = slot;
                        Debug.Log($"Found world ItemSlot via Raycast: {slot.gameObject.name}");
                    }
                }
            }

            if (targetSlot == null)
            {
                Debug.Log($"No ItemSlot found at world position: {worldPos}");
            }
        }

        if (targetSlot != null)
        {
            if (targetSlot.IsOccupied())
            {
                Debug.Log($"Slot {targetSlot.gameObject.name} is already occupied - cannot place soldier here");

                if (parentSource != null)
                {
                    parentSource.onCancelledOrInvalidDrop?.Invoke();
                }

                ShopSpawnInfo shopInfo = GetComponent<ShopSpawnInfo>();
                if (shopInfo != null && shopInfo.shop != null)
                {
                    shopInfo.shop.IncrementSoldierByIndex(shopInfo.slotIndex);
                    Destroy(gameObject);
                    return;
                }

                Debug.Log("Item will remain in place (slot occupied)");
                Destroy(gameObject);
                return;
            }

            if (soldierPrefab != null)
            {
                Vector3 spawnPosition = targetSlot.transform.position;
                spawnPosition.z = 0f;
                Transform parentTransform = targetSlot.transform;

                GameObject spawnedSoldier = Instantiate(soldierPrefab, spawnPosition, Quaternion.identity);
                spawnedSoldier.transform.SetParent(parentTransform, true);
                spawnedSoldier.transform.localRotation = Quaternion.identity;
                spawnedSoldier.transform.localScale = Vector3.one;

                FitVisualChildToSlot(spawnedSoldier, targetSlot);

                Rigidbody2D rb = spawnedSoldier.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.linearVelocity = Vector2.zero;
                }

                Debug.Log($"Set placed soldier root '{spawnedSoldier.name}' localScale to {spawnedSoldier.transform.localScale}");

                SoldierBehaviour soldierBehaviour = spawnedSoldier.GetComponent<SoldierBehaviour>();
                if (soldierBehaviour != null)
                {
                    soldierBehaviour.tier = soldierTier;
                    Debug.Log($"Applied tier {soldierTier} to spawned soldier '{spawnedSoldier.name}'");

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

                    Debug.Log($"Applied enchant color {soldierColor} to soldier '{spawnedSoldier.name}', enabled={shouldEnchant}");
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

                if (parentSource != null && parentSource.oneTimeUse)
                {
                    parentSource.hasBeenUsed = true;
                    Debug.Log($"Parent source '{parentSource.gameObject.name}' marked as used (one-time only)");
                }

                if (parentSource != null)
                {
                    parentSource.onSuccessfulDrop?.Invoke();
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

                        Image image = GetComponent<Image>();
                        if (image != null && image.sprite != null)
                        {
                            Vector2 slotSize = slotRect.rect.size;
                            Vector2 spriteSize = new Vector2(image.sprite.rect.width, image.sprite.rect.height);

                            float paddingFactor = 0.95f;
                            float scaleX = (slotSize.x * paddingFactor) / spriteSize.x;
                            float scaleY = (slotSize.y * paddingFactor) / spriteSize.y;
                            float scaleFactor = Mathf.Min(scaleX, scaleY);

                            rectTransform.localScale = Vector3.one * scaleFactor;

                            Debug.Log($"Scaled UI element by {scaleFactor} to fit slot (slot: {slotSize}, sprite: {spriteSize})");

                            Color color = image.color;
                            color.a = 1.0f;
                            image.color = color;
                        }
                        else
                        {
                            if (image != null)
                            {
                                Color color = image.color;
                                color.a = 1.0f;
                                image.color = color;
                            }
                        }

                        targetSlot.SetOccupied(gameObject);

                        if (parentSource != null && parentSource.oneTimeUse)
                        {
                            parentSource.hasBeenUsed = true;
                            Debug.Log($"Parent source '{parentSource.gameObject.name}' marked as used (one-time only)");
                        }

                        if (parentSource != null)
                        {
                            parentSource.onSuccessfulDrop?.Invoke();
                        }

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

                    RectTransform slotRect = targetSlot.GetComponent<RectTransform>();
                    SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                    if (slotRect != null && spriteRenderer != null && spriteRenderer.sprite != null)
                    {
                        Vector2 slotSize = slotRect.rect.size;
                        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

                        float paddingFactor = 0.95f;
                        float scaleX = (slotSize.x * paddingFactor) / spriteSize.x;
                        float scaleY = (slotSize.y * paddingFactor) / spriteSize.y;
                        float scaleFactor = Mathf.Min(scaleX, scaleY);

                        transform.localScale = Vector3.one * scaleFactor;

                        Debug.Log($"Scaled world sprite by {scaleFactor} to fit slot (slot: {slotSize}, sprite: {spriteSize})");

                        Color color = spriteRenderer.color;
                        color.a = 1.0f;
                        spriteRenderer.color = color;
                    }
                    else
                    {
                        if (spriteRenderer != null)
                        {
                            Color color = spriteRenderer.color;
                            color.a = 1.0f;
                            spriteRenderer.color = color;
                        }
                    }

                    targetSlot.SetOccupied(gameObject);

                    if (parentSource != null && parentSource.oneTimeUse)
                    {
                        parentSource.hasBeenUsed = true;
                        Debug.Log($"Parent source '{parentSource.gameObject.name}' marked as used (one-time only)");
                    }

                    if (parentSource != null)
                    {
                        parentSource.onSuccessfulDrop?.Invoke();
                    }

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

        ShopSpawnInfo info = GetComponent<ShopSpawnInfo>();
        if (info != null && info.shop != null)
        {
            info.shop.IncrementSoldierByIndex(info.slotIndex);
            Destroy(info.gameObject);
            return;
        }

        if (parentSource != null)
        {
            parentSource.onCancelledOrInvalidDrop?.Invoke();
        }

        Debug.Log("Item was not dropped on a valid slot and will remain in place");
        Destroy(gameObject);
    }
}