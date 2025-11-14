[System.Serializable]
public class SoldierDataForSerialization
{
    public string soldierTypeName;    // Name of the SoldierType ScriptableObject
    public string soldierTierName;    // Name/Enum of the tier (e.g., "Common", "Uncommon", "Rare")
    
    public SoldierDataForSerialization(string newSoldierTypeName, string newSoldierTierName)
    {
        soldierTypeName = newSoldierTypeName;
        soldierTierName = newSoldierTierName;
    }
}