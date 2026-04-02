using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopINIT : MonoBehaviour
{
    [SerializeField] List<Image> shopSlots = new List<Image>();
    [SerializeField] List<SoldierType> shopSoldiers = new List<SoldierType>();
    [SerializeField] List<TMP_Text> purchasedTexts = new List<TMP_Text>();

    private HashSet<int> suppressedNextClick = new HashSet<int>();

    void Start()
    {
        InitializeShop();
        AudioManager.Play("GameplayMusic");
    }

    private TMP_Text GetSlotText(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < purchasedTexts.Count && purchasedTexts[slotIndex] != null)
        {
            return purchasedTexts[slotIndex];
        }

        if (slotIndex >= 0 && slotIndex < shopSlots.Count)
        {
            Transform slotTransform = shopSlots[slotIndex].transform;
            if (slotTransform.childCount > 0)
            {
                Transform firstChild = slotTransform.GetChild(0);
                TMP_Text tmpText = firstChild.GetComponent<TMP_Text>();
                if (tmpText != null)
                {
                    return tmpText;
                }
            }

            return slotTransform.GetComponentInChildren<TMP_Text>();
        }

        return null;
    }

    private void SetSlotQuantity(int slotIndex, int delta)
    {
        TMP_Text tmpText = GetSlotText(slotIndex);
        if (tmpText == null)
        {
            return;
        }

        if (!int.TryParse(tmpText.text, out int current))
        {
            current = 0;
        }

        tmpText.text = Mathf.Max(0, current + delta).ToString();
    }

    private void ConfigureDragStarter(Button button, int slotIndex)
    {
        ShopDragStarter dragStarter = button.GetComponent<ShopDragStarter>();
        if (dragStarter == null)
        {
            dragStarter = button.gameObject.AddComponent<ShopDragStarter>();
        }

        dragStarter.shop = this;
        dragStarter.slotIndex = slotIndex;
    }

    private bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < shopSoldiers.Count;
    }

    private void InitializeShop()
    {
        for (int i = 0; i < Mathf.Min(shopSlots.Count, shopSoldiers.Count); i++)
        {
            shopSlots[i].sprite = shopSoldiers[i].characterSprite;

            Button btn = shopSlots[i].GetComponent<Button>();
            if (btn == null)
            {
                continue;
            }

            ConfigureDragStarter(btn, i);

            if (btn.onClick.GetPersistentEventCount() > 0)
            {
                continue;
            }

            btn.onClick.RemoveAllListeners();
            int slotIndex = i;
            btn.onClick.AddListener(() => OnSoldierClick(shopSoldiers[slotIndex].name, slotIndex));
        }
    }

    public void SuppressNextClick(int slotIndex)
    {
        suppressedNextClick.Add(slotIndex);
    }

    public void DecrementSoldierByIndex(int slotIndex)
    {
        SetSlotQuantity(slotIndex, -1);
    }

    private float CalculateCosts(string soldierName)
    {
        for (int i = 0; i < shopSoldiers.Count; i++)
        {
            if (shopSoldiers[i].name == soldierName)
            {
                return shopSoldiers[i].stats.cost;
            }
        }

        Debug.LogWarning("Soldier type not found: " + soldierName);
        return 0f;
    }

    public void OnSoldierClick(string soldierName, int slotIndex)
    {
        float cost = CalculateCosts(soldierName);

        if (suppressedNextClick.Contains(slotIndex))
        {
            suppressedNextClick.Remove(slotIndex);
            Debug.Log($"Suppressed single click for slot {slotIndex} after hold/repeat.");
            return;
        }

        int costInt = Mathf.CeilToInt(cost);
        bool bought = false;

        if (ScoreManager.instance != null)
        {
            bought = ScoreManager.instance.TrySpend(costInt);
        }
        else
        {
            Debug.LogWarning("ScoreManager.instance not found — allowing purchase for testing.");
            bought = true;
        }

        if (!bought)
        {
            AudioManager.Play("InsufficientFunds");
            Debug.Log($"Not enough funds to purchase {soldierName}. Cost: {costInt}");
            return;
        }

        SetSlotQuantity(slotIndex, 1);

        Debug.Log("Button Clicked: " + soldierName + " | Cost: " + cost + " | Slot: " + slotIndex);
    }

    public void OnSoldierClickByIndex(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            Debug.LogWarning("Slot index out of range: " + slotIndex);
            return;
        }

        string soldierName = shopSoldiers[slotIndex].name;
        OnSoldierClick(soldierName, slotIndex);
    }

    public void OnSoldierClickByName(string soldierName)
    {
        int index = shopSoldiers.FindIndex(s => s.name == soldierName);
        if (index == -1)
        {
            Debug.LogWarning("Soldier name not found: " + soldierName);
            return;
        }

        OnSoldierClick(soldierName, index);
    }

    public GameObject TrySpawnOwnedSoldierForDrag(int slotIndex, PointerEventData eventData)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            Debug.LogWarning("Slot index out of range: " + slotIndex);
            return null;
        }

        TMP_Text tmpText = GetSlotText(slotIndex);
        if (tmpText == null)
        {
            Debug.LogWarning("No stock text found for slot " + slotIndex);
            return null;
        }

        if (!int.TryParse(tmpText.text, out int currentOwned) || currentOwned <= 0)
        {
            Debug.Log("No owned stock available to drag for slot " + slotIndex);
            return null;
        }

        return SpawnSoldierForDrag(slotIndex, eventData);
    }

    public GameObject SpawnSoldierForDrag(int slotIndex, PointerEventData eventData)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            Debug.LogWarning("Slot index out of range for SpawnSoldierForDrag: " + slotIndex);
            return null;
        }

        TMP_Text tmpTextCheck = GetSlotText(slotIndex);
        if (tmpTextCheck != null && int.TryParse(tmpTextCheck.text, out int currentCheck) && currentCheck <= 0)
        {
            Debug.Log("No stock to spawn for slot " + slotIndex);
            return null;
        }

        SoldierType soldier = shopSoldiers[slotIndex];
        if (soldier == null || soldier.characterSprite == null)
        {
            Debug.LogWarning("Soldier or sprite missing for slot " + slotIndex);
            return null;
        }

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Canvas canvas = null;

        if (canvases != null && canvases.Length > 0)
        {
            foreach (Canvas c in canvases)
            {
                if (c.rootCanvas == c)
                {
                    canvas = c;
                    break;
                }
            }

            if (canvas == null)
            {
                int bestOrder = int.MinValue;
                foreach (Canvas c in canvases)
                {
                    int order = c.sortingOrder;
                    if (order > bestOrder)
                    {
                        bestOrder = order;
                        canvas = c;
                    }
                }
            }
        }

        if (canvas == null)
        {
            Debug.LogWarning("No Canvas found in scene to spawn draggable soldier.");
            return null;
        }

        Canvas dragCanvas = null;
        GameObject existingDragCanvas = GameObject.Find("DragCanvas");
        if (existingDragCanvas != null)
        {
            dragCanvas = existingDragCanvas.GetComponent<Canvas>();
        }
        else
        {
            GameObject dc = new GameObject("DragCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            dragCanvas = dc.GetComponent<Canvas>();
            dragCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            dragCanvas.sortingOrder = 1000;
            dc.transform.SetParent(null);
        }

        Canvas targetCanvas = dragCanvas != null ? dragCanvas : canvas;

        GameObject go = new GameObject(
            soldier.name + "_Drag",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        RectTransform rt = go.GetComponent<RectTransform>();
        go.transform.SetParent(targetCanvas.transform, false);

        Image img = go.GetComponent<Image>();
        img.sprite = soldier.characterSprite;
        img.raycastTarget = true;

        DragDrop drag = go.AddComponent<DragDrop>();
        drag.canvas = targetCanvas;

        if (soldier.soldierPrefab != null)
        {
            drag.soldierPrefab = soldier.soldierPrefab;
            Debug.Log($"Assigned soldierPrefab '{soldier.soldierPrefab.name}' to shop drag for {soldier.name}");
        }
        else
        {
            Debug.LogWarning($"No soldierPrefab assigned to SoldierType '{soldier.name}' - drop will only place sprite!");
        }

        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = go.AddComponent<CanvasGroup>();
        }

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        Vector2 screenPos = eventData.position;
        RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
            out Vector2 localPoint))
        {
            rt.anchoredPosition = localPoint;
        }
        else
        {
            rt.anchoredPosition = Vector2.zero;
        }

        go.transform.SetAsLastSibling();
        Debug.Log("Spawned draggable soldier: " + go.name + " (set as last sibling)");

        DecrementSoldierByIndex(slotIndex);

        ShopSpawnInfo info = go.AddComponent<ShopSpawnInfo>();
        info.slotIndex = slotIndex;
        info.shop = this;
        return go;
    }

    public void IncrementSoldierByIndex(int slotIndex)
    {
        SetSlotQuantity(slotIndex, 1);
    }
}