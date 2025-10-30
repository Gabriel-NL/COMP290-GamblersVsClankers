using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopINIT : MonoBehaviour
{
    [SerializeField] List<Image> shopSlots = new List<Image>();
    [SerializeField] List<SoldierType> shopSoldiers = new List<SoldierType>();
    [SerializeField] List<TMP_Text> purchasedTexts = new List<TMP_Text>(); // Optional: manually assign purchased text components
    [Header("Economy")]
    [Tooltip("Sound to play when player doesn't have enough money")]
    public AudioClip insufficientFundsClip;

    private AudioSource audioSource;

    //List<float> soldierCosts = new List<float>();
    void Start()
    {
        InitializeShop();
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        AudioManager.Play("GameplayMusic");
    }

    // track suppressed clicks per slot to avoid a single click firing after a hold
    private HashSet<int> suppressedNextClick = new HashSet<int>();

    // Helper method to get the text component for a slot
    private TMP_Text GetSlotText(int slotIndex)
    {
        // First check if we have a manually assigned text component
        if (slotIndex >= 0 && slotIndex < purchasedTexts.Count && purchasedTexts[slotIndex] != null)
        {
            return purchasedTexts[slotIndex];
        }
        
        // If no manual assignment, try to get the first child's TMP_Text component
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
            
            // Fallback: search all children for any TMP_Text
            return slotTransform.GetComponentInChildren<TMP_Text>();
        }
        
        return null;
    }

    private void InitializeShop()
    {
        Debug.Log($"InitializeShop started - shopSlots.Count: {shopSlots.Count}, shopSoldiers.Count: {shopSoldiers.Count}");
        for (int i = 0; i < shopSlots.Count; i++)
        {
            // guard in case lists are different sizes
            if (i >= shopSoldiers.Count) continue;
            
            Debug.Log($"Initializing slot {i}: {shopSlots[i]?.gameObject.name}");

            shopSlots[i].sprite = shopSoldiers[i].characterSprite;
            // capture the loop variable for the listener
            int idx = i;
            var btn = shopSlots[i].GetComponent<Button>();
            if (btn != null)
            {
                // Diagnostics: report any persistent (inspector) listeners that exist on this Button
                int persistentCount = btn.onClick.GetPersistentEventCount();
                if (persistentCount > 0)
                {
                    Debug.Log($"Button '{btn.gameObject.name}' has {persistentCount} persistent OnClick listeners configured in the Inspector. Skipping adding a runtime listener to avoid duplicates.");
                    for (int p = 0; p < persistentCount; p++)
                    {
                        var target = btn.onClick.GetPersistentTarget(p);
                        var methodName = btn.onClick.GetPersistentMethodName(p);
                        Debug.Log($"  Persistent[{p}] target={target} method={methodName}");
                    }
                    // Even if persistent listeners exist, ensure a HoldClickRepeater exists so hold works.
                    var repeaterExisting = btn.gameObject.GetComponent<HoldClickRepeater>();
                    if (repeaterExisting == null) repeaterExisting = btn.gameObject.AddComponent<HoldClickRepeater>();
                    repeaterExisting.shop = this;
                    repeaterExisting.slotIndex = idx;
                }
                else
                {
                    // No inspector listeners — safe to add a single runtime listener.
                    // Clear any runtime listeners first to be safe, then add ours.
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnSoldierClick(shopSoldiers[idx].name, idx));
                    Debug.Log($"Added runtime listener for slot {idx} on Button '{btn.gameObject.name}'");

                    // Attach or configure HoldClickRepeater so press-and-hold repeats the click
                    var repeater = btn.gameObject.GetComponent<HoldClickRepeater>();
                    if (repeater == null) 
                    {
                        repeater = btn.gameObject.AddComponent<HoldClickRepeater>();
                        Debug.Log($"Added HoldClickRepeater to slot {idx} ({btn.gameObject.name})");
                    }
                    repeater.shop = this;
                    repeater.slotIndex = idx;
                    Debug.Log($"Configured HoldClickRepeater for slot {idx} - initialDelay: {repeater.initialDelay}s");
                }
            }
        }
    }

    // Called by HoldClickRepeater to suppress the next single click event after a hold
    public void SuppressNextClick(int slotIndex)
    {
        suppressedNextClick.Add(slotIndex);
    }

    // Decrement the displayed count for a slot down to a minimum of 0
    public void DecrementSoldierByIndex(int slotIndex)
    {
        var tmpText = GetSlotText(slotIndex);
        if (tmpText != null)
        {
            if (int.TryParse(tmpText.text, out int current))
            {
                current = Mathf.Max(0, current - 1);
                tmpText.text = current.ToString();
            }
            else
            {
                tmpText.text = "0";
            }
        }
    }

    private float CalculateCosts(string soldierName)
    {
        // Prefer direct name lookup — this is robust to different naming and order
        for (int i = 0; i < shopSoldiers.Count; i++)
        {
            if (shopSoldiers[i].name == soldierName)
                return shopSoldiers[i].stats.cost;
        }

        Debug.LogWarning("Soldier type not found: " + soldierName);
        return 0f;
    }

    // receives the clicked soldier name and the slot index so we can find child UI elements
    public void OnSoldierClick(string soldierName, int slotIndex)
    {
        float cost = CalculateCosts(soldierName);
        // If a HoldClickRepeater signalled a hold, suppress the next single click (to avoid a final increment)
        if (suppressedNextClick.Contains(slotIndex))
        {
            suppressedNextClick.Remove(slotIndex);
            Debug.Log($"Suppressed single click for slot {slotIndex} after hold/repeat.");
            return;
        }
        // On a regular click, attempt to purchase one: check funds first via ScoreManager
        int costInt = Mathf.CeilToInt(cost);
        bool bought = false;
        if (ScoreManager.instance != null)
        {
            bought = ScoreManager.instance.TrySpend(costInt);
        }
        else
        {
            // If ScoreManager is missing, allow the purchase but warn (helps testing)
            Debug.LogWarning("ScoreManager.instance not found — allowing purchase for testing.");
            bought = true;
        }

        if (!bought)
        {
            if (insufficientFundsClip != null && audioSource != null) audioSource.PlayOneShot(insufficientFundsClip);
            Debug.Log($"Not enough funds to purchase {soldierName}. Cost: {costInt}");
            return;
        }

        // Purchase succeeded — increment the slot counter in UI
        var tmpText = GetSlotText(slotIndex);
        if (tmpText != null)
        {
            if (int.TryParse(tmpText.text, out int current))
            {
                tmpText.text = (current + 1).ToString();
            }
            else
            {
                tmpText.text = "1";
            }
        }
        else
        {
            Debug.LogWarning($"No text component found in shop slot {slotIndex} to set quantity.");
        }

        // if(/*add overall funds here*/ > cost)
        // {
        //     // Deduct cost from overall funds
        //     // overallFunds -= cost;
        // }
        // else
        // {
        //     Debug.Log("Not enough funds to purchase " + soldierName);
        // }
        Debug.Log("Button Clicked: " + soldierName + " | Cost: " + cost + " | Slot: " + slotIndex);
    }

    // inspector-friendly wrapper: pass slot index from Button.OnClick (supports one parameter)
    public void OnSoldierClickByIndex(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= shopSoldiers.Count)
        {
            Debug.LogWarning("Slot index out of range: " + slotIndex);
            return;
        }
        string soldierName = shopSoldiers[slotIndex].name;
        OnSoldierClick(soldierName, slotIndex);
    }

    // optional: inspector-friendly wrapper that accepts a name string
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

    // Spawn a new UI Image that represents the soldier and make it draggable by the existing DragDrop script.
    // Returns the created GameObject (or null on failure).
    public GameObject SpawnSoldierForDrag(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= shopSoldiers.Count)
        {
            Debug.LogWarning("Slot index out of range for SpawnSoldierForDrag: " + slotIndex);
            return null;
        }

        // If the shop UI displays a counter for this slot and it's zero, don't allow spawning.
        var tmpTextCheck = GetSlotText(slotIndex);
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

        // Attempt to find the best Canvas in the scene to parent the UI element.
        // Prefer a top-level/root canvas or the one with the highest sort order so the spawned
        // draggable appears above nested shop UI elements.
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Canvas canvas = null;
        if (canvases != null && canvases.Length > 0)
        {
            // Prefer rootCanvas if present
            foreach (var c in canvases)
            {
                if (c.rootCanvas == c)
                {
                    canvas = c;
                    break;
                }
            }
            // otherwise pick canvas with highest sortingOrder (useful for ScreenSpaceCamera or Overlay)
            if (canvas == null)
            {
                int bestOrder = int.MinValue;
                foreach (var c in canvases)
                {
                    int order = 0;
                    var cc = c.GetComponent<Canvas>();
                    if (cc != null) order = cc.sortingOrder;
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

        // Ensure there's a dedicated DragCanvas at top layer so drags always appear above other UI.
        Canvas dragCanvas = GameObject.FindObjectOfType<Canvas>() as Canvas; // placeholder
        // Try to find an existing GameObject named "DragCanvas"
        var existingDragCanvas = GameObject.Find("DragCanvas");
        if (existingDragCanvas != null)
        {
            dragCanvas = existingDragCanvas.GetComponent<Canvas>();
        }
        else
        {
            // Create DragCanvas as ScreenSpaceOverlay and set a high sorting order
            GameObject dc = new GameObject("DragCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            dragCanvas = dc.GetComponent<Canvas>();
            dragCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            dragCanvas.sortingOrder = 1000; // high so it's above other canvases
            // Make sure it sits at root level
            dc.transform.SetParent(null);
        }

        // Create UI Image
        GameObject go = new GameObject(soldier.name + "_Drag", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        go.transform.SetParent(dragCanvas != null ? dragCanvas.transform : canvas.transform, false);

        var img = go.GetComponent<Image>();
        img.sprite = soldier.characterSprite;
        img.raycastTarget = true; // allow pointer events

        // Add DragDrop and wire Canvas reference
        var drag = go.AddComponent<DragDrop>();
        drag.canvas = canvas;
        
        // CRITICAL: Assign the soldier prefab so it can be instantiated when dropped
        if (soldier.soldierPrefab != null)
        {
            drag.soldierPrefab = soldier.soldierPrefab;
            Debug.Log($"Assigned soldierPrefab '{soldier.soldierPrefab.name}' to shop drag for {soldier.name}");
        }
        else
        {
            Debug.LogWarning($"No soldierPrefab assigned to SoldierType '{soldier.name}' - drop will only place sprite!");
        }

        // Ensure there's a CanvasGroup so raycasts can be toggled by DragDrop later if needed
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        // Default CanvasGroup state: visible and interactive until drag starts
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        // Position at current pointer (mouse/touch) so drag begins under the cursor
        Vector2 localPoint = Vector2.zero;
        Vector2 screenPos = Input.mousePosition;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out localPoint))
        {
            rt.anchoredPosition = localPoint;
        }
        else
        {
            rt.anchoredPosition = Vector2.zero;
        }

    // Ensure spawned drag object is on top of UI so it's not obscured by the shop slot
    go.transform.SetAsLastSibling();
    Debug.Log("Spawned draggable soldier: " + go.name + " (set as last sibling)");
        // Decrement the shop counter immediately since the player grabbed one
        DecrementSoldierByIndex(slotIndex);
        // Attach spawn metadata so drop handlers can return this to the shop counter if needed
        var info = go.AddComponent<ShopSpawnInfo>();
        info.slotIndex = slotIndex;
        info.shop = this;
        return go;
    }

    // Re-add one to the shop UI counter for a slot (inverse of DecrementSoldierByIndex)
    public void IncrementSoldierByIndex(int slotIndex)
    {
        var tmpText = GetSlotText(slotIndex);
        if (tmpText != null)
        {
            if (int.TryParse(tmpText.text, out int current))
            {
                tmpText.text = (current + 1).ToString();
            }
            else
            {
                tmpText.text = "1";
            }
        }
    }
}
