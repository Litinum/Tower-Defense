using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    [SerializeField] private bool useBotSimulator = false;

    public static GameLoop Instance;

    public bool isGameLoopRunning { get; private set; } = false;
    private GameObject endTile;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Grid.Instance.onFinishMapGeneration += StartGameLoop;
        endTile = Grid.Instance.endPoint;

        PlayerStats.Instance.onGameLost += EndGameLose;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameLoopRunning)
            return;
    }

    private void StartGameLoop()
    {
        isGameLoopRunning = true;
        this.transform.AddComponent<EnemySpawner>();

        if(useBotSimulator)
            this.transform.AddComponent<BotSimulator>();            // Bot Simulation

        ResetTowerCost();

        Debug.Log("Game Started"); 
    }

    void ResetTowerCost()
    {
        GameObject[] towerPrefabs;

        towerPrefabs = Resources.LoadAll<GameObject>("Structures");

        foreach (GameObject p in towerPrefabs)
        {
            TowerBehaviour towerBehaviour = p.GetComponent<TowerBehaviour>();
            towerBehaviour?.ResetTowerCost();
        }
    }

    private void EndGameLose()
    {
        Time.timeScale = 0;
    }

    private void OnDisable()
    {
        Grid.Instance.onFinishMapGeneration -= StartGameLoop;
    }
}
