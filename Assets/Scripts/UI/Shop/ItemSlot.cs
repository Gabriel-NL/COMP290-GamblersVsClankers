using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler
{
    private bool isOccupied = false;
    private GameObject occupyingObject = null;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Item dropped");
    }

    public bool IsOccupied()
    {
        // Also check if the occupying object still exists (wasn't destroyed)
        if (occupyingObject == null)
        {
            isOccupied = false;
        }
        return isOccupied;
    }

    public void SetOccupied(GameObject obj)
    {
        isOccupied = true;
        occupyingObject = obj;
        Debug.Log($"Slot {gameObject.name} is now occupied by {obj.name}");
    }

    public void ClearOccupied()
    {
        isOccupied = false;
        occupyingObject = null;
        Debug.Log($"Slot {gameObject.name} is now empty");
    }
}
