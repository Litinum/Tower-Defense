using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using AYellowpaper.SerializedCollections;

public class MainGame : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private GameObject GameLostPanel;
    [SerializedDictionary("Structure", "TextVar")] public SerializedDictionary<GameObject, TMP_Text> structureCostDict;

    private BuildingSystem buildingSystem;
    private PlayerStats playerStats;
    private Dictionary<GameObject, TMP_Text> dict;

    void Start()
    {
        buildingSystem = BuildingSystem.Instance;
        playerStats = PlayerStats.Instance;

        playerStats.onHealthChange += UpdateHealthText;
        playerStats.onGoldChange += UpdateGoldText;
        playerStats.onGameLost += ShowGameLost;
    }

    void Update()
    {
        UpdateStructureCost();
    }

    public void SetStructureToBuild(GameObject structureToBuild)
    {
        buildingSystem.SetBuildStructure(structureToBuild);
    }

    public void UpdateHealthText(int newHealth)
    {
        healthText.text = newHealth.ToString();
        Debug.Log("Health Text Changed");
    }

    public void UpdateGoldText(int newGold)
    {
        goldText.text = newGold.ToString() + " G";
        Debug.Log($"Gold Text Changed {newGold}");
    }

    public void ShowGameLost()
    {
        GameLostPanel.SetActive(true);
    }

    public void UpdateStructureCost()
    {
        if (structureCostDict.Count == 0)
            return;

        foreach(GameObject structure in structureCostDict.Keys)
        {
            TowerBehaviour towerBehaviour = structure.GetComponent<TowerBehaviour>();
            structureCostDict[structure].text = (towerBehaviour?.GetBuildCost()).ToString() + " G";
        }
    }

}
