using UnityEngine;

[System.Serializable]
public class SoldierDataForSerialization
{
    public string soldierTypeName;    // Name of the SoldierType ScriptableObject
    public string soldierTierName;    // Name/Enum of the tier (e.g., "Common", "Uncommon", "Rare")

    // Optional persisted scale values (for backwards compatibility with older saves).
    public bool hasRootScale;
    public float rootScaleX = 1f;
    public float rootScaleY = 1f;
    public float rootScaleZ = 1f;

    public bool hasVisualScale;
    public float visualScaleX = 1f;
    public float visualScaleY = 1f;
    public float visualScaleZ = 1f;
    
    public SoldierDataForSerialization(string newSoldierTypeName, string newSoldierTierName)
    {
        soldierTypeName = newSoldierTypeName;
        soldierTierName = newSoldierTierName;
    }

    public void SetRootLocalScale(Vector3 scale)
    {
        hasRootScale = true;
        rootScaleX = scale.x;
        rootScaleY = scale.y;
        rootScaleZ = scale.z;
    }

    public Vector3 GetRootLocalScale()
    {
        return new Vector3(rootScaleX, rootScaleY, rootScaleZ);
    }

    public void SetVisualLocalScale(Vector3 scale)
    {
        hasVisualScale = true;
        visualScaleX = scale.x;
        visualScaleY = scale.y;
        visualScaleZ = scale.z;
    }

    public Vector3 GetVisualLocalScale()
    {
        return new Vector3(visualScaleX, visualScaleY, visualScaleZ);
    }
}