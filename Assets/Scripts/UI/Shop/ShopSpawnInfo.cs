using UnityEngine;

// Simple component attached to spawned shop copies so Drop handlers can know which shop slot they came from.
public class ShopSpawnInfo : MonoBehaviour
{
    public int slotIndex;
    public ShopINIT shop;
}
