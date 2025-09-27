using System.Collections;
using System.Collections.Generic;
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

    private void InitializeShop()
    {
        for (int i = 0; i < shopSlots.Count; i++)
        {
            shopSlots[i].sprite = shopSoldiers[i].characterSprite;
            //shopSlots[i].GetComponent<Button>().onClick.AddListener(() => OnSoldierClick(shopSoldiers[i].name));
        }
    }

    private float CalculateCosts(string soldierName)
    {
        for (int i = 0; i < shopSoldiers.Count; i++)
        {
            switch (soldierName)
            {
                case "Pistol":
                    if (shopSoldiers[i].type == SoldierType.TypeOfSoldier.Pistol)
                        return shopSoldiers[i].cost;
                    break;
                case "Rifleman":
                    if (shopSoldiers[i].type == SoldierType.TypeOfSoldier.Rifleman)
                        return shopSoldiers[i].cost;
                    break;
                case "ARSoldier":
                    if (shopSoldiers[i].type == SoldierType.TypeOfSoldier.ARSoldier)
                        return shopSoldiers[i].cost;
                    break;
                case "LaserMan":
                    if (shopSoldiers[i].type == SoldierType.TypeOfSoldier.LaserMan)
                        return shopSoldiers[i].cost;
                    break;
                default:
                    Debug.Log("Unknown soldier type: " + soldierName);
                    return 0f;
            }
        }
        Debug.Log("Soldier type not found: " + soldierName);
        return 0f;
    }

    public void OnSoldierClick(string soldierName)
    {
        float cost = CalculateCosts(soldierName);

        // if(/*add overall funds here*/ > cost)
        // {
        //     // Deduct cost from overall funds
        //     // overallFunds -= cost;
        // }
        // else
        // {
        //     Debug.Log("Not enough funds to purchase " + soldierName);
        // }
        Debug.Log("Button Clicked: " + soldierName + " | Cost: " + cost);
    }
}
