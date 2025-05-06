using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] public int gold { get; private set; } = 50;          // Used for buidling structures
    [SerializeField] public int health { get; private set; } = 100;      // Number of enemies that reached the end


    public static PlayerStats Instance;

    public delegate void OnHealthChange(int newHealthValue);
    public event OnHealthChange onHealthChange;
    public delegate void OnGoldChange(int newGoldValue);
    public event OnGoldChange onGoldChange;
    public delegate void OnGameLost();
    public event OnGameLost onGameLost;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        onGoldChange.Invoke(gold);
        onHealthChange.Invoke(health);
    }

    public void ReceiveGold(int goldToReceive)
    {
        gold += goldToReceive;
        onGoldChange.Invoke(gold);
    }

    public void SpendGold(int goldToSpend)
    {
        gold -= goldToSpend;
        onGoldChange.Invoke(gold);
    }

    public void HealthLoss()
    {
        health--;
        onHealthChange.Invoke(health);

        if (health == 0)
            onGameLost.Invoke();
    }


}
