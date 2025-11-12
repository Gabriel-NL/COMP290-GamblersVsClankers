
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavingSystem : MonoBehaviour
{
    [Header("Collection root")]
    public Transform slotsParent; // <-- target parent exposed in Inspector
    [System.Serializable]
    public class SoldierDataForSerialization
    {
        public string soldierType;
        public string tier;

        public SoldierDataForSerialization(string newSoldier,string newTier)
        {
            soldierType=newSoldier;
            tier = newTier;
        }
    }
    public List<SoldierDataForSerialization> datanew = new List<SoldierDataForSerialization>();
    public void SaveData()
    {
        GridBuilder<ItemSlot> gridBuilder = new GridBuilder<ItemSlot>(slotsParent);
        GridSerializer serializer = new GridSerializer();
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        
        var meta = new Dictionary<string, string>
        {
            { "points",scoreManager.GetCurrentScore().ToString() }
        };
        var grid = new Dictionary<(int, int), SoldierDataForSerialization>();
        
        foreach (KeyValuePair<(int, int), ItemSlot> keyValuePair in gridBuilder.customGrid)
        {
            if (keyValuePair.Value==null|| keyValuePair.Value.occupyingObject==null)
            {
                continue;
            }
            if (keyValuePair.Value.occupyingObject.TryGetComponent<SoldierBehaviour>(out SoldierBehaviour soldierBehaviour))
            {
                
                string type = soldierBehaviour.name;
                string tier =soldierBehaviour.tier.ToString();
                SoldierDataForSerialization entry = new SoldierDataForSerialization(type,tier);
                grid.Add(keyValuePair.Key,entry );
 
            }
        }
        
        string json = serializer.ToJson(grid, meta);
        Debug.Log(json);
    }

    void OnSceneUnloaded(Scene scene)
        {
            // Sua função aqui
            Debug.Log($"Cena descarregada: {scene.name}");
            SaveData();
        }
        void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
    
    
    
}
