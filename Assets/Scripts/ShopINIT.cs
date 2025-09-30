using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopINIT : MonoBehaviour
{
    [SerializeField] List<Image> shopSlots = new List<Image>();
    [SerializeField] List<SoldierType> shopSoldiers = new List<SoldierType>();

    //List<float> soldierCosts = new List<float>();
    void Start()
    {
        InitializeShop();
    }

    // track suppressed clicks per slot to avoid a single click firing after a hold
    private HashSet<int> suppressedNextClick = new HashSet<int>();

    private void InitializeShop()
    {
        for (int i = 0; i < shopSlots.Count; i++)
        {
            // guard in case lists are different sizes
            if (i >= shopSoldiers.Count) continue;

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
                    if (repeater == null) repeater = btn.gameObject.AddComponent<HoldClickRepeater>();
                    repeater.shop = this;
                    repeater.slotIndex = idx;
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
        if (slotIndex < 0 || slotIndex >= shopSlots.Count) return;

        Transform slotTransform = shopSlots[slotIndex].transform;
        var tmpText = slotTransform.GetComponentInChildren<TMP_Text>();
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
            return;
        }

        var uiText = slotTransform.GetComponentInChildren<Text>();
        if (uiText != null)
        {
            if (int.TryParse(uiText.text, out int current))
            {
                current = Mathf.Max(0, current - 1);
                uiText.text = current.ToString();
            }
            else
            {
                uiText.text = "0";
            }
            return;
        }
    }

    private float CalculateCosts(string soldierName)
    {
        // Prefer direct name lookup — this is robust to different naming and order
        for (int i = 0; i < shopSoldiers.Count; i++)
        {
            if (shopSoldiers[i].name == soldierName)
                return shopSoldiers[i].cost;
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

        // Try to find a text component (TMPro or legacy Text) in the slot's children and increment it
        if (slotIndex >= 0 && slotIndex < shopSlots.Count)
        {
            Transform slotTransform = shopSlots[slotIndex].transform;

            // TMP first
            var tmpText = slotTransform.GetComponentInChildren<TMP_Text>();
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
                // fallback to legacy UI.Text
                var uiText = slotTransform.GetComponentInChildren<Text>();
                if (uiText != null)
                {
                    if (int.TryParse(uiText.text, out int current))
                    {
                        uiText.text = (current + 1).ToString();
                    }
                    else
                    {
                        uiText.text = "1";
                    }
                }
                else
                {
                    Debug.LogWarning($"No text component found in shop slot {slotIndex} to set quantity.");
                }
            }
        }
        else
        {
            Debug.LogWarning("Slot index out of range: " + slotIndex);
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
}
