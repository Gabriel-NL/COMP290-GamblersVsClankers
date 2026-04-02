using UnityEngine;

[System.Serializable] 
public class PlayerSummon
{
    public string summonName;
    public Sprite summonSprite;
    [Min(0)] public int summonCost;
    public GameObject summonPrefab;
}
